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
    private RectTransform centerRectTransform;

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
        
        // Get the RectTransform of this GameObject
        centerRectTransform = GetComponent<RectTransform>();
        if (centerRectTransform == null)
        {
            Debug.LogError("This GameObject doesn't have a RectTransform component!");
            enabled = false;
            return;
        }
    }

    void Update()
    {
        if (objectToMove == null || parentCanvas == null || centerRectTransform == null)
            return;

        // Get the current center position of this GameObject in canvas space
        // This will be the anchor point for the eye movement
        Vector2 currentCenter = GetCenterPositionInCanvasSpace();
        
        // Get cursor position in canvas space
        Vector2 localCursor;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            objectToMove.parent as RectTransform,
            Input.mousePosition,
            parentCanvas.worldCamera,
            out localCursor
        );

        // Calculate direction from this GameObject's center to cursor
        Vector2 direction = localCursor - currentCenter;
        
        // If cursor is outside boundary, clamp it
        if (direction.magnitude > boundaryRadius)
        {
            direction = direction.normalized * boundaryRadius;
        }

        // Calculate target position relative to this GameObject's center
        Vector2 targetPosition = currentCenter + direction;
        
        // Smoothly move towards the target position
        objectToMove.anchoredPosition = Vector2.Lerp(
            objectToMove.anchoredPosition,
            targetPosition,
            moveSpeed * Time.deltaTime
        );
    }

    // Get the center position of this GameObject in canvas space
    private Vector2 GetCenterPositionInCanvasSpace()
    {
        // Convert this GameObject's position to canvas space
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(
            parentCanvas.worldCamera, 
            centerRectTransform.position
        );
        
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            objectToMove.parent as RectTransform,
            screenPoint,
            parentCanvas.worldCamera,
            out localPoint
        );
        
        return localPoint;
    }

    // Draw the boundary in the editor
    private void OnDrawGizmosSelected()
    {
        if (objectToMove == null || !Application.isPlaying) return;
        
        // Get current center position during gameplay
        Vector2 currentCenter = GetCenterPositionInCanvasSpace();
        Vector3 worldCenter = objectToMove.parent.TransformPoint(currentCenter);
        
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(worldCenter, boundaryRadius);
    }
}