using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections; // �ڷ�ƾ ��� �� �ʿ� (���⼭�� Update ���)

public class DraggableScheduleItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    [SerializeField]
    public ScheduleData scheduleData;
    public RectTransform RectTransform { get; private set; }
    private CanvasGroup canvasGroup;
    private Transform originalParent;
    private int originalSiblingIndex;
    private Canvas rootCanvas;

    private bool isBeingDragged = false;
    private ScheduleDropZone currentDropZoneTarget = null;

    [Header("�� ���� ���� ���� ����")]
    [Tooltip("�� ������� ������ �ּ� �ð� (��)")]
    public float pressAndHoldDuration = 0.5f; // 0.5�� ���� ������ ������ ���� ǥ��

    private bool isPointerDown = false;      // ���� �����Ͱ� ���� ��������
    private float pointerDownTimer = 0f;     // �����Ͱ� ���� �ð�
    private bool isDescriptionCurrentlyShown = false; // �� �����ۿ� ���� ������ ǥ�õǾ�����

    public string ItemNameDebug
    {
        get { return scheduleData != null ? scheduleData.scheduleName : gameObject.name; }
    }

    void Awake()
    {
        RectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    void Update()
    {
        // "�� ������" ���� ó��
        if (isPointerDown && !isBeingDragged && !isDescriptionCurrentlyShown)
        {
            pointerDownTimer += Time.deltaTime;
            if (pointerDownTimer >= pressAndHoldDuration)
            {
                ShowItemDescription();
            }
        }
    }

    private void ShowItemDescription()
    {
        if (scheduleData != null && DescriptionUIManager.Instance != null)
        {
            Debug.Log($"[{ItemNameDebug}] �� ������ ����! ���� ǥ��.");
            DescriptionUIManager.Instance.ShowDescription(scheduleData, RectTransform);
            isDescriptionCurrentlyShown = true;
        }
    }

    private void HideItemDescription()
    {
        if (isDescriptionCurrentlyShown && DescriptionUIManager.Instance != null)
        {
            DescriptionUIManager.Instance.HideDescription();
            isDescriptionCurrentlyShown = false;
            Debug.Log($"[{ItemNameDebug}] ���� ����.");
        }
        // Ÿ�̸� �� ���� �ʱ�ȭ�� OnPointerUp ��� ��������� ó��
    }

    private void ResetPressAndHold()
    {
        isPointerDown = false;
        pointerDownTimer = 0f;
        // isDescriptionCurrentlyShown�� HideItemDescription���� ����
    }

    // --- IPointerDownHandler ���� ---
    public void OnPointerDown(PointerEventData eventData)
    {
        // �巡�װ� �ƴ�, �ܼ� Ŭ�� �Ǵ� �� ������ ������ �� ����
        if (!isBeingDragged) // �̹� �巡�� ���� �ƴ� ����
        {
            isPointerDown = true;
            pointerDownTimer = 0f;
            // isDescriptionCurrentlyShown = false; // ���⼭ false�� �ϸ�, ���� �� ���� �ٽ� �� �� �� ����. Hide���� ó��.
            Debug.Log($"[{ItemNameDebug}] OnPointerDown");
        }
    }

    // --- IPointerUpHandler ���� ---
    public void OnPointerUp(PointerEventData eventData)
    {
        Debug.Log($"[{ItemNameDebug}] OnPointerUp");
        ResetPressAndHold();
        HideItemDescription(); // ���� ���� ���� ����
    }

    // --- IPointerExitHandler ���� (���� ���������� ����) ---
    public void OnPointerExit(PointerEventData eventData)
    {
        // ���� ���·� ī�� ������ ����ٸ� "�� ������" ��� �� ���� ����
        if (isPointerDown) // isBeingDragged ������ �ʿ� ����, �巡�� �߿��� ���� ��� �� ����
        {
            Debug.Log($"[{ItemNameDebug}] OnPointerExit while pointer down. Cancelling press and hold.");
            ResetPressAndHold();
            HideItemDescription();
        }
    }


    // --- IBeginDragHandler ���� ---
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (RectTransform == null) RectTransform = GetComponent<RectTransform>();
        if (rootCanvas == null) rootCanvas = GetComponentInParent<Canvas>();

        if (rootCanvas == null)
        {
            Debug.LogError($"[{ItemNameDebug}] �ֻ��� Canvas�� ã�� �� ���� �巡�׸� ������ �� �����ϴ�.");
            eventData.pointerDrag = null;
            return;
        }

        Debug.Log($"[{ItemNameDebug}] OnBeginDrag ���� from {transform.parent.name}");

        // �巡�װ� ���۵Ǹ� "�� ������" ���� ���¸� ��� �ʱ�ȭ�ϰ� ���� ����
        ResetPressAndHold();
        HideItemDescription(); // �ſ� �߿�! �巡�� ���� �� ���� ��� ����

        isBeingDragged = true; // isPointerDown���� ���� true�� �����Ǿ�� Update ������ ������ ����
        originalParent = transform.parent;
        originalSiblingIndex = transform.GetSiblingIndex();

        canvasGroup.alpha = 0.7f;
        canvasGroup.blocksRaycasts = false;

        transform.SetParent(rootCanvas.transform, true);
        transform.SetAsLastSibling();

        originalParent.GetComponent<ScheduleDropZone>()?.NotifyItemDragStarted(this, originalSiblingIndex);
    }

    // --- IDragHandler ���� ---
    public void OnDrag(PointerEventData eventData)
    {
        if (!isBeingDragged || rootCanvas == null) return;

        RectTransform.anchoredPosition += eventData.delta / rootCanvas.scaleFactor;

        ScheduleDropZone newHoveredDropZone = null;
        GameObject pointerOverObject = eventData.pointerCurrentRaycast.gameObject;
        if (pointerOverObject != null)
        {
            newHoveredDropZone = pointerOverObject.GetComponent<ScheduleDropZone>();
            if (newHoveredDropZone == null && pointerOverObject.transform.parent != null)
            {
                newHoveredDropZone = pointerOverObject.transform.parent.GetComponent<ScheduleDropZone>();
            }
        }

        if (currentDropZoneTarget != newHoveredDropZone)
        {
            currentDropZoneTarget?.NotifyItemDragExited(this);
            newHoveredDropZone?.NotifyItemDragEntered(this);
            currentDropZoneTarget = newHoveredDropZone;
        }

        currentDropZoneTarget?.NotifyItemDraggingOver(this, eventData.position);
    }

    // --- IEndDragHandler ���� ---
    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isBeingDragged) return; // �̹� �巡�װ� �����ų� ���۵��� �ʾҴٸ� ����

        // isBeingDragged�� ���� false�� �����ؾ�, OnPointerUp ���� ȣ��� �� �ùٸ��� ����
        isBeingDragged = false;
        // ResetPressAndHold(); // OnPointerUp���� �̹� ó����, �Ǵ� ���⼭ �� �� �� ȣ���ص� ����

        Debug.Log($"[{ItemNameDebug}] OnEndDrag ����");

        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        ScheduleDropZone finalDropZone = currentDropZoneTarget;

        if (finalDropZone != null && finalDropZone.isQueueDropZone)
        {
            Debug.Log($"[{ItemNameDebug}] �������� '{finalDropZone.name}' (Queue Zone)�� ��ӵ�.");
            finalDropZone.HandleItemDrop(this);
        }
        else
        {
            Debug.Log($"[{ItemNameDebug}] �������� ��ȿ���� ���� �� �Ǵ� ��ť ���� ��ӵǾ� ���� ��ġ�� ����.");
            transform.SetParent(originalParent);
            transform.SetSiblingIndex(originalSiblingIndex);
            originalParent.GetComponent<ScheduleDropZone>()?.RefreshLayout(false);
        }

        currentDropZoneTarget?.NotifyItemDragExited(this);
        currentDropZoneTarget = null;
    }
}
