using UnityEngine;

public class CameraTilt : MonoBehaviour
{
    [Header("Tilt Settings")]
    [SerializeField] private float tiltAmount = 5f; 
    [SerializeField] private float tiltSpeedThreshold = 1f; 
    [SerializeField] private float tiltSmoothTime = 0.1f; 

    private Quaternion targetRotation;
    private float currentTilt;
    private float tiltVelocity;

    private void Start()
    {
        targetRotation = transform.localRotation; 
    }

    private void Update()
    {
        float rotationSpeed = GetCameraRotationSpeed();

        if (rotationSpeed > tiltSpeedThreshold)
        {
            float tiltDirection = Input.GetAxis("Mouse X") > 0 ? -tiltAmount : tiltAmount;
            currentTilt = Mathf.SmoothDamp(currentTilt, tiltDirection, ref tiltVelocity, tiltSmoothTime);
        }
        else
        {
            currentTilt = Mathf.SmoothDamp(currentTilt, 0, ref tiltVelocity, tiltSmoothTime);
        }

        targetRotation = Quaternion.Euler(0, 0, currentTilt);
        transform.localRotation = targetRotation * Quaternion.Euler(0, transform.localEulerAngles.y, 0);
    }

    private float GetCameraRotationSpeed()
    {
        return Mathf.Abs(Input.GetAxis("Mouse X")) + Mathf.Abs(Input.GetAxis("Mouse Y"));
    }
}