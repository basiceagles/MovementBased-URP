using UnityEngine;
using UnityEngine.UI;

public class CameraController : MonoBehaviour
{
    [Header("Camera Settings")]
    private const string HighscoreKey = "Sens";
    [SerializeField] private float sens;
    public Slider slider;
    [SerializeField] private float maxYAngle;

    [Header("Tilt Settings")]
    [SerializeField] private float tiltAmount = 5f; // Degrees to tilt
    [SerializeField] private float tiltSpeedThreshold = 1f; // Minimum speed to trigger tilt
    [SerializeField] private float tiltSmoothTime = 0.1f; // Smoothing time for the tilt effect

    private Quaternion targetRotation;
    private float currentTilt;
    private float tiltVelocity;

    private float rotationX;

    public void ChangeSens()
    {
        sens = slider.value;
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        rotationX = transform.localEulerAngles.x; // Initialize rotationX with current local rotation
    }

    private void Update()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        // Rotate around the Y-axis (left/right)
        transform.parent.Rotate(Vector3.up * mouseX * sens);

        // Rotate around the X-axis (up/down) and clamp the rotation
        rotationX = Mathf.Clamp(rotationX - mouseY * sens, -maxYAngle, maxYAngle);
        
        // Set the local rotation based on the clamped X rotation and Y rotation from the parent
        transform.localRotation = Quaternion.Euler(rotationX, 0f, 0f);

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
        transform.localRotation *= targetRotation; // Combine the tilt with current local rotation
    }

    private float GetCameraRotationSpeed()
    {
        // Calculate the camera's rotation speed based on mouse input or other input methods
        return Mathf.Abs(Input.GetAxis("Mouse X")) + Mathf.Abs(Input.GetAxis("Mouse Y"));
    }
}
