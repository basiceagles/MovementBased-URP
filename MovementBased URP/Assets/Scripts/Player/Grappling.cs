using UnityEngine;

public class Grappling : MonoBehaviour
{
    [Header("Grapple Settings")]
    [SerializeField] private Transform grappleGunTip;   
    [SerializeField] private Transform cam;           
    [SerializeField] private Transform player;        
    [SerializeField] public LayerMask whatIsGrappleable;
    [SerializeField] private LayerMask nodeLayer;
    [SerializeField] public LineRenderer lr;           
    [SerializeField] private KeyCode grappleKey = KeyCode.Mouse0;
    [SerializeField] private float maxGrappleDistance;
    [SerializeField] private float grappleSpeed;
    [SerializeField] private float grapplingCd;
    
    [Header("Rope Animation")]
    [SerializeField] private int ropeSegments;
    [SerializeField] private float ropeWaveSpeed;
    [SerializeField] private float ropeWaveHeight;
    
    public float currentGrappleSpeed;
    
    private Vector3 currentGrapplePosition;
    private Vector3 grapplePoint;
    private bool grappling;
    private bool pulling;
    private float grapplingCdTimer;
    private PlayerController pc;
    private bool hookFlying;
    private float hookFlyProgress;
    private Swinging swinging;
    private Lever targetLever;
    private Transform carriedNode;
    private Rigidbody carriedNodeRb;
    private ItemPickup itemPickup;

    void Start()
    {
        grappleGunTip = transform;
        lr = GetComponent<LineRenderer>();
        cam = Camera.main?.transform;
        
        pc = player.GetComponent<PlayerController>();
        player = pc.transform;
        swinging = FindObjectOfType<Swinging>();
        itemPickup = FindObjectOfType<ItemPickup>();
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
        if (carriedNode != null)
        {
            UpdateCarriedNode();
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
        
        currentGrappleSpeed = new Vector3(pc.horizontalVelocity.x, pc.verticalVelocity, pc.horizontalVelocity.z).magnitude;
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
        if (swinging != null)
        {
            swinging.enabled = false;
        }
        grappling = true;
        bool hit = Physics.Raycast(cam.position, cam.forward, out RaycastHit hitInfo, maxGrappleDistance);
        grapplePoint = hit ? hitInfo.point : cam.position + cam.forward * maxGrappleDistance;

        targetLever = null;
        if (hit)
        {
            Lever lever = hitInfo.collider.GetComponent<Lever>();
            if (lever != null)
            {
                targetLever = lever;
            }
        }
        
        lr.enabled = true; lr.positionCount = ropeSegments;
        currentGrapplePosition = grappleGunTip.position;
        hookFlying = true;
        hookFlyProgress = 0f;

        if (targetLever != null)
        {
            Invoke(nameof(StopGrapple), 0.5f);
        }
        else if (hit && ((1 << hitInfo.collider.gameObject.layer) & nodeLayer) != 0)
        {
            StartCarryNode(hitInfo.collider.transform);
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
        
        if (pulling)
        {
            pc.currentSlideSpeed = currentGrappleSpeed;
        }
        if (carriedNodeRb != null)
        {
            carriedNodeRb.isKinematic = false;
            carriedNodeRb.useGravity = true;
        }
        
        pulling = false; 
        carriedNode = null;
        carriedNodeRb = null;
        pc.activeGrappling = false;
        grappling = false; 
        grapplingCdTimer = grapplingCd;
        if (swinging != null)
        {
            swinging.enabled = true;
        }
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

        if (carriedNode != null)
        {
            endPos = carriedNode.position;
        }

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

                if (targetLever != null)
                {
                    targetLever.Collect();
                    targetLever = null;
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

    private void StartCarryNode(Transform nodeTransform)
    {
        carriedNode = nodeTransform;
        carriedNodeRb = carriedNode != null ? carriedNode.GetComponent<Rigidbody>() : null;
        if (carriedNodeRb != null)
        {
            carriedNodeRb.isKinematic = true;
            carriedNodeRb.useGravity = false;
        }
        hookFlying = false;
        if (carriedNode != null)
        {
            currentGrapplePosition = carriedNode.position;
        }
    }

    private void UpdateCarriedNode()
    {
        if (carriedNode == null)
        {
            return;
        }

        Vector3 targetPos = player.position;
        float dist = Vector3.Distance(carriedNode.position, targetPos);

        if (dist < 1f)
        {
            if (itemPickup != null)
            {
                itemPickup.ForcePickup(carriedNode.gameObject);
            }
            carriedNode = null;
            carriedNodeRb = null;
            StopGrapple();
            return;
        }

        float speed = grappleSpeed;
        Vector3 dir = (targetPos - carriedNode.position).normalized;
        carriedNode.position += dir * speed * Time.deltaTime;
    }

    public void SetGrappleKey(KeyCode newKey)
    {
        grappleKey = newKey;
    }
}
