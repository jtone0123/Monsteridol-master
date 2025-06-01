using UnityEngine;

/// <summary>
/// ��� '�߽���' ���� ȿ�� ScriptableObject�� ����� �Ǵ� �߻� Ŭ�����Դϴ�.
/// �� ������(Origin)�� ����� ��, ���� ������(TargetNext)�� Ư�� ȿ
/// ���� ���� ������ ���� ����(��: PerformScheduleBundle)���� �� ���� ������ �������� �����մϴ�.
/// </summary>
public abstract class BaseOutgoingLinkEffect : ScriptableObject
{
    [Header("ȿ�� ���� (UI ǥ�ÿ�)")]
    [TextArea(2, 4)]
    public string effectDescription = "�� ȿ���� ���� �����Դϴ�.";

    /// <summary>
    /// �� ���� ȿ���� ���� �����ٿ� �����մϴ�.
    /// �� �޼ҵ�� TempScheduleExecutor ���� ������, ���� ������(originScheduleData) ���� ��,
    /// ���� ������(targetNextScheduleData)�� �����ϱ� ������ ȣ��� �� �ֽ��ϴ�.
    /// ���� �������� �ӽ� �Ӽ�(��: �ӽ� ������, �ӽ� ���� ���ʽ�)�� �����ϴ� ������� ������ �� �ֽ��ϴ�.
    /// </summary>
    /// <param name="targetIdol">ȿ���� ���� ���̵� ĳ�����Դϴ�.</param>
    /// <param name="originScheduleData">�� ���� ȿ���� �߻���Ű�� ���� �������� �������Դϴ�.</param>
    /// <param name="targetNextScheduleData">'��������' ����� �����̸�, �� ȿ���� ����� ��� �������� �������Դϴ�.</param>
    /// <param name="nextScheduleTemporaryModifiers">���� ������ ���� �� ����� �ӽ� ���� ������ �����ϰ� �����ϴ� ��ü�Դϴ�. (��: ������ ���ʽ�, ���� ���ʽ� ��)</param>
    public abstract void ApplyToNext(IdolCharacter targetIdol, ScheduleData originScheduleData, ScheduleData targetNextScheduleData, NextScheduleModifiers nextScheduleTemporaryModifiers);

    /// <summary>
    /// UI � ǥ�õ� �� ȿ���� ���� ������ ��ȯ�մϴ�.
    /// �ʿ信 ���� �������� ������ ������ �� �ֽ��ϴ�.
    /// </summary>
    public virtual string GetDescription()
    {
        return effectDescription;
    }
}

/// <summary>
/// ���� ������ ���� �� ����� �ӽ����� ���� ������ ��� Ŭ�����Դϴ�.
/// BaseOutgoingLinkEffect�� ApplyToNext �޼ҵ忡�� �� ��ü�� ���� �����Ͽ�,�Ͽ� ����մϴ�.
/// </summary>
[System.Serializable] // �ʿ信 ����, ������ ������ ���� ������ �������� �����Ͽ� ����
public class NextScheduleModifiers
{
    public float successRateBonus = 0f; // ������ �� �������� �������� �߰��� ���ʽ� (��: 0.1f�� +10%)
    public StatType statToBuff = StatType.None; // ������ �� �������� ����� Ư�� ���ȿ� �߰� ���ʽ��� �� ���
    public int statBuffAmount = 0; // �ش� ���ȿ� �߰��� ���ʽ� ��
    // �ʿ信 ���� �ٸ� ������ ���� ���� �߰� ���� (��: ��Ʈ���� ���� ���ʽ�, ��� ���� ��)

    public void Reset()
    {
        successRateBonus = 0f;
        statToBuff = StatType.None;
        statBuffAmount = 0;
    }
}
