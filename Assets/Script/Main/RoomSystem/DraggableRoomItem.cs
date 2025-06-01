// DraggableRoomItem.cs
// �Ǽ� �޴� UI�� ǥ�õǸ�, �÷��̾ �巡���Ͽ� ���� �����մϴ�.
// �巡�� �߿��� �� UI �������� ���콺�� ����ٴϰ�, ���� ���� ���� ������ �̸����Ⱑ ǥ�õ� ���� �������ϴ�.
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RoomPlacementSystem
{
    [RequireComponent(typeof(Image))]
    public class DraggableRoomItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [Tooltip("�� UI �������� ��Ÿ���� ���� �������Դϴ�.")]
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
            if (_rootCanvas == null) Debug.LogError("DraggableRoomItem�� ���� ��Ʈ Canvas�� ã�� �� �����ϴ�!", gameObject);

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

            _canvasGroup.alpha = 0.7f; // �巡�� ���� �� �������ϰ� ����
            _canvasGroup.blocksRaycasts = false;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!_isActuallyDragging || !PlacementManager.Instance.IsPlacingRoom || _rectTransform == null) return;

            // UI �������� ���콺�� ����ٴϵ��� ��ġ ������Ʈ
            _rectTransform.position = eventData.position;

            // PlacementManager���� ���콺 ��ġ�� �����Ͽ� ���� �̸����� �� �� UI�� ���ü� ������Ʈ ��û
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

        // PlacementManager�� ���� �̸����� ���¿� ���� �� UI �������� ���ü��� �����ϱ� ���� ȣ��
        public void SetVisibilityBasedOnWorldPreview(bool worldPreviewIsActive)
        {
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = worldPreviewIsActive ? 0f : 0.7f; // ���� �̸����� ���̸� ������ ����, �ƴϸ� ������
            }
        }

        private void RestoreToOriginalState()
        {
            _canvasGroup.alpha = 1f; // ������ ���̵���
            _canvasGroup.blocksRaycasts = true;

            if (_originalParent != null)
            {
                transform.SetParent(_originalParent);
                transform.SetSiblingIndex(_originalSiblingIndex);
                _rectTransform.anchoredPosition = _originalAnchoredPosition; // ���� UI ��ġ��
            }
            else if (Application.isPlaying && _rootCanvas != null)
            { // originalParent�� �ı��� ��� �� ���� ó��
                transform.SetParent(_rootCanvas.transform); // �ӽ÷� ��Ʈ��
            }
        }

        public void HandleDragCancellation()
        {
            if (!_isActuallyDragging && !PlacementManager.Instance.IsPlacingRoom) return; // �̹� ��ҵǾ��ų� �巡�� ���� �ƴϸ� ����

            Debug.Log($"'{roomDataToRepresent?.roomName ?? "�� �� ���� ��"}' UI �巡�� ����� ��ҵ�.");
            _isActuallyDragging = false;
            RestoreToOriginalState();
            // PlacementManager�� ���´� PlacementManager.CancelWorldPreviewPlacement���� ������
        }
    }
}
