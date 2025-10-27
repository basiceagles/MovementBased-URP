using UnityEngine;
using System.Collections;

public class CreditsScroller : MonoBehaviour
{
    [Header("Scroll Settings")]
    public float scrollSpeed = 100f; // Units per second
    
    [Header("Position Settings")]
    public float startYPos = -412f;
    
    private RectTransform rectTransform;
    private bool isScrolling = false;
    private Coroutine scrollCoroutine;
    
    void Awake()
    {
        // Get the RectTransform component - use Awake instead of Start for earlier initialization
        rectTransform = GetComponent<RectTransform>();
        
        // Set initial position
        ResetPosition();
    }
    
    void Start()
    {
        // Additional Start logic can go here if needed
    }
    
    // Call this method to start scrolling
    public void StartScroll()
    {
        if (isScrolling) return;
        
        isScrolling = true;
        
        // Ensure we have the RectTransform reference
        if (rectTransform == null)
            rectTransform = GetComponent<RectTransform>();
        
        // Stop any existing coroutine
        if (scrollCoroutine != null)
            StopCoroutine(scrollCoroutine);
            
        // Start scrolling coroutine
        scrollCoroutine = StartCoroutine(ScrollRoutine());
    }
    
    // Call this method to stop scrolling and reset position
    public void StopScroll()
    {
        if (!isScrolling) return;
        
        isScrolling = false;
        
        // Stop the scrolling coroutine
        if (scrollCoroutine != null)
        {
            StopCoroutine(scrollCoroutine);
            scrollCoroutine = null;
        }
        
        // Reset to start position
        ResetPosition();
    }
    
    // Reset the Y position to start position
    public void ResetPosition()
    {
        if (rectTransform != null)
        {
            Vector2 currentPos = rectTransform.anchoredPosition;
            currentPos.y = startYPos;
            rectTransform.anchoredPosition = currentPos;
        }
    }
    
    // Coroutine that handles smooth scrolling
    private IEnumerator ScrollRoutine()
    {
        // Wait one frame to ensure all references are set
        yield return null;
        
        while (isScrolling && rectTransform != null)
        {
            // Get current position
            Vector2 currentPos = rectTransform.anchoredPosition;
            
            // FIX: Scroll DOWN by making Y position more negative
            // To scroll down (content moves upward), we need to DECREASE the Y value
            currentPos.y += scrollSpeed * Time.deltaTime;
            
            // Apply the new position
            rectTransform.anchoredPosition = currentPos;
            
            yield return null; // Wait for next frame
        }
    }
    
    // Optional: Public property to check if currently scrolling
    public bool IsScrolling
    {
        get { return isScrolling; }
    }
    
    // Safety check when the component is disabled
    void OnDisable()
    {
        if (isScrolling)
        {
            StopScroll();
        }
    }
}