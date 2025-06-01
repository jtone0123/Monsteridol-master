// DraggableRoomItem.cs
// 건설 메뉴 UI에 표시되며, 플레이어가 드래그하여 방을 선택합니다.
// 드래그 중에는 이 UI 아이템이 마우스를 따라다니고, 슬롯 위에 월드 프리팹 미리보기가 표시될 때는 숨겨집니다.
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RoomPlacementSystem
{
    [RequireComponent(typeof(Image))]
    public class DraggableRoomItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [Tooltip("이 UI 아이템이 나타내는 방의 데이터입니다.")]
        public RoomData roomDataToRepresent;

        private Image _itemImage;
        private RectTransform _rectTransform;
        private CanvasGroup _canvasGroup;
        private Canvas _rootCanvas;
        private Transform _originalParent;
        private int _originalSiblingIndex;
        private Vector2 _originalAnchoredPosition;

        private bool _isActuallyDragging = false;

        void Awake()
        {
            _itemImage = GetComponent<Image>();
            _rectTransform = GetComponent<RectTransform>();
            _canvasGroup = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();

            Canvas[] canvases = GetComponentsInParent<Canvas>(true);
            if (canvases != null && canvases.Length > 0) _rootCanvas = canvases[canvases.Length - 1].rootCanvas;
            if (_rootCanvas == null) Debug.LogError("DraggableRoomItem이 속한 루트 Canvas를 찾을 수 없습니다!", gameObject);

            if (roomDataToRepresent == null)
            {
                _canvasGroup.interactable = false;
                if (_itemImage != null) _itemImage.enabled = false;
                return;
            }
            if (_itemImage != null && roomDataToRepresent.roomIcon != null)
            {
                _itemImage.sprite = roomDataToRepresent.roomIcon;
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (roomDataToRepresent == null || PlacementManager.Instance == null || _rootCanvas == null) return;
            if (PlacementManager.Instance.IsPlacingRoom) { eventData.pointerDrag = null; return; }

            _isActuallyDragging = true;
            PlacementManager.Instance.StartWorldPreviewPlacement(roomDataToRepresent, this);

            _originalParent = transform.parent;
            _originalSiblingIndex = transform.GetSiblingIndex();
            _originalAnchoredPosition = _rectTransform.anchoredPosition;

            transform.SetParent(_rootCanvas.transform, true);
            transform.SetAsLastSibling();

            _canvasGroup.alpha = 0.7f; // 드래그 시작 시 반투명하게 보임
            _canvasGroup.blocksRaycasts = false;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!_isActuallyDragging || !PlacementManager.Instance.IsPlacingRoom || _rectTransform == null) return;

            // UI 아이템이 마우스를 따라다니도록 위치 업데이트
            _rectTransform.position = eventData.position;

            // PlacementManager에게 마우스 위치를 전달하여 월드 미리보기 및 이 UI의 가시성 업데이트 요청
            PlacementManager.Instance.UpdateDragWorldPreview(eventData.position);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!_isActuallyDragging) return;
            _isActuallyDragging = false;

            if (PlacementManager.Instance == null || !PlacementManager.Instance.IsPlacingRoom)
            {
                RestoreToOriginalState();
                return;
            }

            PlacementManager.Instance.HandleWorldPreviewDrop(eventData);
            RestoreToOriginalState();
        }

        // PlacementManager가 월드 미리보기 상태에 따라 이 UI 아이템의 가시성을 조절하기 위해 호출
        public void SetVisibilityBasedOnWorldPreview(bool worldPreviewIsActive)
        {
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = worldPreviewIsActive ? 0f : 0.7f; // 월드 미리보기 중이면 완전히 숨김, 아니면 반투명
            }
        }

        private void RestoreToOriginalState()
        {
            _canvasGroup.alpha = 1f; // 완전히 보이도록
            _canvasGroup.blocksRaycasts = true;

            if (_originalParent != null)
            {
                transform.SetParent(_originalParent);
                transform.SetSiblingIndex(_originalSiblingIndex);
                _rectTransform.anchoredPosition = _originalAnchoredPosition; // 원래 UI 위치로
            }
            else if (Application.isPlaying && _rootCanvas != null)
            { // originalParent가 파괴된 경우 등 예외 처리
                transform.SetParent(_rootCanvas.transform); // 임시로 루트로
            }
        }

        public void HandleDragCancellation()
        {
            if (!_isActuallyDragging && !PlacementManager.Instance.IsPlacingRoom) return; // 이미 취소되었거나 드래그 중이 아니면 무시

            Debug.Log($"'{roomDataToRepresent?.roomName ?? "알 수 없는 방"}' UI 드래그 명시적 취소됨.");
            _isActuallyDragging = false;
            RestoreToOriginalState();
            // PlacementManager의 상태는 PlacementManager.CancelWorldPreviewPlacement에서 정리됨
        }
    }
}
