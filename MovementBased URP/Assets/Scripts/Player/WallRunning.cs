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
    [SerializeField] private JumpFallSounds jumpFallSounds;
    
    [Header("Wall Coverage")]
    [SerializeField] private float minCoveragePercentage = 0.7f;
    [SerializeField] private int coverageGridSize = 10;
    
    private Camera mainCam;
    private KeyCode jumpKey = KeyCode.Space;

    private PlayerController pc;
    private bool isWallRunning;
    private Vector3 wallNormal;
    private Vector3 wallFaceNormal;
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
        if (cam != null) mainCam = cam.GetComponent<Camera>();
        if (mainCam == null) mainCam = Camera.main;
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

    private static readonly Collider[] wallBuffer = new Collider[16];

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
            return;
        }
        
        if (isWallRunning && !pc.CanWallRun)
        {
            StopWallRun();
            return;
        }
        
        Vector3 origin = transform.position;
        float currentRadius = isWallRunning ? sphereRadius * 1.5f : sphereRadius;
        int hitCount = Physics.OverlapSphereNonAlloc(origin, currentRadius, wallBuffer, wallLayer);
        bool wallFound = false;
        
        for (int i = 0; i < hitCount; i++)
        {
            Collider wallCollider = wallBuffer[i];
            if (wallCollider == null) continue;

            Vector3 closestPoint = wallCollider.ClosestPoint(origin);
            Vector3 directionToWall = (closestPoint - origin).normalized;
            if (Mathf.Abs(Vector3.Dot(directionToWall, Vector3.up)) < 0.3f)
            {
                if (wallCollider == lastWall)
                {
                    continue;
                }
                
                wallNormal = -directionToWall;
                
                if (Physics.Raycast(origin - directionToWall * 0.1f, directionToWall, out RaycastHit hit, currentRadius + 0.5f, wallLayer))
                {
                    wallFaceNormal = hit.normal;
                }
                else
                {
                    wallFaceNormal = wallNormal;
                }

                currentWall = wallCollider;
                wallFound = true;
                
                if (!isWallRunning && CanStartWallRun(wallCollider))
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
    
    private bool CanStartWallRun(Collider wall)
    {
        bool hasEnoughSpeed = pc.CanWallRun;
        bool notGrounded = !pc.grounded;
        
        if (!hasEnoughSpeed || !notGrounded) return false;
        
        return IsWallCoveringCamera(wall);
    }

    private bool IsWallCoveringCamera(Collider wall)
    {
        if (mainCam == null) return false;

        int hits = 0;
        int totalRays = coverageGridSize * coverageGridSize;
        float step = 1.0f / coverageGridSize;

        for (int x = 0; x < coverageGridSize; x++)
        {
            for (int y = 0; y < coverageGridSize; y++)
            {
                Ray ray = mainCam.ViewportPointToRay(new Vector3(x * step + step / 2f, y * step + step / 2f, 0));
                
                if (Physics.Raycast(ray, out RaycastHit hit, sphereRadius + 2f, wallLayer))
                {
                    if (hit.collider == wall)
                    {
                        hits++;
                    }
                }
            }
        }

        float coverage = (float)hits / totalRays;
        return coverage >= minCoveragePercentage;
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
        
        Vector3 wallForward = Vector3.Cross(wallFaceNormal, Vector3.up);
        Vector3 movement = -wallNormal * wallStickForce; 
        
        if (inputDir.magnitude > 0.1f)
        {
            float forwardInput = Vector3.Dot(inputDir, wallForward);
            if (pc.CanWallRun)
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

        jumpFallSounds.CallJumpSoundMethod();
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
        
        if (!pc.grounded && Mathf.Abs(currentTilt) > 0.01f)
        {
            Vector3 rotation = transform.localEulerAngles;
            rotation.x = 0f;  
            rotation.z = currentTilt;  
            transform.localEulerAngles = rotation;
        }
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