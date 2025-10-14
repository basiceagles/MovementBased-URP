using UnityEngine;

public class CameraTilt : MonoBehaviour
{
    [Header("Tilt Settings")]
    [SerializeField] private float tiltAmount = 5f; // Degrees to tilt
    [SerializeField] private float tiltSpeedThreshold = 1f; // Minimum speed to trigger tilt
    [SerializeField] private float tiltSmoothTime = 0.1f; // Smoothing time for the tilt effect

    private Quaternion targetRotation;
    private float currentTilt;
    private float tiltVelocity;

    private void Start()
    {
        targetRotation = transform.localRotation; // Initialize the target rotation
    }

    private void Update()
    {
        // Get the current rotation speed of the camera
        float rotationSpeed = GetCameraRotationSpeed();

        // Determine tilt direction based on rotation speed and direction
        if (rotationSpeed > tiltSpeedThreshold)
        {
            // Tilt left if rotating left, tilt right if rotating right
            float tiltDirection = Input.GetAxis("Mouse X") > 0 ? -tiltAmount : tiltAmount;
            currentTilt = Mathf.SmoothDamp(currentTilt, tiltDirection, ref tiltVelocity, tiltSmoothTime);
        }
        else
        {
            // Reset tilt when not rotating fast enough
            currentTilt = Mathf.SmoothDamp(currentTilt, 0, ref tiltVelocity, tiltSmoothTime);
        }

        // Apply the tilt to the camera's local rotation
        targetRotation = Quaternion.Euler(0, 0, currentTilt);
        transform.localRotation = targetRotation * Quaternion.Euler(0, transform.localEulerAngles.y, 0);
    }

    private float GetCameraRotationSpeed()
    {
        // Calculate the camera's rotation speed based on mouse input or other input methods
        return Mathf.Abs(Input.GetAxis("Mouse X")) + Mathf.Abs(Input.GetAxis("Mouse Y"));
    }
}