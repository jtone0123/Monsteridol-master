using UnityEngine;
using TMPro;
using Unity.VisualScripting;
using UnityEngine.PlayerLoop;

public class StatUI : MonoBehaviour
{
    public TextMeshProUGUI idolStatsText;

    // ǥ���� ���̵��� ������ (�� �����ʹ� �ܺο��� �������־�� �մϴ�)
    public IdolCharacter currentIdol;

    void Update()
    {
        // ���̵��� ������ UI�� ǥ��
        if (idolStatsText != null && currentIdol != null)
        {
            idolStatsText.text = $"�̸�: {currentIdol.characterName}\n" +
                                 $"����: {currentIdol.stats[StatType.Vocal]}\n" +
                                 $"��: {currentIdol.stats[StatType.Dance]}\n" +
                                 $"��: {currentIdol.stats[StatType.Rap]}\n"+
                                 $"��Ʈ����: {currentIdol.currentStress}";
                                 
        }
    }
    

    
}

