using UnityEngine;
using System.Collections;

public class Bug3DController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float minMoveDuration = 0.5f;
    public float maxMoveDuration = 2f;
    public float minPauseDuration = 1f;
    public float maxPauseDuration = 3f;
    public float movementSpeed = 2f;

    [Header("Plane Switching")]
    public float planeSwitchChance = 0.1f;
    public float minPlaneSwitchCooldown = 5f;
    public float neighborSearchRadius = 2f;
    public LayerMask surfaceLayerMask = -1;

    [Header("3D Settings")]
    public float surfaceOffset = 0.01f;
    public bool maintainSpriteOrientation = true;
    public float edgeDetectionDistance = 0.3f;

    [Header("Debug")]
    public bool showDebugRays = false;

    private Vector3 currentDirection;
    private bool isMoving = false;
    private Collider currentSurface;
    private Vector3 surfaceNormal;
    private float lastPlaneSwitchTime = 0f;
    private Coroutine behaviorCoroutine;
    private Quaternion surfaceRotationOffset;

    void Start()
    {
        // Find initial surface
        FindAndStickToSurface();
        
        // Start behavior coroutine
        StartBugBehavior();
    }

    void Update()
    {
        if (isMoving && currentSurface != null)
        {
            MoveAlongSurface();
            
            // Constantly check for edges while moving
            if (IsAtEdge())
            {
                SwitchToOppositeSide();
            }
        }
    }

    void StartBugBehavior()
    {
        if (behaviorCoroutine != null)
            StopCoroutine(behaviorCoroutine);
        behaviorCoroutine = StartCoroutine(BugBehavior());
    }

    void FindAndStickToSurface()
    {
        RaycastHit hit;
        
        // Cast rays in all directions to find surface
        Vector3[] directions = { 
            Vector3.down, Vector3.up, Vector3.forward, 
            Vector3.back, Vector3.left, Vector3.right 
        };
        
        foreach (Vector3 dir in directions)
        {
            if (Physics.Raycast(transform.position, dir, out hit, 2f, surfaceLayerMask))
            {
                StickToSurface(hit.collider, hit.point, hit.normal);
                return;
            }
        }
        
        // If no surface found, log warning
        Debug.LogWarning("Bug could not find surface to stick to!");
    }

    void StickToSurface(Collider surface, Vector3 contactPoint, Vector3 normal)
    {
        currentSurface = surface;
        surfaceNormal = normal;
        
        // Calculate surface rotation relative to world up
        CalculateSurfaceRotation(normal);
        
        // Position bug on surface with small offset
        transform.position = contactPoint + normal * surfaceOffset;
        
        // Apply surface rotation to bug
        ApplySurfaceRotation();
        
        // Reset scale to ensure proper flipping
        transform.localScale = new Vector3(1, 1, 1);
    }

    void CalculateSurfaceRotation(Vector3 normal)
    {
        // Calculate the rotation that aligns the surface normal with world up
        // This gives us the rotation of the surface relative to world space
        if (normal != Vector3.up)
        {
            Quaternion targetRotation = Quaternion.FromToRotation(Vector3.up, normal);
            surfaceRotationOffset = targetRotation;
        }
        else
        {
            surfaceRotationOffset = Quaternion.identity;
        }
    }

    void ApplySurfaceRotation()
    {
        if (maintainSpriteOrientation)
        {
            // Combine surface rotation with camera-facing orientation
            Quaternion cameraRotation = Quaternion.LookRotation(GetCameraForward(), Vector3.up);
            transform.rotation = cameraRotation;
            
            // Apply surface rotation as a local rotation to maintain upright but follow surface
            transform.rotation = surfaceRotationOffset * transform.rotation;
        }
        else
        {
            // Align completely to surface normal with surface rotation
            transform.up = surfaceNormal;
        }
    }

    Vector3 GetCameraForward()
    {
        if (Camera.main != null)
            return Camera.main.transform.forward;
        return Vector3.forward;
    }

    IEnumerator BugBehavior()
    {
        while (true)
        {
            // Rest phase (1-3 seconds)
            isMoving = false;
            float restTime = Random.Range(minPauseDuration, maxPauseDuration);
            yield return new WaitForSeconds(restTime);

            // Movement phase (0.5-2 seconds)
            yield return StartCoroutine(MoveInStraightLine());

            // Chance to switch planes after movement
            if (CanSwitchPlane())
            {
                TrySwitchPlane();
            }
        }
    }

    IEnumerator MoveInStraightLine()
    {
        isMoving = true;
        currentDirection = GetRandomSurfaceDirection();
        float moveTime = Random.Range(minMoveDuration, maxMoveDuration);
        
        float timer = 0f;
        while (timer < moveTime && !IsAtEdge())
        {
            timer += Time.deltaTime;
            yield return null;
        }
        
        isMoving = false;
    }

    Vector3 GetRandomSurfaceDirection()
    {
        // Get a random direction in local surface space
        Vector3 localRandomDir = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized;
        
        // Transform the direction to world space based on surface rotation
        Vector3 worldDirection = surfaceRotationOffset * localRandomDir;
        
        // Project onto surface plane to ensure it's tangent to the surface
        Vector3 surfaceDir = Vector3.ProjectOnPlane(worldDirection, surfaceNormal).normalized;
        
        return surfaceDir != Vector3.zero ? surfaceDir : GetFallbackDirection();
    }

    Vector3 GetFallbackDirection()
    {
        // Get a fallback direction based on surface rotation
        Vector3 fallbackDir = surfaceRotationOffset * Vector3.forward;
        return Vector3.ProjectOnPlane(fallbackDir, surfaceNormal).normalized;
    }

    void MoveAlongSurface()
    {
        if (currentSurface == null) return;

        // Apply surface rotation to movement direction
        Vector3 worldDirection = currentDirection;
        Vector3 movement = worldDirection * movementSpeed * Time.deltaTime;
        Vector3 newPosition = transform.position + movement;

        // Cast ray to ensure we stay on surface
        RaycastHit hit;
        Vector3 rayStart = newPosition + surfaceNormal * 0.2f;
        
        if (Physics.Raycast(rayStart, -surfaceNormal, out hit, 0.5f, surfaceLayerMask))
        {
            if (hit.collider == currentSurface)
            {
                // Move along current surface
                transform.position = hit.point + surfaceNormal * surfaceOffset;
                
                // Update surface rotation if the normal has changed (for curved surfaces)
                if (hit.normal != surfaceNormal)
                {
                    surfaceNormal = hit.normal;
                    CalculateSurfaceRotation(surfaceNormal);
                    ApplySurfaceRotation();
                }
            }
            else
            {
                // Switch to new surface
                StickToSurface(hit.collider, hit.point, hit.normal);
            }
        }
        else
        {
            // Reached edge, change direction immediately
            currentDirection = GetRandomSurfaceDirection();
        }

        UpdateSpriteOrientation();
    }

    void UpdateSpriteOrientation()
    {
        if (currentDirection != Vector3.zero)
        {
            // Transform direction to surface-local space for consistent flipping
            Vector3 localDirection = Quaternion.Inverse(surfaceRotationOffset) * currentDirection;
            
            // Flip sprite based on local X direction
            Vector3 newScale = transform.localScale;
            newScale.x = Mathf.Abs(newScale.x) * Mathf.Sign(localDirection.x);
            transform.localScale = newScale;
        }
    }

    bool IsAtEdge()
    {
        if (currentSurface == null) return false;

        // Cast ray in movement direction to detect edges
        RaycastHit hit;
        Vector3 rayStart = transform.position + surfaceNormal * 0.1f;
        Vector3 rayDirection = currentDirection;
        
        if (!Physics.Raycast(rayStart, rayDirection, out hit, edgeDetectionDistance, surfaceLayerMask))
        {
            return true; // No surface detected = edge
        }
        
        // Check if we're about to leave current surface
        if (hit.collider != currentSurface)
        {
            return true; // Different surface = edge between objects
        }

        return false;
    }

    bool CanSwitchPlane()
    {
        return Time.time - lastPlaneSwitchTime > minPlaneSwitchCooldown && 
               Random.value < planeSwitchChance;
    }

    void TrySwitchPlane()
    {
        // Method 1: Switch to opposite side of current object
        if (Random.value < 0.7f) // Higher chance to switch sides
        {
            SwitchToOppositeSide();
        }
        // Method 2: Move to neighboring object
        else
        {
            TryMoveToNeighbor();
        }
        
        lastPlaneSwitchTime = Time.time;
    }

    void SwitchToOppositeSide()
    {
        if (currentSurface == null) return;

        Vector3 oppositeDirection = -surfaceNormal;
        RaycastHit hit;
        
        // Cast ray through the object to find opposite side
        if (Physics.Raycast(transform.position + surfaceNormal * 0.1f, oppositeDirection, out hit, 3f, surfaceLayerMask))
        {
            if (hit.collider == currentSurface)
            {
                StickToSurface(hit.collider, hit.point, hit.normal);
                StartCoroutine(FlipAnimation());
                
                // Get a new random direction after switching sides
                currentDirection = GetRandomSurfaceDirection();
                
                // Restart behavior to maintain rhythm
                StartBugBehavior();
            }
        }
    }

    void TryMoveToNeighbor()
    {
        Collider[] neighbors = Physics.OverlapSphere(transform.position, neighborSearchRadius, surfaceLayerMask);
        
        foreach (var neighbor in neighbors)
        {
            if (neighbor != currentSurface && neighbor.gameObject != gameObject)
            {
                // Find the closest point on the neighbor surface
                Vector3 closestPoint = neighbor.ClosestPoint(transform.position);
                
                // Cast ray to get proper surface normal
                RaycastHit hit;
                Vector3[] searchDirections = { Vector3.down, Vector3.up, Vector3.forward, Vector3.back, Vector3.left, Vector3.right };
                
                foreach (Vector3 dir in searchDirections)
                {
                    if (Physics.Raycast(closestPoint + dir * 0.5f, -dir, out hit, 1f, surfaceLayerMask))
                    {
                        if (hit.collider == neighbor)
                        {
                            StickToSurface(neighbor, hit.point, hit.normal);
                            currentDirection = GetRandomSurfaceDirection();
                            StartBugBehavior();
                            return;
                        }
                    }
                }
            }
        }
    }

    IEnumerator FlipAnimation()
    {
        // Simple scale animation for flip effect
        float duration = 0.3f;
        float timer = 0f;
        Vector3 originalScale = transform.localScale;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float progress = timer / duration;
            float scaleX = Mathf.Lerp(originalScale.x, 0.1f, progress < 0.5f ? progress * 2 : (1 - progress) * 2);
            transform.localScale = new Vector3(scaleX, originalScale.y, originalScale.z);
            yield return null;
        }

        transform.localScale = originalScale;
    }

    void OnDrawGizmosSelected()
    {
        if (!showDebugRays) return;

        // Draw movement direction
        if (Application.isPlaying && isMoving)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position, currentDirection * 0.5f);
        }

        // Draw surface normal
        if (currentSurface != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(transform.position, surfaceNormal * 0.3f);
        }

        // Draw surface rotation axes
        if (Application.isPlaying && currentSurface != null)
        {
            Gizmos.color = Color.green;
            Vector3 surfaceRight = surfaceRotationOffset * Vector3.right;
            Vector3 surfaceForward = surfaceRotationOffset * Vector3.forward;
            Gizmos.DrawRay(transform.position, surfaceRight * 0.3f);
            Gizmos.DrawRay(transform.position, surfaceForward * 0.3f);
        }

        // Draw neighbor search radius
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, neighborSearchRadius);

        // Draw edge detection
        if (Application.isPlaying && isMoving)
        {
            Gizmos.color = Color.magenta;
            Vector3 rayStart = transform.position + surfaceNormal * 0.1f;
            Gizmos.DrawRay(rayStart, currentDirection * edgeDetectionDistance);
        }
    }
}