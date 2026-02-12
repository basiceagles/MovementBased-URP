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

    private Vector3 currentDirection;
    private bool isMoving = false;
    private Collider currentSurface;
    private Vector3 surfaceNormal;
    private float lastPlaneSwitchTime = 0f;
    private Coroutine behaviorCoroutine;
    private Quaternion surfaceRotationOffset;

    void Start()
    {
        FindAndStickToSurface();
        StartBugBehavior();
    }

    void Update()
    {
        if (isMoving && currentSurface != null)
        {
            MoveAlongSurface();
            
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
    }

    void StickToSurface(Collider surface, Vector3 contactPoint, Vector3 normal)
    {
        currentSurface = surface;
        surfaceNormal = normal;
        
        CalculateSurfaceRotation(normal);
        
        transform.position = contactPoint + normal * surfaceOffset;
        
        ApplySurfaceRotation();
        
        transform.localScale = new Vector3(1, 1, 1);
    }

    void CalculateSurfaceRotation(Vector3 normal)
    {
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
            Quaternion cameraRotation = Quaternion.LookRotation(GetCameraForward(), Vector3.up);
            transform.rotation = cameraRotation;
            transform.rotation = surfaceRotationOffset * transform.rotation;
        }
        else
        {
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
            isMoving = false;
            float restTime = Random.Range(minPauseDuration, maxPauseDuration);
            yield return new WaitForSeconds(restTime);

            yield return StartCoroutine(MoveInStraightLine());

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
        Vector3 localRandomDir = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized;
        Vector3 worldDirection = surfaceRotationOffset * localRandomDir;
        Vector3 surfaceDir = Vector3.ProjectOnPlane(worldDirection, surfaceNormal).normalized;
        return surfaceDir != Vector3.zero ? surfaceDir : GetFallbackDirection();
    }

    Vector3 GetFallbackDirection()
    {
        Vector3 fallbackDir = surfaceRotationOffset * Vector3.forward;
        return Vector3.ProjectOnPlane(fallbackDir, surfaceNormal).normalized;
    }

    void MoveAlongSurface()
    {
        if (currentSurface == null) return;

        Vector3 worldDirection = currentDirection;
        Vector3 movement = worldDirection * movementSpeed * Time.deltaTime;
        Vector3 newPosition = transform.position + movement;

        RaycastHit hit;
        Vector3 rayStart = newPosition + surfaceNormal * 0.2f;
        
        if (Physics.Raycast(rayStart, -surfaceNormal, out hit, 0.5f, surfaceLayerMask))
        {
            if (hit.collider == currentSurface)
            {
                transform.position = hit.point + surfaceNormal * surfaceOffset;
                
                if (hit.normal != surfaceNormal)
                {
                    surfaceNormal = hit.normal;
                    CalculateSurfaceRotation(surfaceNormal);
                    ApplySurfaceRotation();
                }
            }
            else
            {
                StickToSurface(hit.collider, hit.point, hit.normal);
            }
        }
        else
        {
            currentDirection = GetRandomSurfaceDirection();
        }

        UpdateSpriteOrientation();
    }

    void UpdateSpriteOrientation()
    {
        if (currentDirection != Vector3.zero)
        {
            Vector3 localDirection = Quaternion.Inverse(surfaceRotationOffset) * currentDirection;
            
            Vector3 newScale = transform.localScale;
            newScale.x = Mathf.Abs(newScale.x) * Mathf.Sign(localDirection.x);
            transform.localScale = newScale;
        }
    }

    bool IsAtEdge()
    {
        if (currentSurface == null) return false;

        RaycastHit hit;
        Vector3 rayStart = transform.position + surfaceNormal * 0.1f;
        Vector3 rayDirection = currentDirection;
        
        if (!Physics.Raycast(rayStart, rayDirection, out hit, edgeDetectionDistance, surfaceLayerMask))
        {
            return true; 
        }
        
        if (hit.collider != currentSurface)
        {
            return true; 
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
        if (Random.value < 0.7f) 
        {
            SwitchToOppositeSide();
        }
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
        
        if (Physics.Raycast(transform.position + surfaceNormal * 0.1f, oppositeDirection, out hit, 3f, surfaceLayerMask))
        {
            if (hit.collider == currentSurface)
            {
                StickToSurface(hit.collider, hit.point, hit.normal);
                StartCoroutine(FlipAnimation());
                currentDirection = GetRandomSurfaceDirection();
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
                Vector3 closestPoint = neighbor.ClosestPoint(transform.position);
                
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
}