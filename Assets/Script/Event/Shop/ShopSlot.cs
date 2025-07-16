using RoomPlacementSystem;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopSlot : MonoBehaviour
{
    [Header("UI Components")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI priceText;
    public Image artworkImage;
    public Button buyButton;

    private RoomData currentRoomData;

    public event Action<RoomData, ShopSlot> OnPurchaseRequested;

    public void Setup(RoomData roomData)
    {
        currentRoomData = roomData;

        // UI 요소에 카드 데이터 할당
        nameText.text = roomData.roomName;
        descriptionText.text = roomData.description;
        priceText.text = roomData.price.ToString() + " G";
        artworkImage.sprite = roomData.roomIcon;

        // 버튼 클릭 시 이벤트 호출하도록 리스너 추가
        buyButton.onClick.RemoveAllListeners(); // 기존 리스너 제거
        buyButton.onClick.AddListener(HandleBuyButtonClick);
    }

    private void HandleBuyButtonClick()
    {
        // 내가 어떤 카드인지에 대한 정보와 함께 구매 요청 이벤트를 발생시킴
        OnPurchaseRequested?.Invoke(currentRoomData, this);
    }

    public void MarkAsSold()
    {
        buyButton.interactable = false; // 버튼 비활성화
        priceText.text = "SOLD OUT";
    }
}
