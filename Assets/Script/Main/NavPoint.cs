using UnityEngine;




// NavPoint.cs
// ��� ���� �� Ư�� ������ ��Ÿ���� Ŭ�����Դϴ�.
// ���̵��� �̵��ϰų� ��ȣ�ۿ��� ��ǥ �������� ���˴ϴ�.
public class NavPoint : MonoBehaviour
{
    public enum PointType
    {
        General,
        Desk,
        Sofa,
        PracticeVocal,
        PracticeDance,
        RestingSpot,
        StressRelief,
        RuleManifestSpot,
        Stair // ��� Ÿ�� �߰�
    }

    [Header("NavPoint �⺻ ����")]
    [Tooltip("�� NavPoint�� �̸��Դϴ�. (�����Ϳ��� �ĺ���)")]
    public string pointName = "���ο� ����";

    [Tooltip("�� NavPoint�� Ÿ���Դϴ�.")]
    public PointType type = PointType.General;

    [Tooltip("�� NavPoint�� ���� �� ��ȣ�Դϴ�. (��: 1��, 2��)")]
    public int floorID = 1;

    [Header("��� ���� (Type�� Stair�� ���)")]
    [Tooltip("�� NavPoint�� ������� �����Դϴ�. type�� Stair�� �����ϸ� �ڵ����� true�� �ǵ��� �� �� �ֽ��ϴ�.")]
    public bool isStair = false; // Type�� Stair�� true�� ����

    [Tooltip("����� �ݴ��� ��� NavPoint�Դϴ�. (�� ����� ���� �����ϴ� �ٸ� ���� ��� ����)")]
    public NavPoint connectedStairPoint;

    // (������) �ö󰡴� ������� �������� �������
    public enum StairDirection { None, Up, Down }
    [Tooltip("����� �����Դϴ� (�ö󰡴� ��� / �������� ���).")]
    public StairDirection stairDirection = StairDirection.None;


    void OnValidate()
    {
        // Inspector���� Type�� Stair�� �����ϸ� isStair�� true�� �ڵ� ���� (���� ���)
        if (type == PointType.Stair)
        {
            isStair = true;
        }
        else
        {
            isStair = false;
            // ����� �ƴϸ� ����� ��� ����Ʈ�� ������ �ǹ� ����
            connectedStairPoint = null;
            stairDirection = StairDirection.None;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(transform.position, 0.3f);
        if (isStair && connectedStairPoint != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, connectedStairPoint.transform.position);
        }
    }

    void OnDrawGizmos()
    {
        switch (type)
        {
            case PointType.Stair:
                Gizmos.color = Color.magenta;
                break;
            case PointType.RestingSpot:
                Gizmos.color = Color.green;
                break;
            case PointType.RuleManifestSpot:
                Gizmos.color = Color.red;
                break;
            default:
                Gizmos.color = Color.gray;
                break;
        }
        Gizmos.DrawSphere(transform.position, 0.2f);
    }

    public Vector2 GetPosition()
    {
        return transform.position;
    }
}
