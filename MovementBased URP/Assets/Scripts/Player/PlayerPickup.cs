using UnityEngine;

public class PlayerPickup : MonoBehaviour
{
    [Header("Pickup Settings")]
    public Transform holdPoint;
    public float pickupRange = 2f;
    public KeyCode interactKey = KeyCode.E;
    
    [Header("Physics Settings")]
    public float throwForce = 10f;
    
    private GameObject heldObject;
    private bool isHolding = false;

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
                TryPickup();
            }
            else
            {
                DropObject();
            }
        }
    }
    
    void TryPickup()
    {
        RaycastHit hit;
        Vector3 rayOrigin = transform.position;
        Vector3 rayDirection = transform.forward;
        
        if (Physics.Raycast(rayOrigin, rayDirection, out hit, pickupRange))
        {
            if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Interactable"))
            {
                PickupObject(hit.collider.gameObject);
            }
        }
    }
    
    void PickupObject(GameObject objectToPickup)
    {
        heldObject = objectToPickup;
        heldObject.transform.position = holdPoint.position;
        heldObject.transform.SetParent(holdPoint);
        isHolding = true;
    }

    void DropObject()
    {
        if (heldObject == null) return;
        heldObject.transform.SetParent(null);
        heldObject = null;
        isHolding = false;
    }
    
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