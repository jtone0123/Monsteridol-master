using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System; // LINQ�� ����ϱ� ���� �ʿ� (��: OrderBy)

// Idol.cs
// ���̵� ĳ������ �ൿ, ����, �����͸� �����ϴ� ���� Ŭ�����Դϴ�.
// ���� ��� �̵� ������ �����մϴ�.
[RequireComponent(typeof(IdolMovement))]
public class Idol : MonoBehaviour
{
    public enum IdolState
    {
        Idle,                   // ��� ����
        Wandering,              // ��ȸ ���� (���� �� ��������)
        MovingToScheduledTarget // �����ٿ� ���� ��ǥ �������� �̵� (���� �̵� ����)
    }

    [Header("���̵� �⺻ ����")]
    [Tooltip("���̵��� ���� �� ��ȣ�Դϴ�.")]
    public int currentFloorID = 1;

    [Header("���̵� ����")]
    [Tooltip("���� ���̵��� �����Դϴ�.")]
    public IdolState currentState = IdolState.Idle;

    [Header("��ȸ(Wandering) ����")]
    [Tooltip("��ȸ �� ���� ������ ���� �� ��� �ð� (�ּ�)")]
    public float minIdleTimeBeforeWander = 3f;
    [Tooltip("��ȸ �� ���� ������ ���� �� ��� �ð� (�ִ�)")]
    public float maxIdleTimeBeforeWander = 7f;

    //�ܺ� ����
    public bool bArrived = false;
    public Action OnArrivedAtTarget; // ���� �� ȣ��� �׼� (�ݹ�)

    // ���� ����
    private IdolMovement idolMovement;
    private float currentTimer = 0f;
    private float timeToWait = 0f;

    // ������ ����
    public NavPoint finalScheduledTarget = null;       // ������ �ý������κ��� ���� ���� ��ǥ
    private NavPoint intermediateTarget = null;         // ��� �Ա� �� �߰� ������
    // private bool needsToUseStairs = false; // �� ������ ���� DeterminePath���� �Ź� �Ǵ��ϹǷ� Ŭ���� ����� �ʿ�� ���� �� ����
    private bool isCurrentlyUsingStairsProcess = false; // ���� ����� '�̿�'�ϴ� ���� ���ΰ�? (�����̵� ó����)

    // NavPoint ���� (HubManager�� �̻���������, ���⼭�� ������ ó��)
    private static List<NavPoint> allNavPointsInScene = new List<NavPoint>();

    void Awake()
    {
        idolMovement = GetComponent<IdolMovement>();
        if (idolMovement == null)
        {
            Debug.LogError(gameObject.name + "���� IdolMovement ������Ʈ�� ã�� �� �����ϴ�!");
            enabled = false; // ��ũ��Ʈ ��Ȱ��ȭ
            return;
        }

        idolMovement.OnArrivedAtTargetPoint += HandleArrivalAtNavPoint;

        if (allNavPointsInScene.Count == 0)
        {
            NavPoint[] foundNavPoints = FindObjectsByType<NavPoint>(FindObjectsSortMode.None);
            if (foundNavPoints != null && foundNavPoints.Length > 0)
            {
                allNavPointsInScene.AddRange(foundNavPoints);
                Debug.Log(allNavPointsInScene.Count + "���� NavPoint�� ������ ã�ҽ��ϴ�.");
            }
        }
    }

    void Start()
    {
        SetNewWaitTimeForCurrentState();
    }

    void Update()
    {
        if (isCurrentlyUsingStairsProcess)
        {
            return;
        }

        currentTimer += Time.deltaTime;

        switch (currentState)
        {
            case IdolState.Idle:
                HandleIdleState();
                break;
            case IdolState.Wandering:
                HandleWanderingState();
                break;
            case IdolState.MovingToScheduledTarget:
                HandleMovingToScheduledTargetState();
                break;
        }
    }

    void HandleIdleState()
    {
        if (idolMovement.IsCurrentlyMoving()) idolMovement.StopMovement();

        if (currentTimer >= timeToWait)
        {
            ChangeState(IdolState.Wandering);
        }
    }

    void HandleWanderingState()
    {
        if (!idolMovement.IsCurrentlyMoving() && intermediateTarget == null)
        {
            NavPoint wanderTarget = GetRandomNavPointOnCurrentFloor();
            if (wanderTarget != null)
            {
                intermediateTarget = wanderTarget;
                idolMovement.MoveTo(intermediateTarget);
            }
            else
            {
                ChangeState(IdolState.Idle);
            }
        }
    }

    void HandleMovingToScheduledTargetState()
    {
        if (finalScheduledTarget == null)
        {
            ChangeState(IdolState.Idle);
            return;
        }

        if (!idolMovement.IsCurrentlyMoving() && intermediateTarget == null && !isCurrentlyUsingStairsProcess)
        {
            // ���� ��ǥ�� �̹� �����ߴ��� ���� Ȯ��
            if (currentFloorID == finalScheduledTarget.floorID && Vector2.Distance(transform.position, finalScheduledTarget.GetPosition()) < idolMovement.arrivalThreshold)
            {
                Debug.Log(gameObject.name + ": �̹� ���� ������ ��ǥ(" + finalScheduledTarget.pointName + ")�� �ſ� ������ �ְų� �����߽��ϴ�!");
                HandleArrivalAtFinalTarget();
                return;
            }
            bArrived = false; // ���� ���� �ʱ�ȭ
            DeterminePathToScheduledTarget();
        }
    }

    void DeterminePathToScheduledTarget()
    {
        if (finalScheduledTarget == null) return;

        if (finalScheduledTarget.floorID == currentFloorID)
        {
            // ��ǥ�� ���� ���� ����
            intermediateTarget = finalScheduledTarget;
            Debug.Log(gameObject.name + ": ���� ��ǥ(" + finalScheduledTarget.pointName + ")�� ���� " + currentFloorID + "���� �ֽ��ϴ�. �̵� ����.");
            idolMovement.MoveTo(intermediateTarget);
        }
        else
        {
            // ��ǥ�� �ٸ� ���� ����: ���� ������ ���� ��� Ž��
            Debug.Log(gameObject.name + ": ���� ��ǥ(" + finalScheduledTarget.pointName + ")�� " + finalScheduledTarget.floorID + "��. ����� " + currentFloorID + "��. ��� Ž��.");
            NavPoint nextStairEntrance = FindNextStairTowardsTargetFloor(finalScheduledTarget.floorID);

            if (nextStairEntrance != null)
            {
                intermediateTarget = nextStairEntrance;
                Debug.Log(gameObject.name + ": ���� ��� �Ա�(" + intermediateTarget.pointName + ", " + intermediateTarget.floorID + "��)�� �̵� ����. (��ǥ ��: " + finalScheduledTarget.floorID + ")");
                idolMovement.MoveTo(intermediateTarget);
            }
            else
            {
                Debug.LogWarning(gameObject.name + ": " + currentFloorID + "������ " + finalScheduledTarget.floorID + "�� �������� ���� ����� ã�� �� �����ϴ�! ������ �ߴ�. Idle ���·� ��ȯ.");
                finalScheduledTarget = null;
                ChangeState(IdolState.Idle);
            }
        }
    }

    NavPoint FindNextStairTowardsTargetFloor(int ultimateTargetFloorID)
    {
        List<NavPoint> candidateStairs = new List<NavPoint>();
        bool goingUp = ultimateTargetFloorID > currentFloorID;

        foreach (NavPoint navPoint in allNavPointsInScene)
        {
            if (navPoint.floorID == currentFloorID && navPoint.isStair && navPoint.connectedStairPoint != null)
            {
                if (goingUp && (navPoint.stairDirection == NavPoint.StairDirection.Up || navPoint.connectedStairPoint.floorID > currentFloorID))
                {
                    // �ö󰡴� ����̰ų�, ����� ������ ���� ������ ������ ���
                    candidateStairs.Add(navPoint);
                }
                else if (!goingUp && (navPoint.stairDirection == NavPoint.StairDirection.Down || navPoint.connectedStairPoint.floorID < currentFloorID))
                {
                    // �������� ����̰ų�, ����� ������ ���� ������ �Ʒ����� ���
                    candidateStairs.Add(navPoint);
                }
            }
        }

        if (candidateStairs.Count > 0)
        {
            // ���� ����� ������ ��� ����
            return candidateStairs.OrderBy(stair => Vector2.Distance(transform.position, stair.GetPosition())).FirstOrDefault();
        }
        return null;
    }

    void HandleArrivalAtNavPoint(NavPoint arrivedPoint)
    {
        if (arrivedPoint == null) return; // Ȥ�� �� null üũ

        // Debug.Log(gameObject.name + "��(��) " + arrivedPoint.pointName + " ("+ arrivedPoint.floorID +"��)�� ���� (Idol.cs���� �˸� ����)");

        if (currentState == IdolState.Wandering)
        {
            intermediateTarget = null;
            ChangeState(IdolState.Idle);
        }
        else if (currentState == IdolState.MovingToScheduledTarget)
        {
            if (arrivedPoint == intermediateTarget) // ���� ������ �߰� ��ǥ�� ����
            {
                if (arrivedPoint.isStair && arrivedPoint.connectedStairPoint != null && arrivedPoint.floorID == currentFloorID) // ���� ���� ��� �Ա��� ����
                {
                    // ��� �Ա��� ������ ���: �� ���� ó��
                    Debug.Log(gameObject.name + ": ��� �Ա�(" + arrivedPoint.pointName + ") ����. �� ���� ����.");
                    isCurrentlyUsingStairsProcess = true;

                    NavPoint exitStair = arrivedPoint.connectedStairPoint;
                    transform.position = exitStair.GetPosition();
                    currentFloorID = exitStair.floorID;
                    Debug.Log(gameObject.name + ": " + currentFloorID + "�� (" + exitStair.pointName + ")���� �̵� �Ϸ�.");

                    intermediateTarget = null; // �߰� ��ǥ(��� �Ա�) �Ϸ�
                    isCurrentlyUsingStairsProcess = false;

                    // ���ο� ���� ���������Ƿ�, ���� ��θ� ��� ���� (Update ������ ��ٸ��� �ʰ�)
                    // ���� ��ǥ�� �����ߴ���, �ƴϸ� �� �ٸ� ����� Ÿ�� �ϴ��� ��
                    if (finalScheduledTarget != null && currentFloorID == finalScheduledTarget.floorID)
                    {
                        // ���� ��ǥ�� ���� ���� ����������, ���� ��ǥ�� �ٷ� �̵� ����
                        Debug.Log(gameObject.name + ": ���� ��ǥ�� ���� ���� ����. ���� ��ǥ�� ��� ����.");
                        DeterminePathToScheduledTarget();
                    }
                    else if (finalScheduledTarget != null)
                    {
                        // ���� ���� ��ǥ ���� �ƴ�, ���� ��� ã�ƾ� ��
                        Debug.Log(gameObject.name + ": ���� ���� ��ǥ ���� �ƴ�. ���� ��� ����.");
                        DeterminePathToScheduledTarget();
                    }
                    else // ���� ��ǥ�� ���� �̻��� ��Ȳ (������ ��� ��)
                    {
                        ChangeState(IdolState.Idle);
                    }
                }
                else if (arrivedPoint == finalScheduledTarget)
                {
                    // ���� ��ǥ ������ ������ ���
                    HandleArrivalAtFinalTarget();
                }
                else
                {
                    // ����� �ƴ� �ٸ� �߰� ���� (���� ���������� �߻��ϱ� �����)
                    Debug.LogWarning(gameObject.name + ": ����ġ ���� �߰� ���� " + arrivedPoint.pointName + "�� ����. ���� ��ǥ Ȯ��.");
                    intermediateTarget = null;
                    // ��� �缳�� �õ�
                    if (finalScheduledTarget != null) DeterminePathToScheduledTarget(); else ChangeState(IdolState.Idle);
                }
            }
        }
    }

    void HandleArrivalAtFinalTarget()
    {
        Debug.Log(gameObject.name + ": ���� ������ ��ǥ(" + (finalScheduledTarget != null ? finalScheduledTarget.pointName : "�� �� ����") + ")�� ����!");
        finalScheduledTarget = null;
        intermediateTarget = null;
        bArrived = true; // ���� ���� �÷��� ����
        OnArrivedAtTarget?.Invoke(); // ���� �� ȣ��� �׼� (�ݹ�)
        ChangeState(IdolState.Idle);
    }


    void SetNewWaitTimeForCurrentState()
    {
        currentTimer = 0f;
        if (currentState == IdolState.Idle)
        {
            timeToWait = UnityEngine.Random.Range(minIdleTimeBeforeWander, maxIdleTimeBeforeWander);
        }
    }

    void ChangeState(IdolState newState)
    {
        if (currentState == newState && !(newState == IdolState.Idle && finalScheduledTarget != null))
        {
            if (currentState == newState && currentState != IdolState.Idle) return;
        }

        currentState = newState;
        SetNewWaitTimeForCurrentState();

        if (newState == IdolState.Idle || newState == IdolState.Wandering)
        {
            intermediateTarget = null;
        }
    }

    NavPoint GetRandomNavPointOnCurrentFloor()
    {
        List<NavPoint> navPointsOnFloor = allNavPointsInScene.Where(p => p.floorID == currentFloorID && !p.isStair).ToList();
        if (navPointsOnFloor.Count > 0)
        {
            int randomIndex = UnityEngine.Random.Range(0, navPointsOnFloor.Count);
            return navPointsOnFloor[randomIndex];
        }
        return null;
    }

    public void AssignScheduledTarget(NavPoint targetNavPoint)
    {
        if (targetNavPoint == null)
        {
            Debug.LogWarning(gameObject.name + ": �Ҵ�� ������ ��ǥ�� null�Դϴ�.");
            finalScheduledTarget = null;
            ChangeState(IdolState.Idle);
            return;
        }

        Debug.Log(gameObject.name + ": ���ο� ������ ��ǥ �Ҵ�� - " + targetNavPoint.pointName + " (" + targetNavPoint.floorID + "��)");
        finalScheduledTarget = targetNavPoint;
        intermediateTarget = null;
        ChangeState(IdolState.MovingToScheduledTarget);
    }

    void OnDisable()
    {
        if (idolMovement != null)
        {
            idolMovement.OnArrivedAtTargetPoint -= HandleArrivalAtNavPoint;
        }
    }
}
