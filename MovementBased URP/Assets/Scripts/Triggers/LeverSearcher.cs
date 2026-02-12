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
    public GameObject doorCloseTarget;
    private SlideMetalDoorManager slideMetalDoorManager;
    private SlideMetalDoorManager slideMetalDoorManagerCloseState;

    [SerializeField] private List<GameObject> leverObjects = new List<GameObject>();
    [SerializeField] private List<Lever> leverScripts = new List<Lever>();
    [SerializeField] private List<bool> leverCollectedStates = new List<bool>();
    private bool playerInTrigger = false;
    private Collider triggerCollider;

    void Start()
    {
        triggerCollider = GetComponent<Collider>();

        slideMetalDoorManager = doorTarget.GetComponent<SlideMetalDoorManager>();

        if (doorCloseTarget != null)
        {
            slideMetalDoorManagerCloseState = doorCloseTarget.GetComponent<SlideMetalDoorManager>();
        }
        
        SearchForLevers();
    }

    void SearchForLevers()
    {
        if (triggerCollider == null) return;

        leverObjects.Clear();
        leverScripts.Clear();
        leverCollectedStates.Clear();

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

                    bool collectedState = GetCollectedState(leverScript);
                    leverCollectedStates.Add(collectedState);
                }
            }
        }

        CheckLeverStatus();
    }

    bool GetCollectedState(Lever leverScript)
    {
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
            return false;
        }
    }

    public void CheckLeverStatus()
    {
        if (leverScripts.Count == 0) SearchForLevers();

        bool allCollected = true;

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
            if (doorTarget != null)
            {
                slideMetalDoorManager.OpenSlideDoor();
            }
            if (doorCloseTarget !=null)
            {
                slideMetalDoorManagerCloseState.CloseDoor();
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
    }
}