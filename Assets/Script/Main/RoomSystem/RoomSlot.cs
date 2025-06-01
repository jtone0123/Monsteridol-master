// RoomSlot.cs
// ���� �̸� ��ġ�Ǿ� ���� �Ǽ��� �� �ִ� �� '����'�� ����մϴ�.
// PlacementManager�� Raycast�� ���� �����ϰ�, �� ��ũ��Ʈ�� �޼��带 ȣ���Ͽ� ���¸� �����մϴ�.
using UnityEngine;

namespace RoomPlacementSystem
{
    public class RoomSlot : MonoBehaviour
    {
        [Tooltip("���� �� ������ ��� ������ �����Դϴ�.")]
        public bool IsOccupied { get; private set; } = false;
        [Tooltip("���� �� ���Կ� ��ġ�� ���� �������Դϴ�.")]
        public RoomData OccupyingRoomData { get; private set; }
        [Tooltip("���� �� ���Կ� ��ġ�� ���� ���� ���ӿ�����Ʈ �ν��Ͻ��Դϴ�.")]
        public GameObject PlacedRoomInstance { get; private set; }

        private SpriteRenderer _slotVisualFeedbackRenderer;
        private Color _originalColor;
        private Vector2 _slotWorldSize = Vector2.one;

        void Awake()
        {
            // ������ �ð��� ǥ��(SpriteRenderer) �� ũ�� ���� �ʱ�ȭ
            _slotVisualFeedbackRenderer = GetComponent<SpriteRenderer>();
            if (_slotVisualFeedbackRenderer != null)
            {
                _originalColor = _slotVisualFeedbackRenderer.color;
                // SpriteRenderer�� bounds�� ����Ͽ� �ʱ� ���� ũ�� ���� (���� ������ ����� ũ��)
                _slotWorldSize = _slotVisualFeedbackRenderer.bounds.size;
            }
            else
            {
                // SpriteRenderer�� ���ٸ� Collider �������� ũ�� ���� �õ�
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
                        // 3D Collider�� ���, X�� Y ũ�⸸ ��� (2D ���� ��� ����)
                        _slotWorldSize = new Vector2(col3D.bounds.size.x, col3D.bounds.size.y);
                    }
                    else
                    {
                        Debug.LogWarning($"RoomSlot '{gameObject.name}'�� SpriteRenderer�� Collider�� ���� ��Ȯ�� ũ�⸦ �� �� �����ϴ�. �⺻ ũ��(1,1)�� ����մϴ�.");
                    }
                }
            }
        }

        // PlacementManager�� �� ���� ���� ���콺�� �ִٰ� �Ǵ����� �� ȣ�� (�ð��� �ǵ���)
        public void SetHoverState(bool isHovered)
        {
            if (_slotVisualFeedbackRenderer != null)
            {
                // ȣ�� ���¿� ���� ���� ���� ���� (��: ���� �� �����, ������� �ʷϻ�, ȣ�� �ƴϸ� ���� ��)
                _slotVisualFeedbackRenderer.color = isHovered ? (IsOccupied ? Color.yellow : Color.green) : _originalColor;
            }
        }

        // ���� �� ������ �ν��Ͻ��� �޾� ���Կ� ��ġ (���� ��ġ)
        public bool PlaceRoom(RoomData roomDataToPlace, GameObject roomInstanceToAssign)
        {
            if (IsOccupied) return false; // �̹� ������ ��� ��ġ �Ұ�
            if (roomDataToPlace == null || roomInstanceToAssign == null)
            {
                Debug.LogError("PlaceRoom ȣ�� �� RoomData �Ǵ� roomInstanceToAssign�� null�Դϴ�.");
                return false;
            }

            PlacedRoomInstance = roomInstanceToAssign;
            PlacedRoomInstance.transform.SetParent(transform); // ������ �θ�� ����
            PlacedRoomInstance.transform.position = transform.position; // ��ġ ����ȭ
            // �������� PlacementManager�� AdjustPrefabScaleToSlot���� �̹� ���Կ� �°� �����Ǿ��� ����

            IsOccupied = true;
            OccupyingRoomData = roomDataToPlace;
            Debug.Log($"'{OccupyingRoomData.roomName}'��(��) ���� '{gameObject.name}'�� **����������** ��ġ�Ǿ����ϴ�.");

            if (_slotVisualFeedbackRenderer != null) _slotVisualFeedbackRenderer.enabled = false; // �� ��ġ �� ���� ���־� ����
            return true;
        }

        // �ӽ� �̸����� �ν��Ͻ��� ���� ��ġ�� ������ Ȯ���ϴ� �޼���
        public bool ConfirmTemporaryPreviewAsPlaced(RoomData roomData, GameObject tempPreviewInstance)
        {
            if (IsOccupied)
            {
                Debug.LogError($"���� '{gameObject.name}'�� �̹� �����Ǿ� �־� �ӽ� �̸����⸦ Ȯ���� �� �����ϴ�.");
                if (tempPreviewInstance != null && tempPreviewInstance != PlacedRoomInstance) Destroy(tempPreviewInstance);
                return false;
            }
            if (roomData == null || tempPreviewInstance == null)
            {
                Debug.LogError("ConfirmTemporaryPreviewAsPlaced ȣ�� �� RoomData �Ǵ� tempPreviewInstance�� null�Դϴ�.");
                return false;
            }

            PlacedRoomInstance = tempPreviewInstance;
            if (PlacedRoomInstance.transform.parent != transform) PlacedRoomInstance.transform.SetParent(transform);
            PlacedRoomInstance.transform.position = transform.position;

            IsOccupied = true;
            OccupyingRoomData = roomData;
            Debug.Log($"'{OccupyingRoomData.roomName}'��(��) ���� '{gameObject.name}'�� �ӽ� �̸����⿡�� ���� ��ġ�� Ȯ���Ǿ����ϴ�.");

            if (_slotVisualFeedbackRenderer != null) _slotVisualFeedbackRenderer.enabled = false;
            return true;
        }

        // ���Կ� ��ġ�� ���� ����
        public void ClearSlot()
        {
            if (!IsOccupied) return;
            if (PlacedRoomInstance != null) Destroy(PlacedRoomInstance);
            PlacedRoomInstance = null;
            Debug.Log($"���� '{gameObject.name}'���� '{OccupyingRoomData?.roomName ?? "�� �� ���� ��"}' ���ŵ�.");
            IsOccupied = false;
            OccupyingRoomData = null;
            if (_slotVisualFeedbackRenderer != null)
            {
                _slotVisualFeedbackRenderer.enabled = true; // ���� ���־� �ٽ� Ȱ��ȭ
                SetHoverState(false); // ȣ�� ���� �ƴ�(���� ����)���� ����
            }
        }

        // ������ ���� ���� ���� ũ�⸦ ��ȯ
        public Vector2 GetSlotWorldSize()
        {
            // ��Ÿ�ӿ� ���� ũ�Ⱑ ������� �ʴ´ٰ� ����. ����ȴٸ� ���⼭ bounds�� �ٽ� ����ؾ� ��.
            return _slotWorldSize;
        }

        // ������ �ν��Ͻ��� �������� �� ���� ũ�⿡ �°� ���� (Aspect Fit)
        public void AdjustPrefabScaleToSlot(GameObject prefabInstance, RoomData roomDataForPrefab)
        {
            if (prefabInstance == null || roomDataForPrefab == null || roomDataForPrefab.roomPrefab == null) return;

            // �������� �⺻ ���� ũ�⸦ �˾ƾ� ��.
            // ���� ���� ����� RoomData�� �� ������ �����ϰų�, ������ ��ü�� ���� ũ�� ������ ���� ������Ʈ�� �δ� ��.
            // ���⼭�� ������ ��Ʈ�� Renderer bounds�� ����ϵ�, localScale (1,1,1)�� ���� ũ�⸦ �������� ������ �õ�.
            Renderer prefabRenderer = prefabInstance.GetComponentInChildren<Renderer>();
            if (prefabRenderer == null)
            {
                Debug.LogWarning($"'{roomDataForPrefab.roomName}' ������ �ν��Ͻ����� Renderer�� ã�� �� ���� ������ ������ �ǳ�<0xEB><0xA9><0xB4>�ϴ�. �⺻ ������(1,1,1) ����.");
                prefabInstance.transform.localScale = Vector3.one;
                return;
            }

            Vector3 originalInstanceScale = prefabInstance.transform.localScale; // ���� ������ ����
            prefabInstance.transform.localScale = Vector3.one; // ����� ���� ��� (1,1,1)�� ����
            Vector3 prefabBaseWorldSize = prefabRenderer.bounds.size; // localScale (1,1,1)�� ���� ���� ũ��
            prefabInstance.transform.localScale = originalInstanceScale; // ���� �����Ϸ� ���� (���� ���� �ƴ�)

            Vector2 slotSize = GetSlotWorldSize();

            if (slotSize.x <= 0 || slotSize.y <= 0 || prefabBaseWorldSize.x <= 0 || prefabBaseWorldSize.y <= 0)
            {
                // Debug.LogWarning($"ũ�� ���� ������ ������ ���� �Ұ�. Slot: {slotSize}, PrefabBase: {prefabBaseWorldSize}");
                prefabInstance.transform.localScale = Vector3.one; // ���� �� �⺻ ������
                return;
            }

            float scaleRatioX = slotSize.x / prefabBaseWorldSize.x;
            float scaleRatioY = slotSize.y / prefabBaseWorldSize.y;
            float finalUniformScaleFactor = Mathf.Min(scaleRatioX, scaleRatioY); // Aspect Fit

            // �������� ���� ������ ������(originalInstanceScale)�� ���� ������ ����.
            // ���� �������� �̹� (1,1,1)���� ������ ���� ũ�⸦ �����ٸ� originalInstanceScale�� Vector3.one�� ������ ����.
            prefabInstance.transform.localScale = new Vector3(
                originalInstanceScale.x * finalUniformScaleFactor,
                originalInstanceScale.y * finalUniformScaleFactor,
                originalInstanceScale.z * finalUniformScaleFactor // Z �����ϵ� ������ �°� �Ǵ� ������ ���
            );
        }
    }
}
