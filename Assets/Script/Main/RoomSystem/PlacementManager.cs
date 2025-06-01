// PlacementManager.cs
// 방 배치 과정을 전반적으로 관리합니다.
// 드래그 중인 UI 아이템은 마우스를 따라다니고, 슬롯 위에 월드 프리팹 미리보기가 표시될 때는 UI 아이템을 숨깁니다.
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
        private DraggableRoomItem _sourceDragItem; // 드래그를 시작한 UI 아이템
        private GameObject _tempPreviewInstance;   // 월드에 임시로 표시되는 방 프리팹 인스턴스
        private RoomSlot _currentHoveredSlot = null;

        private Mouse _currentMouseDevice;

        // 월드 프리팹 미리보기가 현재 활성화되어 있는지 여부 (DraggableRoomItem의 가시성 제어용)
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
                // 배치 모드가 아닐 때 정리 로직 (UpdateDragWorldPreview가 호출되지 않을 때 대비)
                if (_tempPreviewInstance != null) DestroyPreview();
                if (_currentHoveredSlot != null) { _currentHoveredSlot.SetHoverState(false); _currentHoveredSlot = null; }
                _sourceDragItem?.SetVisibilityBasedOnWorldPreview(false); // UI 아이템 다시 보이게
                return;
            }

            // 마우스 우클릭으로 배치 취소
            if (_currentMouseDevice.rightButton.wasPressedThisFrame)
            {
                CancelWorldPreviewPlacement();
            }
        }

        // DraggableRoomItem의 OnDrag에서 매 프레임 호출됨
        public void UpdateDragWorldPreview(Vector2 mouseScreenPos)
        {
            if (!IsPlacingRoom) return;

            RoomSlot previouslyHoveredSlot = _currentHoveredSlot;
            _currentHoveredSlot = GetSlotUnderScreenPosition(mouseScreenPos);

            if (_currentHoveredSlot != previouslyHoveredSlot)
            {
                previouslyHoveredSlot?.SetHoverState(false);
                _currentHoveredSlot?.SetHoverState(true);
                UpdateTemporaryPreviewVisuals(); // 미리보기 생성/제거 및 UI 아이템 가시성 조절
            }

            // 임시 미리보기 위치는 항상 현재 호버된 슬롯의 위치로 (또는 마우스 위치로 부드럽게 이동)
            if (_tempPreviewInstance != null && _currentHoveredSlot != null)
            {
                _tempPreviewInstance.transform.position = _currentHoveredSlot.transform.position;
                // 스케일은 UpdateTemporaryPreviewVisuals에서 슬롯 변경 시 한 번만 설정
            }
        }

        private void UpdateTemporaryPreviewVisuals()
        {
            // 1. 기존 미리보기 제거
            DestroyPreview();

            // 2. 새 슬롯 위에 있다면 새 미리보기 생성
            if (_currentHoveredSlot != null && _selectedRoomData != null && _selectedRoomData.roomPrefab != null)
            {
                _tempPreviewInstance = Instantiate(_selectedRoomData.roomPrefab);
               _tempPreviewInstance.transform.SetParent(_currentHoveredSlot.transform, false); // 슬롯의 자식으로 설정
                _tempPreviewInstance.transform.localScale = Vector3.one; // 초기 스케일은 1,1,1로 설정
                _tempPreviewInstance.name = _selectedRoomData.roomName + "_TempPreview";
                _tempPreviewInstance.transform.position = _currentHoveredSlot.transform.position;

                

                SetPreviewCollidersActive(false); // 미리보기는 충돌 없음

                // TODO: 미리보기의 Sorting Layer / Order in Layer 설정 필요
                // SpriteRenderer[] srs = _tempPreviewInstance.GetComponentsInChildren<SpriteRenderer>();
                // foreach(var sr in srs) { sr.sortingLayerName = "WorldPreview"; sr.sortingOrder = 10; }
                Debug.Log($"임시 미리보기 '{_tempPreviewInstance.name}' 생성 at '{_currentHoveredSlot.name}'.");
            }

            // 3. 드래그 중인 UI 아이템의 가시성 업데이트
            _sourceDragItem?.SetVisibilityBasedOnWorldPreview(IsWorldPreviewActive);
        }

        private void SetPreviewCollidersActive(bool active)
        {
            if (_tempPreviewInstance == null) return;
            Collider col = _tempPreviewInstance.GetComponent<Collider>();
            if (col != null) col.enabled = active;
            Collider2D col2D = _tempPreviewInstance.GetComponent<Collider2D>();
            if (col2D != null) col2D.enabled = active;
            // 자식 콜라이더들도 처리하려면 GetComponentsInChildren 사용
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
                Debug.LogError("PlacementManager: StartWorldPreviewPlacement에 전달된 인자가 null입니다.");
                return;
            }
            IsPlacingRoom = true;
            _selectedRoomData = roomData;
            _sourceDragItem = sourceItem; // 드래그 시작한 UI 아이템 저장

            // 시작 시에는 월드 미리보기가 없으므로 UI 아이템은 보임 (DraggableRoomItem에서 알파 조절)
            _sourceDragItem.SetVisibilityBasedOnWorldPreview(false);
            Debug.Log($"PlacementManager: '{_selectedRoomData.roomName}' 방 배치(월드 미리보기) 시작. 드래그 UI: '{_sourceDragItem.name}'.");
        }

        public void HandleWorldPreviewDrop(PointerEventData eventData)
        {
            if (!IsPlacingRoom || _selectedRoomData == null)
            {
                EndWorldPreviewPlacement(false); // 실패로 종료
                return;
            }
            Debug.Log("PlacementManager: HandleWorldPreviewDrop 호출됨.");

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
                    // 현재 표시 중인 임시 미리보기를 영구 배치로 사용
                    SetPreviewCollidersActive(true); // 실제 배치될 때는 콜라이더 활성화
                    finalDropTargetSlot.ConfirmTemporaryPreviewAsPlaced(_selectedRoomData, _tempPreviewInstance);
                    _tempPreviewInstance = null; // 이제 더 이상 임시가 아님, 소유권 이전
                    placedSuccessfully = true;
                }
                else // 임시 미리보기가 없거나 다른 슬롯에 있었다면 새로 생성
                {
                    if (_tempPreviewInstance != null) DestroyPreview(); // 잘못된 위치의 미리보기 제거

                    GameObject newRoomInstance = Instantiate(_selectedRoomData.roomPrefab);
                    newRoomInstance.transform.SetParent(finalDropTargetSlot.transform, false); // 슬롯의 자식으로 설정
                    newRoomInstance.transform.localScale = Vector3.one; // 초기 스케일은 1,1,1로 설정
                    newRoomInstance.name = _selectedRoomData.roomName + "_Placed";
                    
                    // PlaceRoom 내부에서 부모 및 위치 설정
                    if (finalDropTargetSlot.PlaceRoom(_selectedRoomData, newRoomInstance))
                    {
                        placedSuccessfully = true;
                    }
                }
            }
            else
            {
                Debug.LogWarning("PlacementManager: 유효한 RoomSlot에 드롭되지 않아 배치를 취소합니다.");
            }

            EndWorldPreviewPlacement(placedSuccessfully);
        }

        private void EndWorldPreviewPlacement(bool success)
        {
            IsPlacingRoom = false;

            DestroyPreview(); // 남아있는 임시 미리보기 정리

            if (_currentHoveredSlot != null)
            {
                _currentHoveredSlot.SetHoverState(false);
                _currentHoveredSlot = null;
            }

            // 드래그 시작했던 UI 아이템의 최종 상태 복원은 DraggableRoomItem.OnEndDrag/HandleDragCancellation에서 처리
            // 여기서는 IsPlacingRoom = false로 설정했으므로, DraggableRoomItem의 OnDrag 등에서 이를 참조하여 스스로 복원할 수 있음
            _sourceDragItem?.SetVisibilityBasedOnWorldPreview(false); // UI 아이템 확실히 다시 보이게

            _selectedRoomData = null;
            // _sourceDragItem은 DraggableRoomItem의 OnEndDrag에서 null로 만들거나 여기서 할 필요 없음.
            // 다음 드래그 시작 시 덮어쓰여짐.
            Debug.Log($"PlacementManager: 월드 미리보기 배치 모드 종료. 성공: {success}");
        }

        public void CancelWorldPreviewPlacement()
        {
            if (!IsPlacingRoom) return;
            Debug.Log("PlacementManager: 월드 미리보기 배치 취소됨.");

            _sourceDragItem?.HandleDragCancellation(); // UI 아이템에게 취소 알림 및 상태 복원 요청
            EndWorldPreviewPlacement(false); // 실패로 간주하고 모든 상태 정리
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
            // LayerMask를 사용하면 더 효율적입니다. 예: public LayerMask roomSlotLayer;
            // if (Physics.Raycast(ray, out hit, Mathf.Infinity, roomSlotLayer))
            if (Physics.Raycast(ray, out hit, Mathf.Infinity))
            {
                return hit.collider.GetComponent<RoomSlot>();
            }
            return null;
        }
    }
}
