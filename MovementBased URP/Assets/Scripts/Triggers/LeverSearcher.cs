using UnityEngine;
using System.Collections.Generic;

public class LeverSearcher : MonoBehaviour
{
    [Header("Lever Settings")]
    public string leverTag = "Lever";
    
    [Header("Debug")]
    public bool showGizmos = true;
    public Color gizmoColor = Color.cyan;
    
    [Header("Door Reference")]
    public GameObject doorTarget;

    private List<GameObject> leverObjects = new List<GameObject>();
    private List<Lever> leverScripts = new List<Lever>();
    private List<bool> leverCollectedStates = new List<bool>();
    private bool playerInTrigger = false;
    private Collider triggerCollider;

    void Start()
    {
        // Get reference to existing collider
        triggerCollider = GetComponent<Collider>();

        // Search for Lever objects at the end of the frame
        Invoke("SearchForLevers", 0.1f); // Small delay to ensure all objects are loaded
    }

    void SearchForLevers()
    {
        if (triggerCollider == null) return;

        // Clear previous lists
        leverObjects.Clear();
        leverScripts.Clear();
        leverCollectedStates.Clear();

        // Search for Lever objects using the collider's bounds
        Collider[] collidersInRange = Physics.OverlapBox(
            triggerCollider.bounds.center,
            triggerCollider.bounds.extents,
            transform.rotation
        );

        foreach (Collider collider in collidersInRange)
        {
            if (collider.CompareTag(leverTag))
            {
                leverObjects.Add(collider.gameObject);
                
                Lever leverScript = collider.GetComponent<Lever>();
                if (leverScript != null)
                {
                    leverScripts.Add(leverScript);
                    
                    // Get the collected bool from the Lever script
                    bool collectedState = GetCollectedState(leverScript);
                    leverCollectedStates.Add(collectedState);
                    
                    Debug.Log("Found Lever: " + collider.gameObject.name + " - Collected: " + collectedState);
                }
                else
                {
                    Debug.LogWarning("Lever script not found on: " + collider.gameObject.name);
                }
            }
        }

        Debug.Log("Found " + leverScripts.Count + " Lever scripts in range");
        
        // Initial check of lever status
        CheckLeverStatus();
    }

    bool GetCollectedState(Lever leverScript)
    {
        // Use reflection to get the collected field/property
        System.Type leverType = leverScript.GetType();
        var collectedField = leverType.GetField("collected");
        var collectedProperty = leverType.GetProperty("collected");
        
        if (collectedField != null)
        {
            return (bool)collectedField.GetValue(leverScript);
        }
        else if (collectedProperty != null)
        {
            return (bool)collectedProperty.GetValue(leverScript);
        }
        else
        {
            Debug.LogWarning("No 'collected' field or property found in Lever script on " + leverScript.gameObject.name);
            return false;
        }
    }

    public void CheckLeverStatus()
    {
        if (leverScripts.Count == 0) return;

        bool allCollected = true;
        
        // Update collected states and check if all are true
        for (int i = 0; i < leverScripts.Count; i++)
        {
            if (leverScripts[i] != null)
            {
                bool currentCollected = GetCollectedState(leverScripts[i]);
                leverCollectedStates[i] = currentCollected;
                
                if (!currentCollected)
                {
                    allCollected = false;
                }
            }
        }

        if (allCollected)
        {
            Debug.Log("All lever bools are true");
            
            // Optional: Do something with the door target
            if (doorTarget != null)
            {
                // Example: Open the door
                // doorTarget.SetActive(false);
                // Or trigger an animation, etc.
                Debug.Log("Door should open now!");
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !playerInTrigger)
        {
            playerInTrigger = true;
            ShowAllUITips();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && playerInTrigger)
        {
            playerInTrigger = false;
            HideAllUITips();
        }
    }

    void ShowAllUITips()
    {
        foreach (Lever lever in leverScripts)
        {
            if (lever != null)
            {
                lever.ShowUITip();
            }
        }
        Debug.Log("ShowAllUITips called for " + leverScripts.Count + " levers");
    }

    void HideAllUITips()
    {
        foreach (Lever lever in leverScripts)
        {
            if (lever != null)
            {
                lever.HideUITip();
            }
        }
        Debug.Log("HideAllUITips called for " + leverScripts.Count + " levers");
    }
}