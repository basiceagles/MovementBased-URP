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

        if (Physics.Raycast(ray, out hit, interactionRange, interactableLayer))
        {
            hitLever = hit.collider.GetComponent<Lever>();
            if (hitLever != null)
            {
                currentLever = hitLever;
                isLookingAtLever = true;
            }
        }

        if (isLookingAtLever && currentLever != null && !wasLookingAtLever && !isCollected)
        {
            currentLever.ShowUIKeybind();
            wasLookingAtLever = true;
        }

        if (!isLookingAtLever && wasLookingAtLever && previousLever != null)
        {
            previousLever.HideUIKeybind();
            wasLookingAtLever = false;
        }

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
        if (currentLever != null && Input.GetKeyDown(interactionKey))
        {
            currentLever.Collect();
            currentLever.HideUIKeybind();
        }
    }

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