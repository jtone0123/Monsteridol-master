using UnityEngine;

[CreateAssetMenu(fileName = "NewIncreaseNextStatEffect", menuName = "MyGame/Link Effects/Increase Next Stat", order = 2)]
public class IncreaseNextStatEffect : BaseOutgoingLinkEffect
{
    [Header("���� ������ ����")]
    [Tooltip("���� �������� ����� ������ ���� Ÿ��")]
    public StatType statToIncrease = StatType.None;

    [Tooltip("�ش� ���ȿ� �߰��� ������ ������")]
    public int additionalStatIncreaseAmount = 1;

    public override void ApplyToNext(IdolCharacter targetIdol, ScheduleData originScheduleData, ScheduleData targetNextScheduleData, NextScheduleModifiers nextScheduleTemporaryModifiers)
    {
        if (nextScheduleTemporaryModifiers != null)
        {
            // �� ȿ���� ���� �������� '�� ��� ����(primaryTargetStat)'�� �� ȿ���� 'statToIncrease'�� ��ġ�� ����
            // ���ʽ��� �ִ� ������� �� ���� �ְ�, �Ǵ� ������ statToIncrease�� ���ʽ��� �ֵ��� �� ���� �ֽ��ϴ�.
            // ���⼭�� ���� �������� ����� �� ���� ���ʽ��� '�߰�'�Ǵ� �������� �����մϴ�.
            // ���� ���� ������ ���� �������� Perform �������� NextScheduleModifiers�� �����Ͽ� �̷�����ϴ�.

            nextScheduleTemporaryModifiers.statToBuff = statToIncrease; // � ���ȿ� ���ʽ��� ���� ���
            nextScheduleTemporaryModifiers.statBuffAmount += additionalStatIncreaseAmount; // ���ʽ� �� ���� ����

            Debug.Log($"���� ȿ�� �ߵ�: '{originScheduleData.scheduleName}'�� �������� ���� ������ '{targetNextScheduleData.scheduleName}' ���� �� {statToIncrease} ���ȿ� +{additionalStatIncreaseAmount} �߰� ���ʽ��� ����� �����Դϴ�.");
        }
        else
        {
            Debug.LogWarning($"IncreaseNextStatEffect: nextScheduleTemporaryModifiers ��ü�� null�Դϴ�. ȿ���� ������ �� �����ϴ�.");
        }
    }

    public override string GetDescription()
    {
        return $"���� ������ ���� �� {statToIncrease} ���� ȹ�淮�� {additionalStatIncreaseAmount}��ŭ �߰��� �����մϴ�. ({effectDescription})";
    }
}
