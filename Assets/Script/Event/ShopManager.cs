using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

public class ShopManager : MonoBehaviour
{

    public int scheduleCount;
    public List<ScheduleData> allSchedulCatalog;


    private List<ScheduleData> generatedSchedlue = new List<ScheduleData>();
    public void GenereateInventory()
    {
        List<ScheduleData> tempSchedule = new List<ScheduleData>(allSchedulCatalog);

        for (int i = 0; i < scheduleCount; i++)
        {
            if (tempSchedule.Count == 0) break; // 뽑을 카드가 더 없으면 중단

            // TODO: 실제 게임에서는 희귀도에 따른 가중치 추첨 로직을 구현해야 합니다.
            // 여기서는 간단하게 완전 무작위로 선택합니다.
            int randomIndex = Random.Range(0, tempSchedule.Count);
            ScheduleData selectedSchedlue = tempSchedule[randomIndex];

            generatedSchedlue.Add(selectedSchedlue);
            tempSchedule.RemoveAt(randomIndex); 
        }
    }
    private void HandlePurchaseRequest(ScheduleData schedlueToBuy)//, ShopSlotUI slot)
    {
        

        // TODO: 실제 플레이어의 골드와 비교하는 로직 필요
        // 예시: if (PlayerStats.Instance.Gold >= cardToBuy.price)
        int playerGold = 100; // 임시 플레이어 골드

        if (playerGold >= schedlueToBuy.price)
        {
            // 골드 차감
            MoneyManager.Instance.PayMoney(schedlueToBuy.price);

            // 덱에 카드 추가
            // PlayerDeck.Instance.AddCard(cardToBuy);
            

            // 해당 슬롯을 '판매 완료'로 표시
           

            // 구매 이벤트 구독 해제 (중복 구매 방지)
            
        }
        else
        {
            
            // TODO: 골드 부족 알림 UI 표시
        }
    }
}
