using UnityEngine;
using System; // Action�� ����ϱ� ���� �ʿ�

// IdolMovement.cs
// ���̵� ĳ������ 2D �̵��� ����ϴ� Ŭ�����Դϴ�.
// Rigidbody2D�� ����Ͽ� ���� ��� �̵��� ó���մϴ�.
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class IdolMovement : MonoBehaviour
{
    [Header("�̵� ����")]
    [Tooltip("���̵��� �̵� �ӵ��Դϴ�.")]
    public float moveSpeed = 3f;

    [Tooltip("��ǥ ������ �󸶳� ��������� ������ ������ �������� �Ÿ��Դϴ�.")]
    public float arrivalThreshold = 0.1f;

    // ���� ���� ������
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    private Vector2 currentTargetPosition;
    private bool isMoving = false;

    // ��ǥ ���� ���� �� ȣ��� �׼�(�ݹ�)
    public Action<NavPoint> OnArrivedAtTargetPoint;
    private NavPoint currentTargetNavPointObject; // ���� �̵� ��ǥ NavPoint ��ü

    private Animator animator;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
    }

    void FixedUpdate()
    {
        if (isMoving)
        {
            HandleMovement();
        }
        else
        {
            if (rb.linearVelocity != Vector2.zero)
            {
                rb.linearVelocity = Vector2.zero;
            }
        }
    }

    private void HandleMovement()
    {
        // currentTargetNavPointObject�� null�ε� isMoving�� true�� ���������� ��Ȳ ����
        if (currentTargetNavPointObject == null && isMoving)
        {
            Debug.LogWarning(gameObject.name + " - IdolMovement: isMoving�� true������ currentTargetNavPointObject�� null�Դϴ�. �̵��� �����մϴ�.");
            StopMovement(); // �����ϰ� �̵� ����
            return;
        }
        if (!isMoving) return; // �̹� isMoving�� false�� �� �̻� ó�� �� ��


        Vector2 directionToTarget = (currentTargetPosition - (Vector2)transform.position);
        float distanceToTarget = directionToTarget.magnitude;

        if (distanceToTarget > arrivalThreshold)
        {
            Vector2 moveDirection = directionToTarget.normalized;
            rb.linearVelocity = moveDirection * moveSpeed;
            FlipSprite(moveDirection);
        }
        else
        {
            // ���� ����, ArrivedAtTarget ȣ��
            ArrivedAtTarget();
        }
    }

    public void MoveTo(NavPoint targetNavPoint)
    {
        // Debug.Log(gameObject.name + " - IdolMovement: MoveTo ȣ���. ��ǥ: " + (targetNavPoint == null ? "NULL" : targetNavPoint.pointName) + ", ���� isMoving: " + isMoving + ", ���� currentTargetObj: " + (currentTargetNavPointObject == null ? "NULL" : currentTargetNavPointObject.pointName));
        if (targetNavPoint == null)
        {
            Debug.LogWarning(gameObject.name + ": ��ǥ NavPoint�� null�Դϴ�. �̵��� �� �����ϴ�.");
            // StopMovement(); // ��ǥ�� null�̸� �̵��� ���ߴ� ���� ������ �� �ֽ��ϴ�.
            isMoving = false; // ��������� �̵� ���� �ƴ��� ǥ��
            return;
        }
        currentTargetNavPointObject = targetNavPoint;
        currentTargetPosition = targetNavPoint.GetPosition();
        isMoving = true; // �̵� ���� �÷���
        animator?.SetBool("isWalking", true); // �ִϸ��̼� ���� ������Ʈ
        Vector2 initialDirection = (currentTargetPosition - (Vector2)transform.position).normalized;
        if (initialDirection.sqrMagnitude > 0.01f)
        {
            FlipSprite(initialDirection);
        }
    }

    private void ArrivedAtTarget()
    {
        // �� �Լ��� �����ߴٴ� ���� �����ߴٴ� �ǹ�
        // Debug.Log(gameObject.name + " - IdolMovement: ArrivedAtTarget() ���� ����. isMoving: " + isMoving + ", currentTarget: " + (currentTargetNavPointObject == null ? "NULL" : currentTargetNavPointObject.pointName));
        animator?.SetBool("isWalking", false); // �ִϸ��̼� ���� ������Ʈ
        // ���� ���� isMoving�� false�� �����Ͽ� �ߺ� ȣ�� �� �ǵ�ġ ���� �߰� �̵� ����
        isMoving = false;
        rb.linearVelocity = Vector2.zero; // ������ �����ӵ� ��� ����

        NavPoint arrivedPoint = currentTargetNavPointObject; // �ݹ����� ������ NavPoint�� ���� ������ ����

        // currentTargetNavPointObject�� null�� �����ϴ� ���� Invoke ���Ŀ�,
        // �׸��� Invoke �������� �� ���� ������� �ʾ��� ��쿡�� �����ؾ� �մϴ�.
        // ������ �� ������ �����, Invoke�� �� �Ŀ��� �� ArrivedAtTarget�� å���� �����ٰ� ����,
        // ���ο� �̵��� �ݵ�� MoveTo�� ���� ���۵ǵ��� �ϴ� ���Դϴ�.
        // ����, �� ���� �̺�Ʈ�� ���� ��ǥ�� ���� �Ϸ�� ������ �����ϰ� null�� �����մϴ�.
        currentTargetNavPointObject = null;

        

        if (arrivedPoint != null)
        {
            // Debug.Log(gameObject.name + " - IdolMovement: OnArrivedAtTargetPoint.Invoke ȣ�� ���� (" + arrivedPoint.pointName + ")");
            OnArrivedAtTargetPoint?.Invoke(arrivedPoint);
        }
        else
        {
            // �� �αװ� ������, ArrivedAtTarget�� ȣ��� �� currentTargetNavPointObject�� �̹� null�̾��ٴ� �ǹ�.
            // �̴� MoveTo�� null�� ȣ��Ǿ��ų�, �ٸ� ������ currentTargetNavPointObject�� ����ġ �ʰ� null�� �� ���.
            // HandleMovement�� ��� �ڵ带 �߰������Ƿ� �� ���� �پ���� ��.
            Debug.LogError(gameObject.name + " - IdolMovement: ArrivedAtTarget���� Invoke �Ϸ� ������ arrivedPoint�� null�Դϴ�!");
            OnArrivedAtTargetPoint?.Invoke(null); // Idol.cs���� null�� ó���ϹǷ� null�� ȣ��
        }
    }

    public void StopMovement()
    {
        isMoving = false;
        animator?.SetBool("isWalking", false); // �ִϸ��̼� ���� ������Ʈ
        rb.linearVelocity = Vector2.zero;
        currentTargetNavPointObject = null; // �̵� ���� �� ���� ��ǥ�� �ʱ�ȭ�ϴ� ���� ������ �� �ֽ��ϴ�.
         Debug.Log(gameObject.name + " �̵� ������.");
    }

    private void FlipSprite(Vector2 moveDirection)
    {
        if (moveDirection.x > 0.01f && spriteRenderer.flipX)
        {
            spriteRenderer.flipX = false;
        }
        else if (moveDirection.x < -0.01f && !spriteRenderer.flipX)
        {
            spriteRenderer.flipX = true;
        }
    }

    public bool IsCurrentlyMoving()
    {
        return isMoving;
    }
}
