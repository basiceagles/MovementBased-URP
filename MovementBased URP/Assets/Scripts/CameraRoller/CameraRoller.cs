using UnityEngine;

public class CameraRoller : MonoBehaviour
{
    [Header("Dolly Settings")]
    public Vector3 dollyDirection = Vector3.forward; // Direction to dolly in
    public float dollySpeed = 2f; // Units per second
    
    [Header("Distance Limits")]
    public float maxDollyDistance = 10f;
    public float minDollyDistance = 1f;
    
    [Header("Control")]
    public bool enableDollying = true;
    
    private Vector3 startPosition;
    private float currentDistance = 0f;

    void Start()
    {
        // Store initial position
        startPosition = transform.position;
    }

    void Update()
    {
        if (enableDollying)
        {
            DollyCamera();
        }
    }

    void DollyCamera()
    {
        // Calculate movement for this frame
        float movement = dollySpeed * Time.deltaTime;
        
        // Update current distance
        currentDistance += movement;
        
        // Clamp distance to limits
        currentDistance = Mathf.Clamp(currentDistance, minDollyDistance, maxDollyDistance);
        
        // Calculate new position
        Vector3 newPosition = startPosition + dollyDirection.normalized * currentDistance;
        
        // Apply the movement
        transform.position = newPosition;
    }

    // Public methods to control dollying
    public void StartDollying()
    {
        enableDollying = true;
    }

    public void StopDollying()
    {
        enableDollying = false;
    }

    public void SetDollySpeed(float newSpeed)
    {
        dollySpeed = newSpeed;
    }

    public void SetDollyDirection(Vector3 newDirection)
    {
        dollyDirection = newDirection.normalized;
    }

    public void ResetDolly()
    {
        currentDistance = 0f;
        transform.position = startPosition;
    }

    public void SetDollyDistance(float distance)
    {
        currentDistance = Mathf.Clamp(distance, minDollyDistance, maxDollyDistance);
        transform.position = startPosition + dollyDirection.normalized * currentDistance;
    }
}