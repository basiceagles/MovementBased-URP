using UnityEngine;

public class CameraFollowsCursor : MonoBehaviour
{
    public float sensitivity = 0.001f;
    public float smoothSpeed = 5f;
    public float maxOffset = 0.3f;
    public bool inverse = true; // Toggle for inverse movement
    
    private Vector3 originalPosition;
    private Vector3 targetOffset;
    private float halfScreenWidth;
    private float halfScreenHeight;

    private void Start()
    {
        originalPosition = transform.position;
        halfScreenWidth = Screen.width * 0.5f;
        halfScreenHeight = Screen.height * 0.5f;
    }

    private void Update()
    {
        if (halfScreenWidth == 0f) return;

        Vector3 mousePos = Input.mousePosition;
        float normX = (mousePos.x - halfScreenWidth) / halfScreenWidth;
        float normY = (mousePos.y - halfScreenHeight) / halfScreenHeight;
        
        float dirMultiplier = inverse ? -1f : 1f;
        targetOffset.x = Mathf.Clamp(normX * sensitivity * dirMultiplier, -maxOffset, maxOffset);
        targetOffset.y = Mathf.Clamp(normY * sensitivity * dirMultiplier, -maxOffset, maxOffset);
        targetOffset.z = 0f;
        
        transform.position = Vector3.Lerp(transform.position, originalPosition + targetOffset, Time.deltaTime * smoothSpeed);
    }
}