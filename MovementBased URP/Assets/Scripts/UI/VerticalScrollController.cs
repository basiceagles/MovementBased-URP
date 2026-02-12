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
        if (content.hasChanged)
        {
            EnsureTopLeftAlignment();
            content.hasChanged = false;
        }
    }

    private void EnsureTopLeftAlignment()
    {
        verticalLayout.childAlignment = TextAnchor.UpperLeft;

        content.anchoredPosition = new Vector2(content.anchoredPosition.x, 0);

        content.pivot = new Vector2(0, 1);
        content.anchorMin = new Vector2(0, 1);
        content.anchorMax = new Vector2(0, 1);
    }

    [ContextMenu("Call My Method")]
    public void ScrollToTop()
    {
        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 1f;
    }

    public void ScrollToNewItem()
    {
        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0f;
    }
}