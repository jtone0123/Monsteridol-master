using System.Collections.Generic;
using UnityEngine;

// IdolCharacter.cs ���� ������ StatType enum�� �־�� �մϴ�.
// public enum StatType { None, Vocal, Dance, Rap, Visual, Stamina } // �� �κ��� IdolCharacter.cs �� �����ϰų� �� ������ ����

[CreateAssetMenu(fileName = "NewScheduleData", menuName = "MyGame/Schedule Data", order = 1)]
public class ScheduleData : ScriptableObject
{
        

    [Header("�⺻ ����")]
    public string scheduleName = "�� ������";
    [TextArea(3, 5)]
    public string description = "�����ٿ� ���� �����Դϴ�.";
    public Sprite icon;
    public int cost = 1; // ������ ���� ��� (��: �ൿ��)

    [Header("���� ȿ��")]
    [Range(0f, 1f)] // 0.0 (0%) ~ 1.0 (100%)
    public float baseSuccessRate = 0.7f; // �⺻ ���� Ȯ��

    // �� �������� �������� �� �ַ� � �ɷ�ġ�� �󸶳� �ø� ���ΰ�
    public StatType primaryTargetStat = StatType.None; // �ַ� ������ �ִ� �ɷ�ġ Ÿ��
    public int primaryStatImprovementAmount = 5;   // �ش� �ɷ�ġ ��� ���� (���� ��)

    public int stressChangeOnSuccess = 2;      // ���� �� ��Ʈ���� ��ȭ��

    // �������� ���� ȿ��
    // public StatType statPenaltyTargetStat = StatType.None; // ���� �� ������ �� ���� (�ʿ��ϴٸ�)
    // public int statPenaltyAmount = 0;       // ���� �� ���� ���ҷ� (0 �Ǵ� ����)
    public int stressChangeOnFailure = 10;     // ���� �� ��Ʈ���� ��ȭ��

    [Header("���� ȿ�� (2�ܰ�)")]
    [Tooltip("�� �������� ���� ������ �����Ͽ� Ư���� ȿ���� ���� �� �ִ��� ����")]
    public bool canFormConsecutiveBundle = false;

    [Tooltip("���� ȿ���� �ߵ��ϱ� ���� �ּ� ���� ������ ���� (��: 2�� �̻� ���� ��)")]
    public int minItemsForConsecutiveEffect = 2;

    [Tooltip("���� �� ������ ����(n)�� �⺻ ���� Ȯ���� �������ų� ������ ��. ��: -0.2�� n���� 20%�� ����")]
    public float consecutiveSuccessRateModifierPerN = 0f;

    [Tooltip("���� ȿ���� ���� �� ��� ����(primaryTargetStat)�� �߰��Ǵ� ���ʽ� ��ġ")]
    public int consecutiveStatBonus = 0;

    [Tooltip("consecutiveStatBonus�� ���� �� �� �����۸��� ������� (true), �ƴϸ� ���� ��ü�� �� ���� ������� (false) ����")]
    public bool applyStatBonusPerItemInBundle = false;

    [Tooltip("���� Ȯ�� ���Ƽ(consecutiveSuccessRateModifierPerN < 0 �� ���)�� �����Ǵ� �ִ� n��. 0�̸� ���� ����.")]
    public int maxNForSuccessRatePenaltyStack = 0;



    // ���� ȿ�� �ϳ��� �����ϴ� ���� Ŭ���� (�Ǵ� ����ü)
    [Header("���� ȿ�� (3�ܰ� - �߽���)")]
    [Tooltip("�� �������� ����� ��, Ư�� ������ ���� �����ٿ� ������ �� �ִ� ȿ�� ���")]
    public List<OutgoingLinkEffectRule> outgoingLinkEffectRules;

    // �� ������(A)�� ������ ���� ������(B)�� � ������ ���� �����ϴ� ���� Ŭ����
    [System.Serializable]
    public class OutgoingLinkEffectRule
    {
        [Tooltip("�� ȿ���� ����� '����' �������� ���� (��: Ư�� ������ �����Ϳ� ��ġ�ؾ� ��)")]
        public ScheduleData targetNextScheduleCondition; // ������ �� �������� �;� ȿ�� �ߵ�

        [Tooltip("�� ������ �����Ǹ� '����' �����ٿ� ����� Ư�� ���� ȿ�� ����")]
        public BaseOutgoingLinkEffect effectToApplyOnNextSchedule; // � ȿ���� ���� �����ٿ� �� ���ΰ� (BaseOutgoingLinkEffect�� ���� �ܰ迡�� ����)
    }
}