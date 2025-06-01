using UnityEngine;
using TMPro; // TextMeshProUGUI ��� ��. �Ϲ� Text ��� �� UnityEngine.UI
using UnityEngine.UI; // CanvasScaler ���� ���� ���� �߰�

public class DescriptionUIManager : MonoBehaviour
{
    public static DescriptionUIManager Instance { get; private set; }

    [Header("UI ����")]
    [Tooltip("������ ǥ���� TextMeshProUGUI �Ǵ� Text ������Ʈ")]
    public TextMeshProUGUI descriptionTextUI; // Inspector���� ����

    [Tooltip("���� UI ��ü�� ���δ� GameObject (Ȱ��ȭ/��Ȱ��ȭ �����)")]
    public GameObject descriptionPanel; // Inspector���� ����

    [Header("�г� ��ġ ����")]
    [Tooltip("�����ٷκ��� ���� �г��� ������ X�� ������")]
    public float offsetX = 10f;
    [Tooltip("�����ٷκ��� ���� �г��� ������ Y�� ������")]
    public float offsetY = 0f;
    [Tooltip("���� �г��� ȭ�� �����ڸ����� �󸶳� �������� �ϴ��� (��� ������)")]
    public float screenPadding = 10f;


    private RectTransform panelRectTransform;
    private Canvas mainCanvas; // �г� ��ġ ����� ���� �ֻ��� Canvas ����

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (descriptionPanel != null)
        {
            panelRectTransform = descriptionPanel.GetComponent<RectTransform>();
            if (panelRectTransform == null)
            {
                Debug.LogError("DescriptionUIManager: descriptionPanel�� RectTransform ������Ʈ�� �����ϴ�!");
            }

            // �ֻ��� Canvas ã�� (Screen Space Overlay ����)
            // 1. ���� ���� ������Ʈ �Ǵ� �θ𿡼� Canvas�� ���� ã�ƺ��ϴ�.
            mainCanvas = GetComponentInParent<Canvas>();

            // 2. GetComponentInParent�� ã�� ���߰�, Screen Space Overlay�� �ƴ϶��
            //    ������ CanvasScaler�� ���� Ȱ��ȭ�� Canvas�� ã�ƺ��ϴ�. (������ ����)
            if (mainCanvas == null || mainCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
            {
                // FindFirstObjectByType ��� (FindObjectOfType ��ü)
                CanvasScaler scaler = FindFirstObjectByType<CanvasScaler>();
                if (scaler != null)
                {
                    mainCanvas = scaler.GetComponent<Canvas>();
                }

                if (mainCanvas == null || mainCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
                {
                    Debug.LogError("DescriptionUIManager: Screen Space Overlay ����� �ֻ��� Canvas�� ã�� �� �����ϴ�. �г� ��ġ ��꿡 ������ ���� �� �ֽ��ϴ�. Inspector���� mainCanvas�� ���� �Ҵ��ϰų�, �� ������Ʈ�� �ùٸ� Canvas�� �ڽ����� Ȯ�����ּ���.");
                }
            }
            descriptionPanel.SetActive(false);
        }
        else
        {
            Debug.LogError("DescriptionUIManager: ���� UI ���(descriptionPanel)�� Inspector�� ������� �ʾҽ��ϴ�!");
        }
    }

    /// <summary>
    /// ������ ScheduleData�� ������ Ư�� ������ ���� UI�� ǥ���մϴ�.
    /// </summary>
    /// <param name="dataToShow">ǥ���� ������ ������</param>
    /// <param name="targetScheduleRect">���� �г��� ������ �� �������� RectTransform</param>
    public void ShowDescription(ScheduleData dataToShow, RectTransform targetScheduleRect)
    {
        if (dataToShow == null || panelRectTransform == null || descriptionTextUI == null || mainCanvas == null)
        {
            // ... (null üũ �� ��� �α״� ������ ����) ...
            HideDescription();
            return;
        }

        // 1. ���� �ؽ�Ʈ ����
        string displayText = $"<size=40>{dataToShow.description}</size>\n\n";
        
    
        descriptionTextUI.text = displayText;

        // 2. ���� �г� ũ�� ���� ������Ʈ (ContentSizeFitter ���� ���뿡 �°� ũ�⸦ ������ �� ��ġ ���)
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(panelRectTransform);

        // 3. ���� �г� ��ġ ��� (���� ������ ������ ����)
        Vector3[] scheduleWorldCorners = new Vector3[4];
        targetScheduleRect.GetWorldCorners(scheduleWorldCorners);

        Vector3 panelTargetWorldPos;
        bool showOnRightSide = false;

        Vector3 scheduleLeftCenterWorld = (scheduleWorldCorners[0] + scheduleWorldCorners[1]) / 2f;
        Vector3 scheduleRightCenterWorld = (scheduleWorldCorners[2] + scheduleWorldCorners[3]) / 2f;

        float panelWidth = panelRectTransform.rect.width * mainCanvas.scaleFactor;
        float panelHeight = panelRectTransform.rect.height * mainCanvas.scaleFactor;

        panelTargetWorldPos = new Vector3(
            scheduleLeftCenterWorld.x - ((1f - panelRectTransform.pivot.x) * panelWidth) - offsetX * mainCanvas.scaleFactor,
            scheduleLeftCenterWorld.y - ((0.5f - panelRectTransform.pivot.y) * panelHeight) + offsetY * mainCanvas.scaleFactor,
            0);

        if (panelTargetWorldPos.x - (panelRectTransform.pivot.x * panelWidth) < screenPadding)
        {
            showOnRightSide = true;
        }

        if (showOnRightSide)
        {
            panelTargetWorldPos = new Vector3(
                scheduleRightCenterWorld.x + (panelRectTransform.pivot.x * panelWidth) + offsetX * mainCanvas.scaleFactor,
                scheduleRightCenterWorld.y - ((0.5f - panelRectTransform.pivot.y) * panelHeight) + offsetY * mainCanvas.scaleFactor,
                0);

            if (panelTargetWorldPos.x + (1f - panelRectTransform.pivot.x) * panelWidth > Screen.width - screenPadding)
            {
                panelTargetWorldPos.x = Screen.width - screenPadding - ((1f - panelRectTransform.pivot.x) * panelWidth);
            }
        }

        float panelTopY = panelTargetWorldPos.y + (1f - panelRectTransform.pivot.y) * panelHeight;
        float panelBottomY = panelTargetWorldPos.y - panelRectTransform.pivot.y * panelHeight;

        if (panelTopY > Screen.height - screenPadding)
        {
            panelTargetWorldPos.y -= (panelTopY - (Screen.height - screenPadding));
        }
        if (panelBottomY < screenPadding)
        {
            panelTargetWorldPos.y += (screenPadding - panelBottomY);
        }

        panelRectTransform.position = panelTargetWorldPos;

        descriptionPanel.SetActive(true);
        Debug.Log($"'{dataToShow.scheduleName}' ������ ���� ǥ�� (���� ������ ��).");
    }

    public void HideDescription()
    {
        if (descriptionPanel != null && descriptionPanel.activeSelf)
        {
            descriptionPanel.SetActive(false);
            Debug.Log("������ ���� UI ����.");
        }
        else if (descriptionTextUI != null && descriptionTextUI.gameObject.activeSelf)
        {
            descriptionTextUI.gameObject.SetActive(false);
        }
    }
}
