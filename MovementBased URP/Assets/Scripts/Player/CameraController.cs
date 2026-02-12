using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Camera Settings")]
    [SerializeField] private float sens = 1f;
    [SerializeField] private float maxYAngle = 80f;

    [Header("Start Rotation")]
    [SerializeField] private Vector3 cameraStartRotation = Vector3.zero; 

    [Header("Tilt Settings")]
    public float tiltAmount = 5f; 
    [SerializeField] private float tiltSpeedThreshold = 1f; 
    [SerializeField] private float tiltSmoothTime = 0.1f; 

    private float currentTilt;
    private float tiltVelocity;
    private float rotationX;

    [SerializeField] private bool canMoveCamera = true;

    public void SetSensitivity(float value)
    {
        sens = value;
    }

    private void Start()
    {
        rotationX = 0f;
        currentTilt = 0f;
        tiltVelocity = 0f;

        if (transform != null)
        {
            transform.localRotation = Quaternion.Euler(cameraStartRotation);
        }

        if (canMoveCamera)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        Vector3 localEuler = transform.localEulerAngles;
        rotationX = NormalizeAngle(localEuler.x);

        rotationX = Mathf.Clamp(rotationX, -maxYAngle, maxYAngle);
    }

    private float externalMouseX;
    private float externalMouseY;

    public void AddExternalRotation(float x, float y)
    {
        externalMouseX += x;
        externalMouseY += y;
    }

    private void Update()
    {
        float mouseX = 0f;
        float mouseY = 0f;

        if (canMoveCamera)
        {
            mouseX = Input.GetAxis("Mouse X") + externalMouseX;
            mouseY = Input.GetAxis("Mouse Y") + externalMouseY;
            
            externalMouseX = 0f;
            externalMouseY = 0f;
        }
        if (transform.parent != null)
        {
            transform.parent.Rotate(Vector3.up * mouseX * sens);
        }

        if (!float.IsNaN(mouseY) && !float.IsInfinity(mouseY))
        {
            rotationX = Mathf.Clamp(rotationX - mouseY * sens, -maxYAngle, maxYAngle);
        }

        float rotationSpeed = GetCameraRotationSpeed();

        if (rotationSpeed > tiltSpeedThreshold && Mathf.Abs(mouseX) > 0.01f)
        {
            float tiltDirection = mouseX > 0 ? -tiltAmount : tiltAmount;
            currentTilt = Mathf.SmoothDamp(currentTilt, tiltDirection, ref tiltVelocity, tiltSmoothTime);
        }
        else
        {
            currentTilt = Mathf.SmoothDamp(currentTilt, 0, ref tiltVelocity, tiltSmoothTime);
        }

        Vector3 finalRotation = new Vector3(rotationX, 0f, currentTilt);

        if (IsValidRotation(finalRotation))
        {
            transform.localRotation = Quaternion.Euler(finalRotation);
        }
        else
        {
            ResetCameraRotation();
        }
    }

    public void SetCameraMovementEnabled(bool value)
    {
        canMoveCamera = value;

        if (canMoveCamera)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            currentTilt = Mathf.SmoothDamp(currentTilt, 0, ref tiltVelocity, tiltSmoothTime);
            Vector3 finalRotation = new Vector3(rotationX, 0f, currentTilt);
            transform.localRotation = Quaternion.Euler(finalRotation);

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    private float GetCameraRotationSpeed()
    {
        return canMoveCamera ? Mathf.Abs(Input.GetAxis("Mouse X")) : 0f;
    }

    private float NormalizeAngle(float angle)
    {
        while (angle > 180f) angle -= 360f;
        while (angle < -180f) angle += 360f;
        return angle;
    }

    private bool IsValidRotation(Vector3 eulerAngles)
    {
        return !float.IsNaN(eulerAngles.x) && !float.IsNaN(eulerAngles.y) && !float.IsNaN(eulerAngles.z) &&
               !float.IsInfinity(eulerAngles.x) && !float.IsInfinity(eulerAngles.y) && !float.IsInfinity(eulerAngles.z) &&
               Mathf.Abs(eulerAngles.x) <= 90f && Mathf.Abs(eulerAngles.z) <= 90f; 
    }

    public void SnapToLookAt(Vector3 targetPosition)
    {
        Vector3 direction = (targetPosition - transform.position).normalized;
        
        Vector3 directionYaw = direction;
        directionYaw.y = 0;
        Quaternion targetRotationYaw = Quaternion.LookRotation(directionYaw, Vector3.up);
        
        if (transform.parent != null)
        {
            transform.parent.rotation = targetRotationYaw;
        }

        float targetPitch = -Mathf.Asin(direction.y) * Mathf.Rad2Deg;
        rotationX = Mathf.Clamp(targetPitch, -maxYAngle, maxYAngle);
        
        transform.localRotation = Quaternion.Euler(rotationX, 0f, 0f);
        
        currentTilt = 0f;
        tiltVelocity = 0f;
    }

    public void ResetCameraRotation()
    {
        rotationX = 0f;
        currentTilt = 0f;
        tiltVelocity = 0f;
        transform.localRotation = Quaternion.identity;
    }
}