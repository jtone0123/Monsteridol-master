// RoomSlot.cs
// 씬에 미리 배치되어 방이 건설될 수 있는 각 '슬롯'을 담당합니다.
// PlacementManager가 Raycast로 직접 감지하고, 이 스크립트의 메서드를 호출하여 상태를 변경합니다.
using UnityEngine;

namespace RoomPlacementSystem
{
    public class RoomSlot : MonoBehaviour
    {
        [Tooltip("현재 이 슬롯이 사용 중인지 여부입니다.")]
        public bool IsOccupied { get; private set; } = false;
        [Tooltip("현재 이 슬롯에 배치된 방의 데이터입니다.")]
        public RoomData OccupyingRoomData { get; private set; }
        [Tooltip("현재 이 슬롯에 배치된 방의 실제 게임오브젝트 인스턴스입니다.")]
        public GameObject PlacedRoomInstance { get; private set; }

        private SpriteRenderer _slotVisualFeedbackRenderer;
        private Color _originalColor;
        private Vector2 _slotWorldSize = Vector2.one;

        void Awake()
        {
            // 슬롯의 시각적 표현(SpriteRenderer) 및 크기 정보 초기화
            _slotVisualFeedbackRenderer = GetComponent<SpriteRenderer>();
            if (_slotVisualFeedbackRenderer != null)
            {
                _originalColor = _slotVisualFeedbackRenderer.color;
                // SpriteRenderer의 bounds를 사용하여 초기 슬롯 크기 설정 (월드 스케일 적용된 크기)
                _slotWorldSize = _slotVisualFeedbackRenderer.bounds.size;
            }
            else
            {
                // SpriteRenderer가 없다면 Collider 기준으로 크기 설정 시도
                Collider2D col2D = GetComponent<Collider2D>();
                if (col2D != null)
                {
                    _slotWorldSize = col2D.bounds.size;
                }
                else
                {
                    Collider col3D = GetComponent<Collider>();
                    if (col3D != null)
                    {
                        // 3D Collider의 경우, X와 Y 크기만 사용 (2D 게임 평면 가정)
                        _slotWorldSize = new Vector2(col3D.bounds.size.x, col3D.bounds.size.y);
                    }
                    else
                    {
                        Debug.LogWarning($"RoomSlot '{gameObject.name}'에 SpriteRenderer나 Collider가 없어 정확한 크기를 알 수 없습니다. 기본 크기(1,1)을 사용합니다.");
                    }
                }
            }
        }

        // PlacementManager가 이 슬롯 위에 마우스가 있다고 판단했을 때 호출 (시각적 피드백용)
        public void SetHoverState(bool isHovered)
        {
            if (_slotVisualFeedbackRenderer != null)
            {
                // 호버 상태에 따라 슬롯 색상 변경 (예: 점유 시 노란색, 비었으면 초록색, 호버 아니면 원래 색)
                _slotVisualFeedbackRenderer.color = isHovered ? (IsOccupied ? Color.yellow : Color.green) : _originalColor;
            }
        }

        // 실제 방 프리팹 인스턴스를 받아 슬롯에 배치 (영구 배치)
        public bool PlaceRoom(RoomData roomDataToPlace, GameObject roomInstanceToAssign)
        {
            if (IsOccupied) return false; // 이미 점유된 경우 배치 불가
            if (roomDataToPlace == null || roomInstanceToAssign == null)
            {
                Debug.LogError("PlaceRoom 호출 시 RoomData 또는 roomInstanceToAssign이 null입니다.");
                return false;
            }

            PlacedRoomInstance = roomInstanceToAssign;
            PlacedRoomInstance.transform.SetParent(transform); // 슬롯을 부모로 설정
            PlacedRoomInstance.transform.position = transform.position; // 위치 동기화
            // 스케일은 PlacementManager의 AdjustPrefabScaleToSlot에서 이미 슬롯에 맞게 조절되었을 것임

            IsOccupied = true;
            OccupyingRoomData = roomDataToPlace;
            Debug.Log($"'{OccupyingRoomData.roomName}'이(가) 슬롯 '{gameObject.name}'에 **영구적으로** 배치되었습니다.");

            if (_slotVisualFeedbackRenderer != null) _slotVisualFeedbackRenderer.enabled = false; // 방 배치 후 슬롯 비주얼 숨김
            return true;
        }

        // 임시 미리보기 인스턴스를 실제 배치된 방으로 확정하는 메서드
        public bool ConfirmTemporaryPreviewAsPlaced(RoomData roomData, GameObject tempPreviewInstance)
        {
            if (IsOccupied)
            {
                Debug.LogError($"슬롯 '{gameObject.name}'은 이미 점유되어 있어 임시 미리보기를 확정할 수 없습니다.");
                if (tempPreviewInstance != null && tempPreviewInstance != PlacedRoomInstance) Destroy(tempPreviewInstance);
                return false;
            }
            if (roomData == null || tempPreviewInstance == null)
            {
                Debug.LogError("ConfirmTemporaryPreviewAsPlaced 호출 시 RoomData 또는 tempPreviewInstance가 null입니다.");
                return false;
            }

            PlacedRoomInstance = tempPreviewInstance;
            if (PlacedRoomInstance.transform.parent != transform) PlacedRoomInstance.transform.SetParent(transform);
            PlacedRoomInstance.transform.position = transform.position;

            IsOccupied = true;
            OccupyingRoomData = roomData;
            Debug.Log($"'{OccupyingRoomData.roomName}'이(가) 슬롯 '{gameObject.name}'에 임시 미리보기에서 영구 배치로 확정되었습니다.");

            if (_slotVisualFeedbackRenderer != null) _slotVisualFeedbackRenderer.enabled = false;
            return true;
        }

        // 슬롯에 배치된 방을 제거
        public void ClearSlot()
        {
            if (!IsOccupied) return;
            if (PlacedRoomInstance != null) Destroy(PlacedRoomInstance);
            PlacedRoomInstance = null;
            Debug.Log($"슬롯 '{gameObject.name}'에서 '{OccupyingRoomData?.roomName ?? "알 수 없는 방"}' 제거됨.");
            IsOccupied = false;
            OccupyingRoomData = null;
            if (_slotVisualFeedbackRenderer != null)
            {
                _slotVisualFeedbackRenderer.enabled = true; // 슬롯 비주얼 다시 활성화
                SetHoverState(false); // 호버 상태 아님(원래 색상)으로 복원
            }
        }

        // 슬롯의 월드 공간 기준 크기를 반환
        public Vector2 GetSlotWorldSize()
        {
            // 런타임에 슬롯 크기가 변경되지 않는다고 가정. 변경된다면 여기서 bounds를 다시 계산해야 함.
            return _slotWorldSize;
        }

        // 프리팹 인스턴스의 스케일을 이 슬롯 크기에 맞게 조절 (Aspect Fit)
        public void AdjustPrefabScaleToSlot(GameObject prefabInstance, RoomData roomDataForPrefab)
        {
            if (prefabInstance == null || roomDataForPrefab == null || roomDataForPrefab.roomPrefab == null) return;

            // 프리팹의 기본 월드 크기를 알아야 함.
            // 가장 좋은 방법은 RoomData에 이 정보를 저장하거나, 프리팹 자체에 기준 크기 정보를 갖는 컴포넌트를 두는 것.
            // 여기서는 프리팹 루트의 Renderer bounds를 사용하되, localScale (1,1,1)일 때의 크기를 기준으로 삼으려 시도.
            Renderer prefabRenderer = prefabInstance.GetComponentInChildren<Renderer>();
            if (prefabRenderer == null)
            {
                Debug.LogWarning($"'{roomDataForPrefab.roomName}' 프리팹 인스턴스에서 Renderer를 찾을 수 없어 스케일 조정을 건너<0xEB><0xA9><0xB4>니다. 기본 스케일(1,1,1) 적용.");
                prefabInstance.transform.localScale = Vector3.one;
                return;
            }

            Vector3 originalInstanceScale = prefabInstance.transform.localScale; // 현재 스케일 저장
            prefabInstance.transform.localScale = Vector3.one; // 계산을 위해 잠시 (1,1,1)로 설정
            Vector3 prefabBaseWorldSize = prefabRenderer.bounds.size; // localScale (1,1,1)일 때의 월드 크기
            prefabInstance.transform.localScale = originalInstanceScale; // 원래 스케일로 복원 (아직 최종 아님)

            Vector2 slotSize = GetSlotWorldSize();

            if (slotSize.x <= 0 || slotSize.y <= 0 || prefabBaseWorldSize.x <= 0 || prefabBaseWorldSize.y <= 0)
            {
                // Debug.LogWarning($"크기 정보 오류로 스케일 조정 불가. Slot: {slotSize}, PrefabBase: {prefabBaseWorldSize}");
                prefabInstance.transform.localScale = Vector3.one; // 오류 시 기본 스케일
                return;
            }

            float scaleRatioX = slotSize.x / prefabBaseWorldSize.x;
            float scaleRatioY = slotSize.y / prefabBaseWorldSize.y;
            float finalUniformScaleFactor = Mathf.Min(scaleRatioX, scaleRatioY); // Aspect Fit

            // 프리팹의 원래 디자인 스케일(originalInstanceScale)에 계산된 비율을 곱함.
            // 만약 프리팹이 이미 (1,1,1)에서 적절한 단위 크기를 가진다면 originalInstanceScale은 Vector3.one과 유사할 것임.
            prefabInstance.transform.localScale = new Vector3(
                originalInstanceScale.x * finalUniformScaleFactor,
                originalInstanceScale.y * finalUniformScaleFactor,
                originalInstanceScale.z * finalUniformScaleFactor // Z 스케일도 비율에 맞게 또는 고정값 사용
            );
        }
    }
}
