using UnityEngine;

public class itemPickup : MonoBehaviour
{
    public Transform holdPoint;
    public float pickupRange = 3f;
    public LayerMask interactableLayer;
    public KeyCode interactKey = KeyCode.E;
    
    private GameObject heldItem;
    private bool isHolding;
    private Vector3 originalItemScale;
    private Collider itemCollider;

    private void Update()
    {
        if (Input.GetKeyDown(interactKey))
        {
            if (!isHolding)
            {
                TryPickup();
            }
            else
            {
                Drop();
            }
        }
    }
    
    private void LateUpdate()
    {
        if (isHolding && heldItem != null)
        {
            heldItem.transform.position = holdPoint.position;
            heldItem.transform.rotation = holdPoint.rotation;
            heldItem.transform.localScale = originalItemScale;
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
        
        heldItem.transform.position = holdPoint.position;
        heldItem.transform.rotation = holdPoint.rotation;
        
        isHolding = true;
    }
    
    private void Drop()
    {
        if (heldItem == null)
        {
            return;
        }

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
    }
}
