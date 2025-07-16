using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(CanvasGroup))]
public class DragSchedule : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Tooltip("UI 최상단 Canvas의 RectTransform을 할당해주세요.")]
    public RectTransform canvasRectTransform;

    // --- Private & Static Variables ---
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;

    // 드래그가 시작된 원래 부모를 기억하기 위한 변수들
    private Transform originalParent;
    private DropSlot originalSlot; // 아이템이 원래 속해있던 슬롯

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();

        if (canvasRectTransform == null)
        {
            canvasRectTransform = transform.root.GetComponent<RectTransform>();
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log("OnBeginDrag: 드래그 시작");

        // 원래 상태를 모두 기록
        originalParent = transform.parent;
        originalSlot = originalParent.GetComponent<DropSlot>();

        // 드래그 시각 효과
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.6f;

        // 드래그 시작과 동시에 아이템을 최상단 캔버스 자식으로 옮겨
        // 다른 UI 요소 위에 그려지도록 하고, 부모 UI의 레이아웃 계산 부담을 줄여줍니다.
        transform.SetParent(canvasRectTransform);
        transform.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData eventData)
    {
        // rectTransform.position을 직접 마우스 위치로 설정합니다.
        rectTransform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log("OnEndDrag: 드래그 종료");

        // 시각 효과 복원
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1.0f;

        // 만약 드롭된 부모가 최상단 캔버스 그대로라면, 유효한 슬롯에 드롭되지 않은 것입니다.
        if (transform.parent == canvasRectTransform)
        {
            // [수정됨] 아이템의 부모만 원래 슬롯으로 되돌립니다.
            // 위치는 바꾸지 않습니다. RealignItems가 애니메이션으로 위치를 잡아줄 것입니다.
            transform.SetParent(originalParent);
        }

        // 만약 이 아이템이 원래 슬롯에서 시작되었다면,
        // 아이템이 다른 슬롯으로 이동했거나, 허공에 드롭되었거나 상관없이
        // 원래 슬롯은 아이템이 하나 빠져나갔으므로 재정렬이 필요합니다.
        if (originalSlot != null)
        {
            originalSlot.RealignItems();
        }

        // 만약 아이템이 새로운 슬롯으로 이동했다면,
        // 새로운 슬롯의 OnDrop() 메서드가 이미 자신의 RealignItems()를 호출했을 것이므로
        // 여기서 추가로 호출할 필요가 없습니다.
    }
}

