using UnityEngine;

public class KeybindRaycaster : MonoBehaviour
{
    [Header("Raycast Settings")]
    public float interactionRange = 5f;
    public LayerMask interactableLayer;
    
    [Header("Interaction Key")]
    public KeyCode interactionKey = KeyCode.E;
    
    private Camera playerCamera;
    private Lever currentLever;
    private Lever previousLever;
    private bool wasLookingAtLever = false;
    private bool isCollected;
    private GameObject mainCamera;
    private ItemPickup itemPickup;

    void Start()
    {
        playerCamera = GetComponent<Camera>();
    }

    void Update()
    {
        HandleRaycast();
        HandleInteraction();
    }

    void HandleRaycast()
    {
        if (playerCamera == null) return;

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;

        bool isLookingAtLever = false;
        Lever hitLever = null;

        if (currentLever != null)
            isCollected = currentLever.collected;


        // Cast ray to find interactable objects with Lever script
        if (Physics.Raycast(ray, out hit, interactionRange, interactableLayer))
        {
            hitLever = hit.collider.GetComponent<Lever>();
            if (hitLever != null)
            {
                currentLever = hitLever;
                isLookingAtLever = true;
            }
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

    void HandleInteraction()
    {
        // Call Collect method when interaction key is pressed and looking at a lever
        if (currentLever != null && Input.GetKeyDown(interactionKey))
        {
            currentLever.Collect();
            currentLever.HideUIKeybind();
        }
    }

    // Optional: Clean up when disabled or destroyed
    void OnDisable()
    {
        if (currentLever != null)
        {
            currentLever.HideUIKeybind();
            currentLever = null;
        }
        wasLookingAtLever = false;
    }
}