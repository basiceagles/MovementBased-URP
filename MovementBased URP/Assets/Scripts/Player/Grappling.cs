using UnityEngine;

public class Grappling : MonoBehaviour
{
    [Header("Grapple Settings")]
    [SerializeField] private Transform grappleGunTip;   
    [SerializeField] private Transform cam;           
    [SerializeField] private Transform player;        
    [SerializeField] public LayerMask whatIsGrappleable;
    [SerializeField] public LineRenderer lr;           
    [SerializeField] private KeyCode grappleKey = KeyCode.Mouse0;
    [SerializeField] private float maxGrappleDistance;
    [SerializeField] private float grappleSpeed;
    [SerializeField] private float grapplingCd;
    
    [Header("Rope Animation")]
    [SerializeField] private int ropeSegments;
    [SerializeField] private float ropeWaveSpeed;
    [SerializeField] private float ropeWaveHeight;
    
    private Vector3 currentGrapplePosition;
    private Vector3 grapplePoint;
    private bool grappling;
    private bool pulling;
    private float grapplingCdTimer;
    private PlayerController pc;
    private bool hookFlying;
    private float hookFlyProgress;
    private Soul targetSoul; 

    void Start()
    {
        grappleGunTip = transform;
        lr = GetComponent<LineRenderer>();
        cam = Camera.main?.transform;
        
        pc = player.GetComponent<PlayerController>();
        player = pc.transform;
    }

    bool IsValidVector(Vector3 vector)
    {
        return !float.IsNaN(vector.x) && !float.IsNaN(vector.y) && !float.IsNaN(vector.z) &&
        !float.IsInfinity(vector.x) && !float.IsInfinity(vector.y) && !float.IsInfinity(vector.z);
    }

    private void Update()
    {
        if (Input.GetKeyDown(grappleKey))
        {
            StartGrapple();
        }
        if (Input.GetKeyUp(grappleKey) && grappling)
        {
            StopGrapple();
        }
        if (grapplingCdTimer > 0)
        {
            grapplingCdTimer -= Time.deltaTime;
        }
        if (pulling)
        {
            UpdateGrapplePull();
        }
    }

    private void UpdateGrapplePull()
    {
        
        float dist = Vector3.Distance(player.position, grapplePoint);
        if (dist < 1f) 
        {
            StopGrapple(); 
            return; 
        }
        
        Vector3 dir = (grapplePoint - player.position).normalized;
        float speed = grappleSpeed;
        
        float gravityCompensation = Mathf.Abs(pc.gravity);
        
        pc.horizontalVelocity = new Vector3(dir.x, 0f, dir.z) * speed;
        pc.verticalVelocity = dir.y * speed + gravityCompensation * Time.deltaTime;
    }

    private void LateUpdate()
    {
        DrawRope();
    }

    private void StartGrapple()
    {
        if (cam == null || pc == null || grapplingCdTimer > 0 || grappling) 
        {
            return;
        }
        grappling = true;
        bool hit = Physics.Raycast(cam.position, cam.forward, out RaycastHit hitInfo, maxGrappleDistance);
        grapplePoint = hit ? hitInfo.point : cam.position + cam.forward * maxGrappleDistance;
        
        targetSoul = null;
        if (hit)
        {
            Soul soul = hitInfo.collider.GetComponent<Soul>();
            if (soul != null)
            {
                targetSoul = soul;
            }
        }
        
        lr.enabled = true; lr.positionCount = ropeSegments;
        currentGrapplePosition = grappleGunTip.position;
        hookFlying = true;
        hookFlyProgress = 0f;
        
        if (targetSoul != null)
        {
            Invoke(nameof(StopGrapple), 0.5f);
        }
        else if (hit && ((1 << hitInfo.collider.gameObject.layer) & whatIsGrappleable) != 0)
        {
            pc.activeGrappling = true; pulling = true;
        }
        else
        {
            Invoke(nameof(StopGrapple), 0.5f);
        }
    }

    private void StopGrapple()
    {
        if (!grappling) 
        {
            return;
        }
        CancelInvoke();
        pulling = false; 
        pc.activeGrappling = false;
        grappling = false; 
        grapplingCdTimer = grapplingCd;
        hookFlying = false;
        lr.enabled = false; 
        lr.positionCount = 0;
    }

    private void DrawRope()
    {
        if (!grappling || lr.positionCount < 2) 
        {
            return;
        }

        Vector3 startPos = grappleGunTip.position;
        Vector3 endPos = grapplePoint;

        if (!IsValidVector(startPos) || !IsValidVector(endPos))
        {
            return;
        }

        if (hookFlying)
        {
            hookFlyProgress += Time.deltaTime * 8f;
            if (hookFlyProgress >= 1f)
            {
                hookFlyProgress = 1f;
                hookFlying = false;
                
                if (targetSoul != null)
                {
                    targetSoul.Collect();
                    targetSoul = null;
                }
            }
            currentGrapplePosition = Vector3.Lerp(startPos, endPos, hookFlyProgress);
        }
        else
        {
            currentGrapplePosition = endPos;
        }

        if (!IsValidVector(currentGrapplePosition))
        {
            currentGrapplePosition = endPos;
        }

        int segments = lr.positionCount;
        for (int i = 0; i < segments; i++)
        {
            float t = segments > 1 ? (float)i / (segments - 1) : 0f;
            Vector3 point = Vector3.Lerp(startPos, currentGrapplePosition, t);
            
            if (hookFlying && segments > 2)
            {
                Vector3 ropeDirection = (currentGrapplePosition - startPos);
                if (ropeDirection.magnitude > 0.01f)
                {
                    ropeDirection = ropeDirection.normalized;
                    Vector3 perpendicular = Vector3.Cross(ropeDirection, Vector3.up);
                    
                    if (perpendicular.magnitude < 0.1f)
                        perpendicular = Vector3.Cross(ropeDirection, Vector3.right);
                    
                    if (perpendicular.magnitude > 0.01f)
                    {
                        perpendicular = perpendicular.normalized;
                        
                        float wave1 = Mathf.Sin(t * Mathf.PI * 2f + Time.time * ropeWaveSpeed) * ropeWaveHeight;
                        float wave2 = Mathf.Sin(t * Mathf.PI * 3f + Time.time * ropeWaveSpeed * 1.5f) * ropeWaveHeight * 0.3f;
                        float combinedWave = wave1 + wave2;
                        
                        float fadeOut = Mathf.Pow(Mathf.Sin(t * Mathf.PI), 0.5f);
                        combinedWave *= fadeOut;
                        
                        float intensity = Mathf.Clamp01(hookFlyProgress * 2f);
                        combinedWave *= intensity;
                        
                        Vector3 waveOffset = perpendicular * combinedWave;
                        if (IsValidVector(waveOffset))
                        {
                            point += waveOffset;
                        }
                    }
                }
            }
            
            lr.SetPosition(i, point);
        }
    }
}
