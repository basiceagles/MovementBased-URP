using UnityEngine;
using System.Collections;

public class CreditsScroller : MonoBehaviour
{
    [Header("Scroll Settings")]
    public float scrollSpeed = 100f; 
    
    [Header("Position Settings")]
    public float startYPos = -412f;
    
    private RectTransform rectTransform;
    private bool isScrolling = false;
    private Coroutine scrollCoroutine;
    
    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        
        ResetPosition();
    }
    
    void Start()
    {
    }
    
    public void StartScroll()
    {
        if (isScrolling) return;
        
        isScrolling = true;
        
        if (rectTransform == null)
            rectTransform = GetComponent<RectTransform>();
        
        if (scrollCoroutine != null)
            StopCoroutine(scrollCoroutine);
            
        scrollCoroutine = StartCoroutine(ScrollRoutine());
    }
    
    public void StopScroll()
    {
        if (!isScrolling) return;
        
        isScrolling = false;
        
        if (scrollCoroutine != null)
        {
            StopCoroutine(scrollCoroutine);
            scrollCoroutine = null;
        }
        
        ResetPosition();
    }
    
    public void ResetPosition()
    {
        if (rectTransform != null)
        {
            Vector2 currentPos = rectTransform.anchoredPosition;
            currentPos.y = startYPos;
            rectTransform.anchoredPosition = currentPos;
        }
    }
    
    private IEnumerator ScrollRoutine()
    {
        yield return null;
        
        while (isScrolling && rectTransform != null)
        {
            Vector2 currentPos = rectTransform.anchoredPosition;
            
            currentPos.y += scrollSpeed * Time.deltaTime;
            
            rectTransform.anchoredPosition = currentPos;
            
            yield return null; 
        }
    }
    
    public bool IsScrolling
    {
        get { return isScrolling; }
    }
    
    void OnDisable()
    {
        if (isScrolling)
        {
            StopScroll();
        }
    }
}