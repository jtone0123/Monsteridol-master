using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;
using System.Collections.Generic;

public class ScheduleDropZone : MonoBehaviour
{
    [Header("���̾ƿ� ����")]
    public bool isQueueDropZone = false;
    public float itemSpacing = 10f;
    public float topPadding = 10f;
    public float horizontalPadding = 20f;

    [Header("������ ũ�� ����")]
    public bool fitWidthToDropZone = true;

    [Header("�ִϸ��̼� ����")]
    public float siblingAnimationDuration = 0.2f;

    private GameObject placeholder = null;
    private RectTransform placeholderRT = null;
    private LayoutElement placeholderLE = null;

    private List<Tween> activeItemTweens = new List<Tween>();
    // private Transform lastValidDropZone = null; // ���� �������� ���ó�� ��Ȯ���� �ʾ� �ּ� ó�� �Ǵ� ���� ���

    private void Awake()
    {
        
    }


    private struct ItemLayoutState
    {
        public RectTransform rt;
        public Vector2 initialAnchoredPos;
        public ItemLayoutState(RectTransform rt, Vector2 pos)
        {
            this.rt = rt;
            this.initialAnchoredPos = pos;
        }
    }

    public void NotifyItemDragStarted(DraggableScheduleItem draggedItem, int originalIndex)
    {
        if (!isQueueDropZone) return;
        CreatePlaceholder(draggedItem, originalIndex);
        RefreshLayout(false);
    }

    public void NotifyItemDragEntered(DraggableScheduleItem draggedItem)
    {
        if (!isQueueDropZone) return;
        if (placeholder == null)
        {
            CreatePlaceholder(draggedItem, CalculatePlaceholderSiblingIndex(draggedItem.RectTransform.position));
        }
        if (!placeholder.activeSelf) placeholder.SetActive(true); // �ʿ�� Ȱ��ȭ

        if (placeholder.transform.parent != transform)
        {
            placeholder.transform.SetParent(transform);
        }
        int newIndex = CalculatePlaceholderSiblingIndex(draggedItem.RectTransform.position);
        placeholder.transform.SetSiblingIndex(newIndex);
        RefreshLayout(true);
    }

    public void NotifyItemDraggingOver(DraggableScheduleItem draggedItem, Vector2 mouseScreenPosition)
    {
        if (!isQueueDropZone || placeholder == null || !placeholder.activeSelf) return;

        int newPlaceholderIndex = CalculatePlaceholderSiblingIndex(mouseScreenPosition);
        if (placeholder.transform.GetSiblingIndex() != newPlaceholderIndex)
        {
            placeholder.transform.SetSiblingIndex(newPlaceholderIndex);
            RefreshLayout(true);
        }
    }

    public void NotifyItemDragExited(DraggableScheduleItem draggedItem)
    {
        if (!isQueueDropZone) return;
        if (placeholder != null)
        {
            DestroyPlaceholder(); // ���ο��� SetActive(false) �� Destroy
            RefreshLayout(true);
        }
    }

    public void HandleItemDrop(DraggableScheduleItem droppedItem)
    {
        if (!isQueueDropZone)
        {
            DestroyPlaceholder();
            return;
        }

        if (placeholder != null && placeholder.activeSelf) // Placeholder�� Ȱ�� ������ ���� �� ��ġ ���
        {
            droppedItem.transform.SetParent(transform);
            droppedItem.transform.SetSiblingIndex(placeholder.transform.GetSiblingIndex());
            DestroyPlaceholder();
        }
        else // Placeholder�� ���ų� ��Ȱ���̸� �� �ڿ� �߰� (���� ó��)
        {
            droppedItem.transform.SetParent(transform);
            droppedItem.transform.SetAsLastSibling();
            Debug.LogWarning($"[{gameObject.name}] HandleItemDrop: Placeholder�� ��ȿ���� �ʾ� '{droppedItem.ItemNameDebug}'��(��) �� �ڿ� ��ġ�մϴ�.");
            if (placeholder != null) DestroyPlaceholder(); // Ȥ�� �� ��Ȱ�� placeholder ����
        }
        RefreshLayout(false);
    }

    private void CreatePlaceholder(DraggableScheduleItem sourceItem, int siblingIndex)
    {
        if (placeholder != null) Destroy(placeholder); // ���� placeholder Ȯ���� ����

        placeholder = new GameObject("Placeholder_Visible");
        placeholder.transform.SetParent(transform);

        placeholderRT = placeholder.AddComponent<RectTransform>();
        placeholderLE = placeholder.AddComponent<LayoutElement>();

        RectTransform sourceRT = sourceItem.RectTransform;
        LayoutElement sourceLE = sourceItem.GetComponent<LayoutElement>();

        float targetWidth, targetHeight;
        if (sourceLE != null && sourceLE.enabled)
        {
            targetHeight = sourceLE.preferredHeight > 0 ? sourceLE.preferredHeight : sourceRT.sizeDelta.y;
            targetWidth = sourceLE.preferredWidth > 0 ? sourceLE.preferredWidth : sourceRT.sizeDelta.x;
        }
        else
        {
            targetHeight = sourceRT.sizeDelta.y;
            targetWidth = sourceRT.sizeDelta.x;
        }

        if (fitWidthToDropZone)
        {
            RectTransform parentRect = GetComponent<RectTransform>();
            targetWidth = parentRect.rect.width - horizontalPadding;
        }

        placeholderRT.sizeDelta = new Vector2(targetWidth, targetHeight);
        placeholderLE.preferredWidth = targetWidth;
        placeholderLE.preferredHeight = targetHeight;

        placeholderRT.anchorMin = new Vector2(0.5f, 1f);
        placeholderRT.anchorMax = new Vector2(0.5f, 1f);
        placeholderRT.pivot = new Vector2(0.5f, 1f);
        placeholderRT.localScale = Vector3.one;

        Image phImage = placeholder.AddComponent<Image>();
        phImage.color = new Color(0.5f, 0.5f, 1f, 0.4f);
        phImage.raycastTarget = false;

        placeholder.transform.SetSiblingIndex(siblingIndex);
        placeholder.SetActive(true); // ���� �� �ٷ� Ȱ��ȭ
    }

    private void DestroyPlaceholder()
    {
        if (placeholder != null)
        {
            KillActiveItemTweens(); // ���õ� �ٸ� ������ �ִϸ��̼� ����
            placeholder.SetActive(false); // Destroy ���� ��Ȱ��ȭ�Ͽ� ���̾ƿ� ��꿡�� ��� ����
            Destroy(placeholder.gameObject); // GameObject�� �ı�
            placeholder = null; // ������ null�� ����
            placeholderRT = null;
            placeholderLE = null;
        }
    }

    private int CalculatePlaceholderSiblingIndex(Vector2 referencePos)
    {
        int newIndex = transform.childCount;
        // placeholder�� �̹� �� ������� �ڽ��̰� Ȱ�� ���¶��, childCount���� 1�� ���� ��
        if (placeholder != null && placeholder.transform.parent == transform && placeholder.activeSelf)
        {
            newIndex = transform.childCount - 1;
        }
        else if (placeholder != null && placeholder.transform.parent == transform && !placeholder.activeSelf)
        {
            // placeholder�� �ڽ������� ��Ȱ�� ���¶��, ���� Ȱ�� ������ �� �������� ����ؾ� ��
            // �Ǵ�, �� �Լ��� placeholder�� Ȱ���� ���� ȣ��ȴٰ� ������ �� ����
            // ���⼭�� �ϴ� Ȱ�� placeholder ����
        }


        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            // placeholder�� ��꿡�� ���� (�ڱ� �ڽŰ� �� ����)
            // ����, ��Ȱ�� �ڽ��� ���̾ƿ��� ������ ���� �����Ƿ� �ǳʶ�
            if (!child.gameObject.activeSelf || (placeholder != null && child == placeholder.transform)) continue;

            RectTransform childRect = child.GetComponent<RectTransform>();
            if (childRect == null) continue;

            Vector3[] childCorners = new Vector3[4];
            childRect.GetWorldCorners(childCorners);
            float childCenterY_World = (childCorners[1].y + childCorners[0].y) / 2f;

            if (referencePos.y > childCenterY_World)
            {
                newIndex = i;
                // ���� placeholder�� newIndex���� �ڿ� �־��ٸ�, newIndex�� �״�� ����
                // ���� placeholder�� newIndex���� �տ� �־��ٸ�, newIndex�� i�� �Ǿ�� ��
                // ���� ������ placeholder�� �����ϰ� ������ �����۵� ������ �ε����� ã�� �Ϳ� �����
                // placeholder�� �� '���ڸ�'�� �ε����� ã�� ��
                if (placeholder != null && placeholder.transform.parent == transform && placeholder.activeSelf && i > placeholder.transform.GetSiblingIndex())
                {
                    // �� ���� placeholder�� i��° �����ۺ��� ���� �־��µ�,
                    // �巡�� �������� i��° �����ۺ��� �� ���� �ִٴ� �ǹ� -> placeholder�� i ���� ���� ��
                }
                break;
            }
        }
        return newIndex;
    }

    public void RefreshLayout(bool animate,bool isUseTween = false)
    {
        KillActiveItemTweens();
        // Ȱ��ȭ�� �ڽĸ� ī��Ʈ�Ͽ� ���� �������� ���� �� ���ʿ��� ��� ����
        int activeChildCount = 0;
        for (int i = 0; i < transform.childCount; i++)
        {
            if (transform.GetChild(i).gameObject.activeSelf) activeChildCount++;
        }
        if (activeChildCount == 0) return;

        float currentYPosition = -topPadding;
        RectTransform parentRect = GetComponent<RectTransform>();

        List<ItemLayoutState> initialStates = new List<ItemLayoutState>();
        if (animate)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                RectTransform childRT_anim = transform.GetChild(i) as RectTransform;
                if (childRT_anim != null && childRT_anim.gameObject.activeSelf)
                {
                    initialStates.Add(new ItemLayoutState(childRT_anim, childRT_anim.anchoredPosition));
                }
            }
        }

        // SiblingIndex ������� ��ȸ�ϸ� 'Ȱ��ȭ��' �ڽĵ��� ��ġ ��� �� ����
        for (int i = 0; i < transform.childCount; i++)
        {
            RectTransform childRT = transform.GetChild(i) as RectTransform;
            // ��Ȱ��ȭ�� �ڽ� (��: ��� DestroyPlaceholder���� SetActive(false)�� placeholder)�� ���̾ƿ� ��� �� �ִϸ��̼ǿ��� ����
            if (childRT == null || !childRT.gameObject.activeSelf) continue;

            float itemHeight = 0;
            float itemWidth = 0;
            LayoutElement childLE = childRT.GetComponent<LayoutElement>();

            if (childLE != null && childLE.enabled)
            {
                itemHeight = childLE.preferredHeight > 0 ? childLE.preferredHeight : childRT.sizeDelta.y;
                itemWidth = childLE.preferredWidth > 0 ? childLE.preferredWidth : childRT.sizeDelta.x;
            }
            else
            {
                itemHeight = childRT.sizeDelta.y;
                itemWidth = childRT.sizeDelta.x;
            }

            Vector2 newSize = childRT.sizeDelta;
            if (fitWidthToDropZone)
            {
                newSize.x = parentRect.rect.width - horizontalPadding;
            }
            else
            {
                newSize.x = itemWidth;
            }
            newSize.y = itemHeight;
            childRT.sizeDelta = newSize;

            childRT.anchorMin = new Vector2(0.5f, 1f);
            childRT.anchorMax = new Vector2(0.5f, 1f);
            childRT.pivot = new Vector2(0.5f, 1f);

            Vector2 targetAnchoredPos = new Vector2(0, currentYPosition);

            // Placeholder ��ü�� �ִϸ��̼����� �ʰ� ��� ��ġ�� �̵�
            if (placeholder != null && childRT.gameObject == placeholder)
            {
                childRT.anchoredPosition = targetAnchoredPos;
            }
            else if (animate)
            {
                ItemLayoutState? foundState = null;
                foreach (var state in initialStates) { if (state.rt == childRT) { foundState = state; break; } }

                Vector2 startPos = foundState.HasValue ? foundState.Value.initialAnchoredPos : childRT.anchoredPosition;

                if (Vector2.Distance(startPos, targetAnchoredPos) > 0.01f)
                {
                    Tween t = null; 
                 
                    if(isUseTween)//���Ȱ� ������ ��
                    {
                       t = childRT.DOAnchorPos(targetAnchoredPos, siblingAnimationDuration)
                       .SetEase(Ease.OutQuad)
                       .OnKill(() => { /* �ʿ��� ���� �۾� */ });
                    }
                    else
                    {
                        t = childRT.DOAnchorPos(targetAnchoredPos, siblingAnimationDuration)
                     .SetEase(Ease.OutQuad)
                     .OnKill(() => { /* �ʿ��� ���� �۾� */ });
                    }

                        activeItemTweens.Add(t);
                }
                else
                {
                    childRT.anchoredPosition = targetAnchoredPos;
                }
            }
            else
            {
                childRT.anchoredPosition = targetAnchoredPos;
            }
            // Ȱ��ȭ�� �����۸� Y ��ġ ������ ����
            currentYPosition -= (itemHeight + itemSpacing);
        }
    }

    public void RefreshLayoutAfterItemReturn()
    {
        RefreshLayout(true);
    }

    private void KillActiveItemTweens()
    {
        foreach (Tween t in activeItemTweens)
        {
            if (t != null && t.IsActive())
            {
                t.Kill(false);
            }
        }
        activeItemTweens.Clear();
    }
}
