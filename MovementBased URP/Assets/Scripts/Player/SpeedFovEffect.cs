using UnityEngine;

public class SpeedFOVEffect : MonoBehaviour
{
    [Header("FOV Settings")]
    public float normalFOV = 80f;
    public float minFOV = 40f;
    public float speedThreshold = 20f;
    public float shrinkMultiplier = 0.5f;
    
    [Header("Smoothing")]
    public float fovSmoothTime = 0.2f;
    
    [Header("Player Speed")]
    public float currentSpeed; // Empty float as requested
    
    private Camera mainCamera;
    private float targetFOV;
    private float fovVelocity;
    private PlayerController playerController;

    void Start()
    {
        playerController = FindObjectOfType<PlayerController>();
        mainCamera = GetComponent<Camera>();
        
        // Initialize FOV
        if (mainCamera != null)
        {
            mainCamera.fieldOfView = normalFOV;
            targetFOV = normalFOV;
        }
    }

    void Update()
    {
        currentSpeed = playerController.currentSpeed;
        UpdateFOVBasedOnSpeed();
    }

    private void UpdateFOVBasedOnSpeed()
    {
        if (mainCamera == null) return;

        // Calculate target FOV based on speed
        if (currentSpeed <= speedThreshold)
        {
            // Normal FOV when speed is 0-20
            targetFOV = normalFOV;
        }
        else
        {
            // Calculate how much above threshold we are
            float excessSpeed = currentSpeed - speedThreshold;
            
            // Calculate FOV reduction using shrink multiplier
            float fovReduction = excessSpeed * shrinkMultiplier;
            
            // Apply reduction to normal FOV and clamp to minimum
            targetFOV = normalFOV - fovReduction;
            targetFOV = Mathf.Max(targetFOV, minFOV);
        }

        // Smoothly transition to target FOV
        mainCamera.fieldOfView = Mathf.SmoothDamp(mainCamera.fieldOfView, targetFOV, ref fovVelocity, fovSmoothTime);
    }

    // Public method to set speed from another script
    public void SetSpeed(float speed)
    {
        currentSpeed = speed;
    }

    // Public method to get current FOV for debugging
    public float GetCurrentFOV()
    {
        return mainCamera != null ? mainCamera.fieldOfView : 0f;
    }

    // Reset FOV to normal immediately
    public void ResetFOV()
    {
        mainCamera.fieldOfView = normalFOV;
        targetFOV = normalFOV;
        currentSpeed = 0f;
    }
}