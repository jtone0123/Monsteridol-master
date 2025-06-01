// IdolCharacter.cs
using UnityEngine;
using System.Collections.Generic; // Dictionary ��� ��

// ScheduleData���� ����� StatType�� �����ؾ� ��
public enum StatType { None, Vocal, Dance, Rap, Visual, Stamina } // ���� ���� Ÿ��

public class IdolCharacter : MonoBehaviour
{
    public string characterName = "idol";

    // ���� (Dictionary �Ǵ� ���� ������ ����)
    public Dictionary<StatType, int> stats = new Dictionary<StatType, int>();
    public int currentStress = 0;
    public int maxStress = 100; // �ִ� ��Ʈ���� (����)

    void Awake()
    {
        // �ʱ� ���� ����
        stats[StatType.Vocal] = 10;
        stats[StatType.Dance] = 10;
        stats[StatType.Rap] = 5;
        stats[StatType.Visual] = 20;
        stats[StatType.Stamina] = 50; // ������ ������ � ���� �� �� �ִ� ���� (���߿� Ȱ��)
        currentStress = 0;

        Debug.Log($"{characterName} �ʱ� ����: {GetCurrentStatus()}");
    }

    // ���� ���� ���� �Լ� (����)
    public void AddVocalPoint(int amount)
    {
        if (stats.ContainsKey(StatType.Vocal))
        {
            stats[StatType.Vocal] += amount;
            Debug.Log($"{characterName} ���� �ɷ�ġ {amount} ����. ����: {stats[StatType.Vocal]}");
        }
    }
    public void AddDancePoint(int amount)
    {
        if (stats.ContainsKey(StatType.Dance))
        {
            stats[StatType.Dance] += amount;
            Debug.Log($"{characterName} �� �ɷ�ġ {amount} ����. ����: {stats[StatType.Dance]}");
        }
    }

    public void AddRapPoint(int amount)
    {
        if (stats.ContainsKey(StatType.Rap))
        {
            stats[StatType.Rap] += amount;
            Debug.Log($"{characterName} �� �ɷ�ġ {amount} ����. ����: {stats[StatType.Rap]}");
        }
    }
    // ... �ٸ� ���� �Լ��鵵 �����ϰ� ...

    public void ChangeStress(int amount)
    {
        currentStress += amount;
        currentStress = Mathf.Clamp(currentStress, 0, maxStress); // ��Ʈ������ 0�� maxStress ���� ������ ����
        Debug.Log($"{characterName} ��Ʈ���� {amount} ����. ����: {currentStress}/{maxStress}");
        // TODO: ��Ʈ���� ��ġ�� ���� ����� �Ǵ� ������ Ư�� ���� ���� (���߿� �߰�)
    }

    public string GetCurrentStatus()
    {
        string status = $"��Ʈ����: {currentStress}/{maxStress}";
        foreach (var statEntry in stats)
        {
            status += $", {statEntry.Key}: {statEntry.Value}";
        }
        return status;
    }
}