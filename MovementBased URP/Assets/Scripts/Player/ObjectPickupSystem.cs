using UnityEngine;

public class ObjectPickupSystem : MonoBehaviour
{
    [Header("ПОДБОР ОБЪЕКТОВ")]
    [SerializeField] private KeyCode pickupKey = KeyCode.E;
    [SerializeField] private float pickupRange = 3f;
    [SerializeField] private string pickupTag = "barrel";
    [SerializeField] private Transform holdPoint;
    [SerializeField] private Vector3 holdOffset = Vector3.zero;
    [SerializeField] private float pickupSpeed = 15f;
    
    [Header("БРОСОК")]
    [SerializeField] private KeyCode throwKey = KeyCode.Mouse0;
    [SerializeField] private float throwForce = 20f;
    [SerializeField] private float throwUpForce = 5f;
    [SerializeField] private bool enableChargeThrow = true;
    [SerializeField] private float maxChargeTime = 2f;
    [SerializeField] private float maxChargeMultiplier = 2f;
    [SerializeField] private bool addSpinOnThrow = true;
    [SerializeField] private float spinForce = 5f;
    
    [Header("ФИЗИКА")]
    [SerializeField] private bool usePhysicsHold = true;
    [SerializeField] private float holdStrength = 1000f;
    [SerializeField] private float holdDamping = 10f;
    [SerializeField] private bool freezeRotationWhenHeld = true;
    
    [Header("КАМЕРА")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Transform raycastOrigin;
    
    private Rigidbody heldRigidbody;
    private RigidbodyConstraints originalConstraints;
    private float throwChargeTime = 0f;
    private bool isCharging = false;
    private GameObject currentLookTarget;
    private GameObject heldObject;
    
    private void Start()
    {
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }
        
        if (raycastOrigin == null && playerCamera != null)
        {
            raycastOrigin = playerCamera.transform;
        }
    }
    
    private void Update()
    {
        if (heldObject == null)
        {
            CheckForPickupObject();
            
            if (Input.GetKeyDown(pickupKey) && currentLookTarget != null)
            {
                PickupObject(currentLookTarget);
            }
        }
        else
        {
            HoldObject();
            
            if (enableChargeThrow)
            {
                HandleChargeThrow();
            }
            else
            {
                if (Input.GetKeyDown(throwKey))
                {
                    ThrowObject(1f);
                }
            }
            
            if (Input.GetKeyDown(pickupKey))
            {
                DropObject();
            }
        }
    }
    
    private void FixedUpdate()
    {
        if (heldObject != null && usePhysicsHold)
        {
            ApplyPhysicsHold();
        }
    }
    
    private void CheckForPickupObject()
    {
        currentLookTarget = null;
        
        if (raycastOrigin == null) return;
        
        Ray ray = new Ray(raycastOrigin.position, raycastOrigin.forward);
        
        if (Physics.Raycast(ray, out RaycastHit hit, pickupRange))
        {
            if (hit.collider.CompareTag(pickupTag))
            {
                currentLookTarget = hit.collider.gameObject;
            }
        }
    }
    
    private void PickupObject(GameObject obj)
    {
        Rigidbody rb = obj.GetComponent<Rigidbody>();
        
        if (rb == null) return;
        
        heldObject = obj;
        heldRigidbody = rb;
        originalConstraints = rb.constraints;
        
        if (freezeRotationWhenHeld)
        {
            rb.constraints = RigidbodyConstraints.FreezeRotation;
        }
        
        if (!usePhysicsHold)
        {
            rb.isKinematic = true;
        }
        else
        {
            rb.useGravity = false;
        }
    }
    
    private void HoldObject()
    {
        if (heldObject == null || holdPoint == null) return;
        
        Vector3 targetPosition = holdPoint.position + holdPoint.TransformDirection(holdOffset);
        
        if (!usePhysicsHold)
        {
            heldObject.transform.position = Vector3.Lerp(
                heldObject.transform.position,
                targetPosition,
                pickupSpeed * Time.deltaTime
            );
            
            heldObject.transform.rotation = Quaternion.Slerp(heldObject.transform.rotation, holdPoint.rotation, pickupSpeed * Time.deltaTime);
        }
    }
    
    private void ApplyPhysicsHold()
    {
        if (heldRigidbody == null || holdPoint == null) return;
        
        Vector3 targetPosition = holdPoint.position + holdPoint.TransformDirection(holdOffset);
        Vector3 direction = targetPosition - heldRigidbody.position;
        
        Vector3 force = direction * holdStrength - heldRigidbody.linearVelocity * holdDamping;
        heldRigidbody.AddForce(force);

        heldRigidbody.MoveRotation(Quaternion.Slerp(heldRigidbody.rotation, holdPoint.rotation, pickupSpeed * Time.deltaTime));
    }
    
    private void HandleChargeThrow()
    {
        if (Input.GetKeyDown(throwKey))
        {
            isCharging = true;
            throwChargeTime = 0f;
        }
        
        if (isCharging)
        {
            throwChargeTime += Time.deltaTime;
            throwChargeTime = Mathf.Min(throwChargeTime, maxChargeTime);
        }
        
        if (Input.GetKeyUp(throwKey) && isCharging)
        {
            float chargeMultiplier = 1f + (throwChargeTime / maxChargeTime) * (maxChargeMultiplier - 1f);
            ThrowObject(chargeMultiplier);
            
            isCharging = false;
            throwChargeTime = 0f;
        }
    }
    
    private void ThrowObject(float forceMultiplier)
    {
        if (heldRigidbody == null || raycastOrigin == null) return;
        
        heldRigidbody.isKinematic = false;
        heldRigidbody.useGravity = true;
        heldRigidbody.constraints = originalConstraints;
        
        Vector3 throwDirection = raycastOrigin.forward;
        Vector3 force = throwDirection * throwForce * forceMultiplier;
        force.y += throwUpForce * forceMultiplier;
        
        heldRigidbody.linearVelocity = Vector3.zero;
        heldRigidbody.angularVelocity = Vector3.zero;
        heldRigidbody.AddForce(force, ForceMode.Impulse);
        
        if (addSpinOnThrow)
        {
            Vector3 randomSpin = new Vector3(Random.Range(-spinForce, spinForce), Random.Range(-spinForce, spinForce), Random.Range(-spinForce, spinForce));
            heldRigidbody.AddTorque(randomSpin, ForceMode.Impulse);
        }
        
        heldObject = null;
        heldRigidbody = null;
    }
    
    private void DropObject()
    {
        if (heldRigidbody == null) return;
        
        heldRigidbody.isKinematic = false;
        heldRigidbody.useGravity = true;
        heldRigidbody.constraints = originalConstraints;
        
        heldObject = null;
        heldRigidbody = null;
    }
    
    public float GetChargeProgress()
    {
        if (!enableChargeThrow || !isCharging) return 0f;
        return throwChargeTime / maxChargeTime;
    }
    
    public bool IsHoldingObject()
    {
        return heldObject != null;
    }
    
    public GameObject GetHeldObject()
    {
        return heldObject;
    }
}
