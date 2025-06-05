using UnityEngine;
using UnityEngine.EventSystems;

public class DragableItem : MonoBehaviour,IBeginDragHandler, IEndDragHandler,IDragHandler
{
    [SerializeField]
    public ItemData itemData;
    public RectTransform RectTransform { get; private set; }
    private CanvasGroup canvasGroup;
    private Canvas rootCanvas;
    private int originalSiblingIndex;
    private Transform originalParent;

    public string ItemNameDebug
    {
        get { return itemData != null ? itemData.itemName : gameObject.name; }
    }

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        
        if(canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        RectTransform = GetComponent<RectTransform>();
        rootCanvas = GetComponentInParent<Canvas>();
        if (rootCanvas == null)
        {
            Debug.LogError($"[{ItemNameDebug}] 최상위 Canvas를 찾을 수 없어 드래그를 시작할 수 없습니다.");
            eventData.pointerDrag = null;
            return;
        }
        Debug.Log($"[{ItemNameDebug}] OnBeginDrag 시작 from {transform.parent.name}");

        originalParent = transform.parent;
        originalSiblingIndex = transform.GetSiblingIndex();

        canvasGroup.blocksRaycasts = false;
        //canvasGroup.alpha = 0.7f;

        transform.SetParent(rootCanvas.transform, true);
        transform.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData eventData)
    {
        RectTransform.anchoredPosition += eventData.delta / rootCanvas.scaleFactor;

        
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;

        transform.SetParent(originalParent);
        transform.SetSiblingIndex(originalSiblingIndex);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
