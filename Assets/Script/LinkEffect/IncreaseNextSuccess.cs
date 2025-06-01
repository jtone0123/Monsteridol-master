using UnityEngine;

[CreateAssetMenu(fileName = "IncreaseNextSuccess", menuName = "MyGame/Link Effects/Increase Next Success", order = 2)]
public class IncreaseNextSuccess : BaseOutgoingLinkEffect
{

    [Header("������ ������ ����")]
    [Tooltip("�ش� �������� �߰��� ������ ������")]
    [Range(0.0f,10.0f)]
    public float additionalStatIncreaseAmount = 1; // ���� Ȯ���� 10���� ��

    public override void ApplyToNext(IdolCharacter targetIdol, ScheduleData originScheduleData, ScheduleData targetNextScheduleData, NextScheduleModifiers nextScheduleTemporaryModifiers)
    {
        if(nextScheduleTemporaryModifiers != null)
        {
            nextScheduleTemporaryModifiers.successRateBonus += additionalStatIncreaseAmount; // ���ʽ� �� ���� ����
        }
        else
        {
            Debug.LogWarning($"IncreaseNextStatEffect: nextScheduleTemporaryModifiers ��ü�� null�Դϴ�. ȿ���� ������ �� �����ϴ�.");
        }
    }

    public override string GetDescription()
    {
        return $"���� ������ ���� �� �������� {additionalStatIncreaseAmount*10}%��ŭ �߰��� �����մϴ�. ({effectDescription})";
    }
}
