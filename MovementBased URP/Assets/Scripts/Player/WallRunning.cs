using UnityEngine;

public class WallRunning : MonoBehaviour
{
    [Header("Wall Run Settings")]
    [SerializeField] private float wallStickForce;
    
    [Header("Wall Detection")]
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private float sphereRadius = 0.6f;
    
    [Header("Wall Jump Settings")]
    [SerializeField] private float wallJumpHeight;
    [SerializeField] private float wallJumpForce;
    [SerializeField] private float wallJumpCooldown = 0.3f;
    
    [Header("Camera Tilt")]
    [SerializeField] private float wallTiltAngle;
    [SerializeField] private float tiltSpeed;
    
    [Header("References")]
    [SerializeField] private Transform cam;
    private KeyCode jumpKey = KeyCode.Space;

    private PlayerController pc;
    private bool isWallRunning;
    private Vector3 wallNormal;
    private bool isActivelyWallRunning;
    public bool IsWallRunning => isWallRunning;
    public bool IsActivelyWallRunning => isActivelyWallRunning;
    private float currentTilt;
    private float wallJumpTimer;
    private Collider lastWall;
    private Collider currentWall;

    private void Start()
    {
        pc = GetComponent<PlayerController>();
    }

    private void Update()
    {
        if (wallJumpTimer > 0)
        {
            wallJumpTimer -= Time.deltaTime;
        }
        
        if (isWallRunning)
        {
            HandleWallRun();
            ApplyWallTilt();
            
            if (Input.GetKeyDown(jumpKey))
            {
                WallJump();
            }
        }
        else
        {
            isActivelyWallRunning = false;
            ResetTilt();
        }
    }

    private void FixedUpdate()
    {
        CheckForWalls();
    }
    
    private void CheckForWalls()
    {
        if (wallJumpTimer > 0)
        {
            if (isWallRunning)
            {
                StopWallRun();
            }
            return;
        }
        
        if (pc.grounded)
        {
            if (isWallRunning)
            {
                StopWallRun();
            }
            
            lastWall = null;
            
            currentTilt = 0f;
            Vector3 rotation = transform.localEulerAngles;
            rotation.x = 0f;
            rotation.z = 0f;
            transform.localEulerAngles = rotation;
            
            return;
        }
        
        if (isWallRunning && (!Input.GetKey(KeyCode.LeftShift) || pc.currentSpeed <= 0.1f))
        {
            StopWallRun();
            return;
        }
        
        Vector3 origin = transform.position;
        Collider[] wallColliders = Physics.OverlapSphere(origin, sphereRadius, wallLayer);
        bool wallFound = false;
        
        foreach (Collider wallCollider in wallColliders)
        {
            Vector3 closestPoint = wallCollider.ClosestPoint(origin);
            Vector3 directionToWall = (closestPoint - origin).normalized;
            if (Mathf.Abs(Vector3.Dot(directionToWall, Vector3.up)) < 0.3f)
            {
                if (wallCollider == lastWall)
                {
                    continue;
                }
                
                wallNormal = -directionToWall;
                currentWall = wallCollider;
                wallFound = true;
                
                if (!isWallRunning && CanStartWallRun())
                {
                    StartWallRun();
                }
                
                if (isWallRunning && currentWall != lastWall)
                {
                    lastWall = null;
                }
                
                break;
            }
        }
        
        if (!wallFound && isWallRunning)
        {
            StopWallRun();
        }
    }
    
    private bool CanStartWallRun()
    {
        bool isSprinting = Input.GetKey(KeyCode.LeftShift);
        bool isMoving = pc.currentSpeed > 0.1f;
        bool notGrounded = !pc.grounded;
        
        return isSprinting && isMoving && notGrounded;
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
        
        if (inputDir.magnitude <= 0.1f)
        {
            isActivelyWallRunning = false;
            StopWallRun();
            return;
        }
        
        Vector3 wallForward = Vector3.Cross(wallNormal, Vector3.up);
        Vector3 movement = -wallNormal * wallStickForce; 
        
        if (inputDir.magnitude > 0.1f)
        {
            float forwardInput = Vector3.Dot(inputDir, wallForward);
            if (Input.GetKey(KeyCode.LeftShift))
            {
                movement += wallForward * forwardInput * pc.runSpeed;
                isActivelyWallRunning = true; 
            }
            else
            {
                isActivelyWallRunning = false;
                StopWallRun();
                return;
            }
        }
        
        pc.horizontalVelocity = new Vector3(movement.x, 0f, movement.z);
        pc.verticalVelocity = 0f; 
    }

    private void WallJump()
    {
        Vector3 lookDir = cam.forward;
        lookDir.y = 0;
        lookDir.Normalize();
        
        pc.horizontalVelocity = lookDir * wallJumpForce;
        pc.verticalVelocity = wallJumpHeight;
        
        wallJumpTimer = wallJumpCooldown;
        lastWall = currentWall;
        
        pc.SetWallJumped();
        StopWallRun();
    }
    
    private void ApplyWallTilt()
    {
        float wallSide = Vector3.Dot(transform.right, wallNormal);
        float targetTilt = wallSide > 0 ? -wallTiltAngle : wallTiltAngle;
        
        currentTilt = Mathf.Lerp(currentTilt, targetTilt, tiltSpeed * Time.deltaTime);
        
        Vector3 rotation = transform.localEulerAngles;
        rotation.x = 0f;  
        rotation.z = currentTilt;  
        transform.localEulerAngles = rotation;
    }
    
    private void ResetTilt()
    {
        if (Mathf.Abs(currentTilt) < 0.01f)
        {
            currentTilt = 0f;
        }
        else
        {
            currentTilt = Mathf.Lerp(currentTilt, 0f, tiltSpeed * Time.deltaTime);
        }
        
        Vector3 rotation = transform.localEulerAngles;
        rotation.x = 0f;  
        rotation.z = currentTilt;  
        transform.localEulerAngles = rotation;
    }
    
    private void OnDrawGizmos()
    {
        Vector3 origin = transform.position;
        Collider[] wallColliders = Physics.OverlapSphere(origin, sphereRadius, wallLayer);
        bool wallDetected = wallColliders.Length > 0;
        Gizmos.color = wallDetected ? Color.red : Color.gray;
        Gizmos.DrawWireSphere(origin, sphereRadius);
    }

    public void SetWallRunningKey(KeyCode newKey)
    {
        jumpKey = newKey;
    }
}