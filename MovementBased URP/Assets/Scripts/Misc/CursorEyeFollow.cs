using UnityEngine;
using UnityEngine.UI;

public class CursorEyeFollow : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private RectTransform objectToMove;  
    [SerializeField] private float boundaryRadius = 50f;  
    [SerializeField] private float moveSpeed = 15f;     
    
    private Vector2 centerPosition;
    private Canvas parentCanvas;
    private RectTransform centerRectTransform;

    void Start()
    {
        // Get the parent canvas
        parentCanvas = GetComponentInParent<Canvas>();
        if (parentCanvas == null)
        {
            enabled = false;
            return;
        }
        
        centerRectTransform = GetComponent<RectTransform>();
        if (centerRectTransform == null)
        {
            enabled = false;
            return;
        }
    }

    void Update()
    {
        if (objectToMove == null || parentCanvas == null || centerRectTransform == null)
            return;
        Vector2 currentCenter = GetCenterPositionInCanvasSpace();
        
        Vector2 localCursor;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            objectToMove.parent as RectTransform,
            Input.mousePosition,
            parentCanvas.worldCamera,
            out localCursor
        );

        Vector2 direction = localCursor - currentCenter;
        
        if (direction.magnitude > boundaryRadius)
        {
            direction = direction.normalized * boundaryRadius;
        }

        Vector2 targetPosition = currentCenter + direction;
        
        objectToMove.anchoredPosition = Vector2.Lerp(
            objectToMove.anchoredPosition,
            targetPosition,
            moveSpeed * Time.deltaTime
        );
    }

    private Vector2 GetCenterPositionInCanvasSpace()
    {
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
}