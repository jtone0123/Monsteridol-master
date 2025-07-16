using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using RoomPlacementSystem;

public class ShopManager : MonoBehaviour
{
    [Header("UI References")]
    public Transform SlotParent; // 상품 슬롯들이 생성될 부모 객체 (Layout Group)
   
    
    [Header("Shop Config")]
    public int roomCount;
    
    [Header("Data & Prefabs")]
    public List<RoomData> allRoomsCatalog;
    public GameObject shopSlotPrefab;

    private List<RoomData> generatedRoom = new List<RoomData>();

    private void Start()
    {
        GenerateAndDisplayShop();       
    }


    public void GenerateAndDisplayShop()
    {
        ClearShop();
        GenerateInventory();
        PopulateShopUI();
    }

    private void ClearShop()
    {
        foreach (Transform child in SlotParent)
        {
            Destroy(child.gameObject);
        }
        generatedRoom.Clear();
    }

    public void GenerateInventory()
    {
        List<RoomData> tempRoom = new List<RoomData>(allRoomsCatalog);

        for (int i = 0; i < roomCount; i++)
        {
            if (tempRoom.Count == 0) break; // 뽑을 카드가 더 없으면 중단

            // TODO: 실제 게임에서는 희귀도에 따른 가중치 추첨 로직을 구현해야 합니다.
            // 여기서는 간단하게 완전 무작위로 선택합니다.
            int randomIndex = Random.Range(0, tempRoom.Count);
            RoomData selectedRoom = tempRoom[randomIndex];

            generatedRoom.Add(selectedRoom);
            tempRoom.RemoveAt(randomIndex); 
        }
    }

    private void PopulateShopUI()
    {
        foreach (RoomData card in generatedRoom)
        {
            // 프리팹으로부터 슬롯 UI 생성
            GameObject slotObject = Instantiate(shopSlotPrefab, SlotParent);
            ShopSlot slotUI = slotObject.GetComponent<ShopSlot>();

            if (slotUI != null)
            {
                // 슬롯 UI에 카드 데이터 전달 및 설정
                slotUI.Setup(card);
                // 슬롯의 구매 버튼 클릭 이벤트를 구독
                slotUI.OnPurchaseRequested += HandlePurchaseRequest;
            }
        }
    }

    private void HandlePurchaseRequest(RoomData RoomToBuy,ShopSlot slot)//, ShopSlotUI slot)
    {
        

        

        if (MoneyManager.Instance.currentMoney >= RoomToBuy.price)
        {
            // 골드 차감
            MoneyManager.Instance.PayMoney(RoomToBuy.price);




            // 해당 슬롯을 '판매 완료'로 표시
            slot.MarkAsSold();

            // 구매 이벤트 구독 해제 (중복 구매 방지)
            slot.OnPurchaseRequested -= HandlePurchaseRequest;
        }
        else
        {
            
            // TODO: 골드 부족 알림 UI 표시
        }
    }
}
