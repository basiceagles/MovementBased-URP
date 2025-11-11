using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Camera Settings")]
    [SerializeField] private float sens = 1f;
    [SerializeField] private float maxYAngle = 80f;

    [Header("Tilt Settings")]
    [SerializeField] private float tiltAmount = 5f; // Degrees to tilt
    [SerializeField] private float tiltSpeedThreshold = 1f; // Minimum speed to trigger tilt
    [SerializeField] private float tiltSmoothTime = 0.1f; // Smoothing time for the tilt effect

    private float currentTilt;
    private float tiltVelocity;
    private float rotationX;

    public void SetSensitivity(float value)
    {
        sens = value;
    }

    private void Start()
    {
        // Reset to safe values when component is enabled (after scene reload)
        rotationX = 0f;
        currentTilt = 0f;
        tiltVelocity = 0f;
        
        if (transform != null)
        {
            transform.localRotation = Quaternion.identity;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        // Safe initialization of rotationX
        Vector3 localEuler = transform.localEulerAngles;
        rotationX = NormalizeAngle(localEuler.x);
        
        // Clamp the initial rotation to prevent invalid values
        rotationX = Mathf.Clamp(rotationX, -maxYAngle, maxYAngle);
    }

    private void Update()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        // Rotate around the Y-axis (left/right)
        if (transform.parent != null)
        {
            transform.parent.Rotate(Vector3.up * mouseX * sens);
        }

        // Rotate around the X-axis (up/down) and clamp the rotation
        // Check for valid values before applying
        if (!float.IsNaN(mouseY) && !float.IsInfinity(mouseY))
        {
            rotationX = Mathf.Clamp(rotationX - mouseY * sens, -maxYAngle, maxYAngle);
        }

        // Get the current rotation speed of the camera (X-axis only)
        float rotationSpeed = GetCameraRotationSpeed();

        // Determine tilt direction based on rotation speed and direction
        if (rotationSpeed > tiltSpeedThreshold && Mathf.Abs(mouseX) > 0.01f)
        {
            // Tilt left if rotating left, tilt right if rotating right
            float tiltDirection = mouseX > 0 ? -tiltAmount : tiltAmount;
            currentTilt = Mathf.SmoothDamp(currentTilt, tiltDirection, ref tiltVelocity, tiltSmoothTime);
        }
        else
        {
            // Reset tilt when not rotating fast enough
            currentTilt = Mathf.SmoothDamp(currentTilt, 0, ref tiltVelocity, tiltSmoothTime);
        }

        // Apply both vertical rotation and tilt in a single operation
        // This prevents the multiplication that can cause NaN values
        Vector3 finalRotation = new Vector3(rotationX, 0f, currentTilt);
        
        // Validate before applying rotation
        if (IsValidRotation(finalRotation))
        {
            transform.localRotation = Quaternion.Euler(finalRotation);
        }
        else
        {
            // Emergency reset if rotation becomes invalid
            ResetCameraRotation();
        }
    }

    private float GetCameraRotationSpeed()
    {
        // Only consider horizontal (X) movement for tilt calculation
        return Mathf.Abs(Input.GetAxis("Mouse X"));
    }

    private float NormalizeAngle(float angle)
    {
        // Normalize angle to be between -180 and 180
        while (angle > 180f) angle -= 360f;
        while (angle < -180f) angle += 360f;
        return angle;
    }

    private bool IsValidRotation(Vector3 eulerAngles)
    {
        return !float.IsNaN(eulerAngles.x) && !float.IsNaN(eulerAngles.y) && !float.IsNaN(eulerAngles.z) &&
               !float.IsInfinity(eulerAngles.x) && !float.IsInfinity(eulerAngles.y) && !float.IsInfinity(eulerAngles.z) &&
               Mathf.Abs(eulerAngles.x) <= 90f && Mathf.Abs(eulerAngles.z) <= 90f; // Additional safety checks
    }

    private void ResetCameraRotation()
    {
        rotationX = 0f;
        currentTilt = 0f;
        tiltVelocity = 0f;
        transform.localRotation = Quaternion.identity;
        Debug.LogWarning("Camera rotation reset due to invalid values");
    }
}