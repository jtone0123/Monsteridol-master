using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class DropSlot : MonoBehaviour, IDropHandler
{
    [Tooltip("애니메이션에 걸리는 시간입니다.")]
    public float tweenDuration = 0.3f;

    [Tooltip("아이템 사이의 간격(패딩)입니다. 0으로 설정하면 아이템들이 서로 딱 붙습니다.")]
    public float spacing = 0f; // 기본값을 0으로 변경하여 딱 붙도록 설정

    // 슬롯에 아이템이 드롭되었을 때 호출됩니다.
    public void OnDrop(PointerEventData eventData)
    {
        GameObject droppedObject = eventData.pointerDrag;
        if (droppedObject == null) return;

        DragSchedule draggableItem = droppedObject.GetComponent<DragSchedule>();
        if (draggableItem != null)
        {
            // 드롭된 아이템의 부모를 이 슬롯으로 설정합니다.
            draggableItem.transform.SetParent(this.transform);

            // 새로운 아이템이 추가되었으므로, 슬롯의 모든 아이템을 재정렬합니다.
            RealignItems();
        }
    }

    /// <summary>
    /// 이 슬롯에 있는 모든 자식 아이템들을 왼쪽부터 차례대로 정렬합니다.
    /// </summary>
    public void RealignItems()
    {
        // DoTween의 DelayedCall을 사용하여 한 프레임 뒤에 실행합니다.
        // 아이템의 부모가 바뀐 직후(OnDrop, OnEndDrag)에 바로 위치를 계산하면
        // RectTransform 값이 갱신되지 않았을 수 있어 정확하지 않을 수 있습니다.
        // 이렇게 한 프레임 지연을 주면 안전하게 최신 값으로 계산할 수 있습니다.
        DOVirtual.DelayedCall(0.01f, () =>
        {
            Debug.Log(gameObject.name + "의 아이템들을 재정렬합니다.");

            RectTransform slotRectTransform = GetComponent<RectTransform>();
            Vector3[] slotCorners = new Vector3[4];
            slotRectTransform.GetWorldCorners(slotCorners);

            // 정렬 시작 위치 (슬롯의 왼쪽 위)
            float startX = slotCorners[1].x;
            float centerY = slotCorners[1].y - (slotRectTransform.rect.height * slotRectTransform.lossyScale.y / 2f);

            // 현재까지 정렬된 아이템들의 누적 너비
            float currentOffset = 0f;

            // 슬롯의 모든 자식들을 순회하며 위치를 재계산하고 애니메이션 실행
            foreach (Transform child in transform)
            {
                DragSchedule item = child.GetComponent<DragSchedule>();
                if (item != null && child.gameObject.activeInHierarchy)
                {
                    RectTransform itemRect = item.GetComponent<RectTransform>();
                    float itemWidth = itemRect.rect.width * itemRect.lossyScale.x;

                    // 목표 위치 계산 (피벗이 중앙 기준)
                    float targetX = startX + currentOffset + (itemWidth / 2f);
                    Vector3 targetPosition = new Vector3(targetX, centerY, item.transform.position.z);

                    // DOTween으로 부드럽게 이동
                    item.transform.DOMove(targetPosition, tweenDuration).SetEase(Ease.OutQuint);

                    // 다음 아이템 위치를 위해 현재 아이템의 너비와 간격을 누적
                    currentOffset += itemWidth + spacing;
                }
            }
        });
    }
}