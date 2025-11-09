using Unity.VisualScripting;
using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    public Transform holdPoint;
    public float pickupRange = 3f;
    public LayerMask interactableLayer;
    public LayerMask grinderLayer;
    public LayerMask leverLayer;
    public KeyCode interactKey = KeyCode.E;
    
    private GameObject heldItem;
    public bool isHolding;
    private Vector3 originalItemScale;
    private Collider itemCollider;
    private Grinder currentGrinder;
    private Grinder previousGrinder;
    private GameObject mainCamera;
    private ItemPickup itemPickup;
    private Camera playerCamera;
    private bool wasLookingAtGrinder = false;
    private bool isLookingAtGrinder;
    private bool isLookingAtLever;

    private GameObject swing;
    private GameObject grapple;

    private Lever currentLever;
    private Lever previousLever;
    private bool wasLookingAtLever = false;
    private bool isCollected;

    public bool disableGrappleAndSwing = false;

    void Start()
    {
        swing = GameObject.Find("Swing");
        grapple = GameObject.Find("Grapple");
        playerCamera = GetComponent<Camera>();
    }

    private void Update()
    {
        HandleRaycast();

        if (Input.GetKeyDown(interactKey))
        {
            if (currentLever != null)
            {
                currentLever.Collect();
                currentLever.HideUIKeybind();
            }
            if (!isHolding && !isLookingAtGrinder)
            {
                TryPickup();
            }
            else if (!isLookingAtGrinder && !isLookingAtLever)
            {
                Drop();
            }
            else if (isLookingAtGrinder && isHolding)
            {
                InsertNode();
            }
        }
    }

    private void LateUpdate()
    {
        if (isHolding && heldItem != null)
        {
            // Position and rotation are now handled by the parent-child relationship
            heldItem.transform.localScale = originalItemScale;
        }
    }

    void HandleRaycast()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;

        isLookingAtLever = false;
        Lever hitLever = null;

        isLookingAtGrinder = false;

        Grinder hitGrinder = null;

        if (currentLever != null)
            isCollected = currentLever.collected;

        if (Physics.Raycast(ray, out hit, pickupRange, grinderLayer))
        {
            hitGrinder = hit.collider.GetComponent<Grinder>();
            if (hitGrinder != null)
            {
                currentGrinder = hitGrinder;
                isLookingAtGrinder = true;
            }
        }

        // Cast ray to find interactable objects with Lever script
        if (Physics.Raycast(ray, out hit, pickupRange, leverLayer))
        {
            hitLever = hit.collider.GetComponent<Lever>();
            if (hitLever != null)
            {
                currentLever = hitLever;
                isLookingAtLever = true;
            }
        }

        //grinder ui
        if (isLookingAtGrinder && currentGrinder != null && !wasLookingAtGrinder && isHolding)
        {
            currentGrinder.ShowUITip();
            wasLookingAtGrinder = true;
        }

        //grinder ui
        if (!isLookingAtGrinder && wasLookingAtGrinder && previousGrinder != null)
        {
            previousGrinder.HideUITip();
            wasLookingAtGrinder = false;
        }

        // Update previous lever reference
        if (!isLookingAtGrinder)
        {
            previousGrinder = currentGrinder;
            currentGrinder = null;
        }
        else
        {
            previousGrinder = currentGrinder;
        }

        // Handle ShowUIKeybind call (only when starting to look at a lever)
        if (isLookingAtLever && currentLever != null && !wasLookingAtLever && !isCollected)
        {
            currentLever.ShowUIKeybind();
            wasLookingAtLever = true;
        }

        // Handle HideUIKeybind call (when stopping looking at a lever)
        if (!isLookingAtLever && wasLookingAtLever && previousLever != null)
        {
            previousLever.HideUIKeybind();
            wasLookingAtLever = false;
        }

        // Update previous lever reference
        if (!isLookingAtLever)
        {
            previousLever = currentLever;
            currentLever = null;
        }
        else
        {
            previousLever = currentLever;
        }
    }
    
    private void TryPickup()
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            return;
        }
        
        RaycastHit hit;
        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, pickupRange, interactableLayer))
        {
            Pickup(hit.collider.gameObject);
        }
    }
    
    private void Pickup(GameObject item)
    {
        heldItem = item;
        
        // Store the original world scale before parenting to preserve the object's intended size
        // This prevents scale accumulation from multiple pickups
        Vector3 worldScale = heldItem.transform.lossyScale;
        originalItemScale = heldItem.transform.localScale;
        
        itemCollider = heldItem.GetComponent<Collider>();
        if (itemCollider != null)
            itemCollider.enabled = false;
        
        Rigidbody rb = heldItem.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }
        
        // Set the holdPoint as parent
        heldItem.transform.SetParent(holdPoint);
        
        // Reset local position and rotation to zero
        heldItem.transform.localPosition = Vector3.zero;
        heldItem.transform.localRotation = Quaternion.identity;
        
        // Restore the world scale by adjusting local scale relative to the new parent
        if (holdPoint.lossyScale.x != 0 && holdPoint.lossyScale.y != 0 && holdPoint.lossyScale.z != 0)
        {
            heldItem.transform.localScale = new Vector3(
                worldScale.x / holdPoint.lossyScale.x,
                worldScale.y / holdPoint.lossyScale.y,
                worldScale.z / holdPoint.lossyScale.z
            );
        }
        
        // Update the originalItemScale to this new calculated local scale
        originalItemScale = heldItem.transform.localScale;

        isHolding = true;

        if (disableGrappleAndSwing)
        {
            grapple.SetActive(false);
            swing.SetActive(false);
        }
    }

    private void Drop()
    {
        if (heldItem == null)
        {
            return;
        }

        // Store the current world scale before removing parent
        Vector3 worldScale = heldItem.transform.lossyScale;

        // Remove the parent-child relationship
        heldItem.transform.SetParent(null);

        // Restore the world scale to maintain consistent size
        heldItem.transform.localScale = worldScale;

        if (itemCollider != null)
        {
            itemCollider.enabled = true;
        }

        Rigidbody rb = heldItem.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }

        heldItem = null;
        isHolding = false;
        itemCollider = null;

        if (disableGrappleAndSwing)
        {
            grapple.SetActive(true);
            swing.SetActive(true);
        }
    }

    private void InsertNode()
    {
        currentGrinder.GrinderInsertNode();

        // Remove parent before destroying
        if (heldItem != null)
        {
            heldItem.transform.SetParent(null);
        }
        
        Destroy(heldItem);
        isHolding = false;
        itemCollider = null;

        grapple.SetActive(true);
        swing.SetActive(true);
    }
}