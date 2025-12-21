using UnityEngine;
using UnityEngine.UI;

public class CursorEyeFollow : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private RectTransform objectToMove;  // Changed to RectTransform for UI elements
    [SerializeField] private float boundaryRadius = 50f;  // In pixels for UI
    [SerializeField] private float moveSpeed = 15f;      // Adjust for responsiveness
    
    private Vector2 centerPosition;
    private Canvas parentCanvas;

    void Start()
    {
        // Get the parent canvas
        parentCanvas = GetComponentInParent<Canvas>();
        if (parentCanvas == null)
        {
            Debug.LogError("No Canvas found in parents!");
            enabled = false;
            return;
        }
        
        // Set initial center position
        centerPosition = Vector2.zero;  // Center of the parent RectTransform
    }

    void Update()
    {
        if (objectToMove == null || parentCanvas == null)
            return;

        // Get cursor position in canvas space
        Vector2 localCursor;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            objectToMove.parent as RectTransform,
            Input.mousePosition,
            parentCanvas.worldCamera,
            out localCursor
        );

        // Calculate direction from center to cursor
        Vector2 direction = localCursor - centerPosition;
        
        // If cursor is outside boundary, clamp it
        if (direction.magnitude > boundaryRadius)
        {
            direction = direction.normalized * boundaryRadius;
        }

        // Smoothly move towards the target position
        objectToMove.anchoredPosition = Vector2.Lerp(
            objectToMove.anchoredPosition,
            direction,
            moveSpeed * Time.deltaTime
        );
    }

    // Draw the boundary in the editor
    private void OnDrawGizmosSelected()
    {
        if (objectToMove == null) return;
        
        Vector3 center = objectToMove.parent != null ? 
            objectToMove.parent.TransformPoint(centerPosition) : 
            centerPosition;
            
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(center, boundaryRadius / 100f); // Scale for better visibility
    }
}