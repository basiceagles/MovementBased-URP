using Unity.VisualScripting;
using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    public Transform holdPoint;
    public float pickupRange = 3f;
    public LayerMask interactableLayer;
    public LayerMask grinderLayer;
    public LayerMask leverLayer;
    public LayerMask grappleableLayer;
    public LayerMask npcLayer;
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
    private bool isLookingAtGrappleable;
    private bool isLookingAtNPC;
    private bool wasLookingAtNPC = false;
    private bool wasLookingAtGrappleable = false;

    private GameObject swing;
    private GameObject grapple;
    private GameObject holdPointBuffer; 
    private Grappling grapplingScript;
    private Swinging swingingScript;
    private float maxGrappleRange; 

    private Lever currentLever;
    private Lever previousLever;
    private bool wasLookingAtLever = false;
    private bool isCollected;

    private DialogueStarterByName currentDialogueStarterByName;
    private DialogueStarterByName previousDialogueStarterByName;

    public bool disableGrappleAndSwing = false;
    
    [SerializeField] private CrosshairSwitcher crosshairSwitcher;

    public bool useInteraction = true;

    void Start()
    {
        UpdateReferences();
    }

    private void UpdateReferences()
    {
        swing = GameObject.Find("Swing");
        grapple = GameObject.Find("Grapple");
        playerCamera = GetComponent<Camera>();
        
        holdPointBuffer = GameObject.Find("holdPointBuffer");
        
        grapplingScript = FindObjectOfType<Grappling>();
        swingingScript = FindObjectOfType<Swinging>();
        
        if (grapplingScript != null && swingingScript != null)
        {
            maxGrappleRange = Mathf.Max(grapplingScript.maxGrappleDistance, swingingScript.maxSwingDistance);
        }
    }

    private void OnEnable()
    {
        LevelGenerator.RestartGenerationEvent += UpdateReferences;
    }
    private void OnDisable()
    {
        LevelGenerator.RestartGenerationEvent -= UpdateReferences;
    }

    public void SetInteractionEnabled(bool value)
    {
        if (value)
        {
            useInteraction = true;
        }
        else
        {
            useInteraction = false;
        }
    }

    private void Update()
    {
        if (!useInteraction) return;

        HandleRaycast();
        
        if (Input.GetKeyDown(interactKey))
        {
            if (isLookingAtNPC)
            {
                if (currentDialogueStarterByName != null) currentDialogueStarterByName.StartDialogue();
                if (currentDialogueStarterByName != null) currentDialogueStarterByName.HideUITip();
            }

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
            heldItem.transform.position = holdPoint.position;
            heldItem.transform.rotation = holdPoint.rotation;
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

        if (Physics.Raycast(ray, out hit, pickupRange, leverLayer))
        {
            hitLever = hit.collider.GetComponent<Lever>();
            if (hitLever != null)
            {
                currentLever = hitLever;
                isLookingAtLever = true;
            }
        }
        
        isLookingAtNPC = false;
        DialogueStarterByName hitDialogueStarter = null;

        if (Physics.Raycast(ray, out hit, pickupRange, npcLayer))
        {
            hitDialogueStarter = hit.collider.GetComponentInParent<DialogueStarterByName>();

            if (hitDialogueStarter != null)
            {
                currentDialogueStarterByName = hitDialogueStarter;
                isLookingAtNPC = true;
            }
        }

        isLookingAtGrappleable = false;
        if (maxGrappleRange > 0 && Physics.Raycast(ray, maxGrappleRange, grappleableLayer))
        {
            isLookingAtGrappleable = true;
        }

        if (isLookingAtGrinder && currentGrinder != null && !wasLookingAtGrinder && isHolding)
        {
            currentGrinder.ShowUITip();
            wasLookingAtGrinder = true;
            crosshairSwitcher.ShowSwitchCrosshair();
        }

        if (!isLookingAtGrinder && wasLookingAtGrinder && previousGrinder != null)
        {
            previousGrinder.HideUITip();
            wasLookingAtGrinder = false;
            crosshairSwitcher.HideSwitchCrosshair();
        }

        if (!isLookingAtGrinder)
        {
            previousGrinder = currentGrinder;
            currentGrinder = null;
        }
        else
        {
            previousGrinder = currentGrinder;
        }

        if (isLookingAtLever && currentLever != null && !wasLookingAtLever && !isCollected)
        {
            currentLever.ShowUIKeybind();
            wasLookingAtLever = true;
            crosshairSwitcher.ShowSwitchCrosshair();
        }

        if (!isLookingAtLever && wasLookingAtLever && previousLever != null)
        {
            previousLever.HideUIKeybind();
            wasLookingAtLever = false;
            crosshairSwitcher.HideSwitchCrosshair();
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
  
        if (!isLookingAtNPC)
        {
            previousDialogueStarterByName = currentDialogueStarterByName;
            currentDialogueStarterByName = null;
        }
        else
        {
            previousDialogueStarterByName = currentDialogueStarterByName;
        }

        if (isLookingAtNPC != wasLookingAtNPC)
        {
            if (isLookingAtNPC)
            {
                crosshairSwitcher.ShowSwitchCrosshair();

                if (currentDialogueStarterByName != null)
                {
                    currentDialogueStarterByName.ShowUITip();
                }
            }
            else
            {
                crosshairSwitcher.HideSwitchCrosshair();

                if (previousDialogueStarterByName != null)
                {
                    previousDialogueStarterByName.HideUITip();
                }
            }
            
            wasLookingAtNPC = isLookingAtNPC;
        }

        if (isLookingAtGrappleable != wasLookingAtGrappleable)
        {
            if (isLookingAtGrappleable)
            {
                crosshairSwitcher.ShowSwitchCrosshair();
            }
            else
            {
                crosshairSwitcher.HideSwitchCrosshair();
            }
            wasLookingAtGrappleable = isLookingAtGrappleable;
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
    
    public void ForcePickup(GameObject item)
    {
        if (item == null)
        {
            return;
        }

        if (isHolding)
        {
            return;
        }

        Pickup(item);
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
        
        heldItem.transform.SetParent(holdPointBuffer.transform);
        
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

        heldItem.transform.SetParent(null);

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

    private void InsertNode()
    {
        currentGrinder.GrinderInsertNode();

        if (heldItem != null)
        {
            heldItem.transform.SetParent(null);
        }
        
        Destroy(heldItem);
        isHolding = false;
        itemCollider = null;
    }
}