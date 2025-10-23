using UnityEngine;
using System.Collections.Generic;

public class LeverSearcher : MonoBehaviour
{
    [Header("Lever Settings")]
    public string leverTag = "Lever";
    
    [Header("Debug")]
    public bool showGizmos = true;
    public Color gizmoColor = Color.cyan;

    private List<GameObject> leverObjects = new List<GameObject>();
    private List<Lever> leverScripts = new List<Lever>();
    private bool playerInTrigger = false;
    private Collider triggerCollider;

    void Start()
    {
        // Get reference to existing collider
        triggerCollider = GetComponent<Collider>();

        // Search for Lever objects
        SearchForLevers();
    }

    void SearchForLevers()
    {
        if (triggerCollider == null) return;

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
                Lever leverScript = collider.GetComponent<Lever>();
                if (leverScript != null)
                {
                    leverScripts.Add(leverScript);
                    Debug.Log("Found Lever: " + collider.gameObject.name);
                }
                else
                {
                    Debug.LogWarning("Lever script not found on: " + collider.gameObject.name);
                }
            }
        }

        Debug.Log("Found " + leverScripts.Count + " Lever scripts in range");
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