// PlacementManager.cs
// �� ��ġ ������ ���������� �����մϴ�.
// �巡�� ���� UI �������� ���콺�� ����ٴϰ�, ���� ���� ���� ������ �̸����Ⱑ ǥ�õ� ���� UI �������� ����ϴ�.
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using System.Collections.Generic;

namespace RoomPlacementSystem
{
    public class PlacementManager : MonoBehaviour
    {
        public static PlacementManager Instance { get; private set; }

        public bool IsPlacingRoom { get; private set; } = false;
        private RoomData _selectedRoomData;
        private DraggableRoomItem _sourceDragItem; // �巡�׸� ������ UI ������
        private GameObject _tempPreviewInstance;   // ���忡 �ӽ÷� ǥ�õǴ� �� ������ �ν��Ͻ�
        private RoomSlot _currentHoveredSlot = null;

        private Mouse _currentMouseDevice;

        // ���� ������ �̸����Ⱑ ���� Ȱ��ȭ�Ǿ� �ִ��� ���� (DraggableRoomItem�� ���ü� �����)
        public bool IsWorldPreviewActive => _tempPreviewInstance != null && _tempPreviewInstance.activeSelf;


        void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        void Update()
        {
            _currentMouseDevice = Mouse.current;
            if (_currentMouseDevice == null) return;

            if (!IsPlacingRoom || _selectedRoomData == null)
            {
                // ��ġ ��尡 �ƴ� �� ���� ���� (UpdateDragWorldPreview�� ȣ����� ���� �� ���)
                if (_tempPreviewInstance != null) DestroyPreview();
                if (_currentHoveredSlot != null) { _currentHoveredSlot.SetHoverState(false); _currentHoveredSlot = null; }
                _sourceDragItem?.SetVisibilityBasedOnWorldPreview(false); // UI ������ �ٽ� ���̰�
                return;
            }

            // ���콺 ��Ŭ������ ��ġ ���
            if (_currentMouseDevice.rightButton.wasPressedThisFrame)
            {
                CancelWorldPreviewPlacement();
            }
        }

        // DraggableRoomItem�� OnDrag���� �� ������ ȣ���
        public void UpdateDragWorldPreview(Vector2 mouseScreenPos)
        {
            if (!IsPlacingRoom) return;

            RoomSlot previouslyHoveredSlot = _currentHoveredSlot;
            _currentHoveredSlot = GetSlotUnderScreenPosition(mouseScreenPos);

            if (_currentHoveredSlot != previouslyHoveredSlot)
            {
                previouslyHoveredSlot?.SetHoverState(false);
                _currentHoveredSlot?.SetHoverState(true);
                UpdateTemporaryPreviewVisuals(); // �̸����� ����/���� �� UI ������ ���ü� ����
            }

            // �ӽ� �̸����� ��ġ�� �׻� ���� ȣ���� ������ ��ġ�� (�Ǵ� ���콺 ��ġ�� �ε巴�� �̵�)
            if (_tempPreviewInstance != null && _currentHoveredSlot != null)
            {
                _tempPreviewInstance.transform.position = _currentHoveredSlot.transform.position;
                // �������� UpdateTemporaryPreviewVisuals���� ���� ���� �� �� ���� ����
            }
        }

        private void UpdateTemporaryPreviewVisuals()
        {
            // 1. ���� �̸����� ����
            DestroyPreview();

            // 2. �� ���� ���� �ִٸ� �� �̸����� ����
            if (_currentHoveredSlot != null && _selectedRoomData != null && _selectedRoomData.roomPrefab != null)
            {
                _tempPreviewInstance = Instantiate(_selectedRoomData.roomPrefab);
               _tempPreviewInstance.transform.SetParent(_currentHoveredSlot.transform, false); // ������ �ڽ����� ����
                _tempPreviewInstance.transform.localScale = Vector3.one; // �ʱ� �������� 1,1,1�� ����
                _tempPreviewInstance.name = _selectedRoomData.roomName + "_TempPreview";
                _tempPreviewInstance.transform.position = _currentHoveredSlot.transform.position;

                

                SetPreviewCollidersActive(false); // �̸������ �浹 ����

                // TODO: �̸������� Sorting Layer / Order in Layer ���� �ʿ�
                // SpriteRenderer[] srs = _tempPreviewInstance.GetComponentsInChildren<SpriteRenderer>();
                // foreach(var sr in srs) { sr.sortingLayerName = "WorldPreview"; sr.sortingOrder = 10; }
                Debug.Log($"�ӽ� �̸����� '{_tempPreviewInstance.name}' ���� at '{_currentHoveredSlot.name}'.");
            }

            // 3. �巡�� ���� UI �������� ���ü� ������Ʈ
            _sourceDragItem?.SetVisibilityBasedOnWorldPreview(IsWorldPreviewActive);
        }

        private void SetPreviewCollidersActive(bool active)
        {
            if (_tempPreviewInstance == null) return;
            Collider col = _tempPreviewInstance.GetComponent<Collider>();
            if (col != null) col.enabled = active;
            Collider2D col2D = _tempPreviewInstance.GetComponent<Collider2D>();
            if (col2D != null) col2D.enabled = active;
            // �ڽ� �ݶ��̴��鵵 ó���Ϸ��� GetComponentsInChildren ���
        }

        private void DestroyPreview()
        {
            if (_tempPreviewInstance != null)
            {
                Destroy(_tempPreviewInstance);
                _tempPreviewInstance = null;
            }
        }

        public void StartWorldPreviewPlacement(RoomData roomData, DraggableRoomItem sourceItem)
        {
            if (IsPlacingRoom) return;
            if (roomData == null || roomData.roomPrefab == null || sourceItem == null)
            {
                Debug.LogError("PlacementManager: StartWorldPreviewPlacement�� ���޵� ���ڰ� null�Դϴ�.");
                return;
            }
            IsPlacingRoom = true;
            _selectedRoomData = roomData;
            _sourceDragItem = sourceItem; // �巡�� ������ UI ������ ����

            // ���� �ÿ��� ���� �̸����Ⱑ �����Ƿ� UI �������� ���� (DraggableRoomItem���� ���� ����)
            _sourceDragItem.SetVisibilityBasedOnWorldPreview(false);
            Debug.Log($"PlacementManager: '{_selectedRoomData.roomName}' �� ��ġ(���� �̸�����) ����. �巡�� UI: '{_sourceDragItem.name}'.");
        }

        public void HandleWorldPreviewDrop(PointerEventData eventData)
        {
            if (!IsPlacingRoom || _selectedRoomData == null)
            {
                EndWorldPreviewPlacement(false); // ���з� ����
                return;
            }
            Debug.Log("PlacementManager: HandleWorldPreviewDrop ȣ���.");

            RoomSlot finalDropTargetSlot = GetSlotUnderScreenPosition(eventData.position);

            bool placedSuccessfully = false;
            if (finalDropTargetSlot != null)
            {
                if (finalDropTargetSlot.IsOccupied)
                {
                    finalDropTargetSlot.ClearSlot();
                }

                if (_tempPreviewInstance != null && _currentHoveredSlot == finalDropTargetSlot)
                {
                    // ���� ǥ�� ���� �ӽ� �̸����⸦ ���� ��ġ�� ���
                    SetPreviewCollidersActive(true); // ���� ��ġ�� ���� �ݶ��̴� Ȱ��ȭ
                    finalDropTargetSlot.ConfirmTemporaryPreviewAsPlaced(_selectedRoomData, _tempPreviewInstance);
                    _tempPreviewInstance = null; // ���� �� �̻� �ӽð� �ƴ�, ������ ����
                    placedSuccessfully = true;
                }
                else // �ӽ� �̸����Ⱑ ���ų� �ٸ� ���Կ� �־��ٸ� ���� ����
                {
                    if (_tempPreviewInstance != null) DestroyPreview(); // �߸��� ��ġ�� �̸����� ����

                    GameObject newRoomInstance = Instantiate(_selectedRoomData.roomPrefab);
                    newRoomInstance.transform.SetParent(finalDropTargetSlot.transform, false); // ������ �ڽ����� ����
                    newRoomInstance.transform.localScale = Vector3.one; // �ʱ� �������� 1,1,1�� ����
                    newRoomInstance.name = _selectedRoomData.roomName + "_Placed";
                    
                    // PlaceRoom ���ο��� �θ� �� ��ġ ����
                    if (finalDropTargetSlot.PlaceRoom(_selectedRoomData, newRoomInstance))
                    {
                        placedSuccessfully = true;
                    }
                }
            }
            else
            {
                Debug.LogWarning("PlacementManager: ��ȿ�� RoomSlot�� ��ӵ��� �ʾ� ��ġ�� ����մϴ�.");
            }

            EndWorldPreviewPlacement(placedSuccessfully);
        }

        private void EndWorldPreviewPlacement(bool success)
        {
            IsPlacingRoom = false;

            DestroyPreview(); // �����ִ� �ӽ� �̸����� ����

            if (_currentHoveredSlot != null)
            {
                _currentHoveredSlot.SetHoverState(false);
                _currentHoveredSlot = null;
            }

            // �巡�� �����ߴ� UI �������� ���� ���� ������ DraggableRoomItem.OnEndDrag/HandleDragCancellation���� ó��
            // ���⼭�� IsPlacingRoom = false�� ���������Ƿ�, DraggableRoomItem�� OnDrag ��� �̸� �����Ͽ� ������ ������ �� ����
            _sourceDragItem?.SetVisibilityBasedOnWorldPreview(false); // UI ������ Ȯ���� �ٽ� ���̰�

            _selectedRoomData = null;
            // _sourceDragItem�� DraggableRoomItem�� OnEndDrag���� null�� ����ų� ���⼭ �� �ʿ� ����.
            // ���� �巡�� ���� �� �������.
            Debug.Log($"PlacementManager: ���� �̸����� ��ġ ��� ����. ����: {success}");
        }

        public void CancelWorldPreviewPlacement()
        {
            if (!IsPlacingRoom) return;
            Debug.Log("PlacementManager: ���� �̸����� ��ġ ��ҵ�.");

            _sourceDragItem?.HandleDragCancellation(); // UI �����ۿ��� ��� �˸� �� ���� ���� ��û
            EndWorldPreviewPlacement(false); // ���з� �����ϰ� ��� ���� ����
        }

        private RoomSlot GetSlotUnderScreenPosition(Vector2 screenPosition)
        {
            if (Camera.main == null) return null;

            PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
            pointerEventData.position = screenPosition;
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerEventData, results);

            foreach (RaycastResult result in results)
            {
                if (result.gameObject.GetComponent<CanvasRenderer>() == null)
                {
                    RoomSlot slot = result.gameObject.GetComponent<RoomSlot>();
                    if (slot != null) return slot;
                    if (result.gameObject.transform.parent != null)
                    {
                        slot = result.gameObject.transform.parent.GetComponent<RoomSlot>();
                        if (slot != null) return slot;
                    }
                }
            }

            Ray ray = Camera.main.ScreenPointToRay(screenPosition);
            RaycastHit hit;
            // LayerMask�� ����ϸ� �� ȿ�����Դϴ�. ��: public LayerMask roomSlotLayer;
            // if (Physics.Raycast(ray, out hit, Mathf.Infinity, roomSlotLayer))
            if (Physics.Raycast(ray, out hit, Mathf.Infinity))
            {
                return hit.collider.GetComponent<RoomSlot>();
            }
            return null;
        }
    }
}
