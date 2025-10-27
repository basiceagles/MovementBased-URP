using UnityEngine;

public class PlayerPickup : MonoBehaviour
{
    [Header("Pickup Settings")]
    public Transform holdPoint; // Assign the hold point transform in inspector
    public float pickupRange = 2f; // How far the player can reach to pick up objects
    public KeyCode interactKey = KeyCode.E;
    
    [Header("Physics Settings")]
    public float throwForce = 10f; // Optional: add some force when throwing
    
    private GameObject heldObject;
    private bool isHolding = false;

    void Start()
    {
        
    }
    
    void Update()
    {
        HandleInput();

    }
    
    void HandleInput()
    {
        if (Input.GetKeyDown(interactKey))
        {
            if (!isHolding)
            {
                // Try to pick up an object
                TryPickup();
            }
            else
            {
                // Drop the currently held object
                DropObject();
            }
        }
    }
    
    void TryPickup()
    {
        // Raycast to find interactable objects in front of the player
        RaycastHit hit;
        Vector3 rayOrigin = transform.position;
        Vector3 rayDirection = transform.forward;
        
        if (Physics.Raycast(rayOrigin, rayDirection, out hit, pickupRange))
        {
            // Check if the hit object is on the Interactable layer
            if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Interactable"))
            {
                PickupObject(hit.collider.gameObject);
            }
        }
    }
    
    void PickupObject(GameObject objectToPickup)
    {
        heldObject = objectToPickup;
        
        // Set object's position and parent to hold point
        heldObject.transform.position = holdPoint.position;
        heldObject.transform.SetParent(holdPoint);
        
        isHolding = true;
        
        Debug.Log("Picked up: " + heldObject.name);
    }

    void DropObject()
    {
        if (heldObject == null) return;

        // Remove parent
        heldObject.transform.SetParent(null);

        Debug.Log("Dropped: " + heldObject.name);

        // Clear references
        heldObject = null;
        isHolding = false;
    }
    
    // Safety cleanup
    void OnDisable()
    {
        if (isHolding)
        {
            DropObject();
        }
    }
    
    void OnDestroy()
    {
        if (isHolding)
        {
            DropObject();
        }
    }
}