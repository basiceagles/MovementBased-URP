using UnityEngine;

public class WallRunning : MonoBehaviour
{
    [Header("Wall Run Settings")]
    [SerializeField] private float wallRunSpeed;
    [SerializeField] private float wallStickForce;
    
    [Header("Wall Jump Settings")]
    [SerializeField] private float wallJumpHeight;
    [SerializeField] private float wallJumpForce;
    
    [Header("Camera Tilt")]
    [SerializeField] private float wallTiltAngle;
    [SerializeField] private float tiltSpeed;
    
    [Header("References")]
    [SerializeField] private Transform cam;

    private PlayerController pc;
    private bool isWallRunning;
    private Vector3 wallNormal;
    private float currentTilt;

    private void Start()
    {
        pc = GetComponent<PlayerController>();
    }

    private void Update()
    {
        if (isWallRunning)
        {
            HandleWallRun();
            ApplyWallTilt();
            
            if (Input.GetKeyDown(KeyCode.Space))
            {
                WallJump();
            }
        }
        else
        {
            ResetTilt();
        }
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (pc.grounded)
        {
            if (isWallRunning)
            {
                StopWallRun();
            }
            return;
        }
        
        if (hit.gameObject.CompareTag("Wall"))
        {
            float dotUp = Vector3.Dot(hit.normal, Vector3.up);
            if (Mathf.Abs(dotUp) < 0.1f)
            {
                wallNormal = hit.normal;
                if (!isWallRunning)
                {
                    StartWallRun();
                }
            }
        }
    }

    private void StartWallRun()
    {
        isWallRunning = true;
        pc.SetWallRunning(true);
    }

    private void StopWallRun()
    {
        isWallRunning = false;
        pc.SetWallRunning(false);
    }

    private void HandleWallRun()
    {
        float vertical = Input.GetAxisRaw("Vertical");
        float horizontal = Input.GetAxisRaw("Horizontal");
        
        Vector3 inputDir = (cam.forward * vertical + cam.right * horizontal);
        inputDir.y = 0;
        inputDir.Normalize();
        
        Vector3 wallForward = Vector3.Cross(wallNormal, Vector3.up);
        Vector3 movement = -wallNormal * wallStickForce; 
        
        if (inputDir.magnitude > 0.1f)
        {
            float forwardInput = Vector3.Dot(inputDir, wallForward);
            movement += wallForward * forwardInput * wallRunSpeed;
        }
        
        pc.horizontalVelocity = new Vector3(movement.x, 0f, movement.z);
        pc.verticalVelocity = 0f; 
    }

    private void WallJump()
    {
        Vector3 jumpDir = cam.forward;
        jumpDir.y = 0;
        jumpDir.Normalize();
        
        pc.horizontalVelocity = jumpDir * wallJumpForce;
        pc.verticalVelocity = wallJumpHeight;
        
        pc.SetWallJumped();
        StopWallRun();
    }
    
    private void ApplyWallTilt()
    {
        float wallSide = Vector3.Dot(transform.right, wallNormal);
        float targetTilt = wallSide > 0 ? -wallTiltAngle : wallTiltAngle;
        
        currentTilt = Mathf.Lerp(currentTilt, targetTilt, tiltSpeed * Time.deltaTime);
        
        Vector3 rotation = transform.eulerAngles;
        rotation.z = currentTilt;
        transform.eulerAngles = rotation;
    }
    
    private void ResetTilt()
    {
        currentTilt = Mathf.Lerp(currentTilt, 0f, tiltSpeed * Time.deltaTime);
        
        Vector3 rotation = transform.eulerAngles;
        rotation.z = currentTilt;
        transform.eulerAngles = rotation;
    }
}