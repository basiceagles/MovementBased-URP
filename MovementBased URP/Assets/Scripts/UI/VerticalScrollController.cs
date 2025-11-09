using UnityEngine;
using UnityEngine.UI;

public class VerticalScrollController : MonoBehaviour
{
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private RectTransform content;
    [SerializeField] private VerticalLayoutGroup verticalLayout;

    void Start()
    {
        EnsureTopLeftAlignment();
    }

    void Update()
    {
        // Optional: Keep content at top-left when adding new items
        if (content.hasChanged)
        {
            EnsureTopLeftAlignment();
            content.hasChanged = false;
        }
    }

    private void EnsureTopLeftAlignment()
    {
        // Force top-left alignment
        verticalLayout.childAlignment = TextAnchor.UpperLeft;

        // Reset content position to top
        content.anchoredPosition = new Vector2(content.anchoredPosition.x, 0);

        // Ensure pivot is top-left
        content.pivot = new Vector2(0, 1);
        content.anchorMin = new Vector2(0, 1);
        content.anchorMax = new Vector2(0, 1);
    }

    // Call this when adding new items to scroll to top
    [ContextMenu("Call My Method")]
    public void ScrollToTop()
    {
        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 1f;
    }

    // Call this when adding new items to scroll to bottom (new items)
    public void ScrollToNewItem()
    {
        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0f;
    }
}