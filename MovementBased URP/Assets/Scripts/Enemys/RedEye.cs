using UnityEngine;
using System.Collections;

public class RedEyeDetector : MonoBehaviour
{
    [Header("Detection Settings")]
    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private string targetTag = "RedEye";
    
    [Header("Fill Settings")]
    [SerializeField] private float fillDuration = 1.5f;
    [SerializeField] private float maxFillValue = 5f;
    
    [Header("Debug")]
    [SerializeField] private float currentFillValue = 0f;
    [SerializeField] private bool isRedEyeVisible = false;
    
    private Camera mainCamera;
    private GameObject redEyeObject;
    private Coroutine fillCoroutine;
    
    // Events for other scripts to listen to
    public System.Action<float> OnFillValueChanged;
    public System.Action OnFillComplete;
    public System.Action OnRedEyeDetected;
    public System.Action OnRedEyeLost;
    
    void Start()
    {
        mainCamera = Camera.main;
        
        if (mainCamera == null)
        {
            Debug.LogError("No main camera found in scene!");
        }
        
        // Find the RedEye object by tag
        redEyeObject = GameObject.FindGameObjectWithTag(targetTag);
        
        if (redEyeObject == null)
        {
            Debug.LogWarning($"No object with tag '{targetTag}' found in scene.");
        }
    }
    
    void Update()
    {
        if (redEyeObject == null || mainCamera == null) return;
        
        CheckRedEyeVisibility();
    }
    
    private void CheckRedEyeVisibility()
    {
        bool wasVisible = isRedEyeVisible;
        isRedEyeVisible = IsObjectOnScreen(redEyeObject);
        
        // Object just became visible
        if (!wasVisible && isRedEyeVisible)
        {
            StartFillCoroutine();
            OnRedEyeDetected?.Invoke();
        }
        // Object just became invisible
        else if (wasVisible && !isRedEyeVisible)
        {
            StopFillCoroutine();
            OnRedEyeLost?.Invoke();
        }
    }
    
    private bool IsObjectOnScreen(GameObject targetObject)
    {
        // Check if object is within range
        float distance = Vector3.Distance(mainCamera.transform.position, targetObject.transform.position);
        if (distance > detectionRange) return false;
        
        // Check if object is within camera view frustum
        Vector3 screenPoint = mainCamera.WorldToViewportPoint(targetObject.transform.position);
        
        // Object is on screen if it's within the viewport (0-1 range) and in front of camera
        bool onScreen = screenPoint.z > 0 && 
                       screenPoint.x >= 0 && screenPoint.x <= 1 && 
                       screenPoint.y >= 0 && screenPoint.y <= 1;
        
        return onScreen;
    }
    
    private void StartFillCoroutine()
    {
        // Stop any existing coroutine
        if (fillCoroutine != null)
        {
            StopCoroutine(fillCoroutine);
        }
        
        // Reset fill value
        currentFillValue = 0f;
        OnFillValueChanged?.Invoke(currentFillValue);
        
        // Start new fill coroutine
        fillCoroutine = StartCoroutine(FillCoroutine());
    }
    
    private void StopFillCoroutine()
    {
        if (fillCoroutine != null)
        {
            StopCoroutine(fillCoroutine);
            fillCoroutine = null;
        }
        
        // Reset fill value when object leaves screen
        currentFillValue = 0f;
        OnFillValueChanged?.Invoke(currentFillValue);
    }
    
    private IEnumerator FillCoroutine()
    {
        float timer = 0f;
        float targetValue = maxFillValue;
        
        while (timer < fillDuration && isRedEyeVisible)
        {
            timer += Time.deltaTime;
            float progress = Mathf.Clamp01(timer / fillDuration);
            
            // Calculate current fill value based on progress - smooth interpolation
            float newFillValue = progress * targetValue;
            
            // Update continuously for smooth changes
            if (newFillValue != currentFillValue)
            {
                currentFillValue = newFillValue;
                OnFillValueChanged?.Invoke(currentFillValue);
            }
            
            yield return null;
        }
        
        // Ensure we reach the target value
        if (isRedEyeVisible && currentFillValue < targetValue)
        {
            currentFillValue = targetValue;
            OnFillValueChanged?.Invoke(currentFillValue);
            OnFillComplete?.Invoke();
        }
        
        fillCoroutine = null;
    }
    
    // Public methods to access current state
    public float GetCurrentFillValue() => currentFillValue;
    public bool IsRedEyeVisible() => isRedEyeVisible;
    public bool IsFilling() => fillCoroutine != null;
    
    // Optional: Get rounded integer value if needed
    public int GetCurrentFillValueRounded() => Mathf.RoundToInt(currentFillValue);
    public int GetCurrentFillValueFloored() => Mathf.FloorToInt(currentFillValue);
    public int GetCurrentFillValueCeiled() => Mathf.CeilToInt(currentFillValue);
    
    // Editor helper to visualize detection range
    void OnDrawGizmosSelected()
    {
        if (mainCamera != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(mainCamera.transform.position, detectionRange);
        }
    }
}