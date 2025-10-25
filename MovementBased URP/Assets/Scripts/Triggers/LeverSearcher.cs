using UnityEngine;
using System.Collections.Generic;
//Данный скрипт ищет все рычаги в своем триггере
//Также методом GetCollectedState проверяет, активированы ли все рычаги, если да, то открывает дверь
//Также данный скрипт использует свой коллайд, чтобы проверять, зашел ли игрок в или нет, чтобы активировать UI на рычагах
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

    private List<GameObject> leverObjects = new List<GameObject>();
    private List<Lever> leverScripts = new List<Lever>();
    private List<bool> leverCollectedStates = new List<bool>();
    private bool playerInTrigger = false;
    private Collider triggerCollider;

    void Start()
    {
        // Get reference to existing collider
        triggerCollider = GetComponent<Collider>();

        //Ищем скрипт который задает анимации металлической двери
        slideMetalDoorManager = doorTarget.GetComponent<SlideMetalDoorManager>();

        if (doorCloseTarget != null)
        {
            slideMetalDoorManagerCloseState = doorCloseTarget.GetComponent<SlideMetalDoorManager>();
        }
        
        // Инвок чтобы не глючило ебат
        Invoke("SearchForLevers", 0.1f); // Small delay to ensure all objects are loaded
        
    }

    //Ищем рычаги и суем их в массив
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

    //Тут мы ищем переменную collected внутри скрипта Lever
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

    //Этот метод вызвается только тогда, когда активируется кнопка
    //Этот метод проверяет состояние всех булевых во всех кнопках
    //Если булевая collected во всех кнопках true (значит что все кнопки активированы), то метод активирует дверь
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
            // Optional: Do something with the door target
            if (doorTarget != null)
            {
                //Открываем дверку вызовом метода
                slideMetalDoorManager.OpenSlideDoor();
                Debug.Log("Door should open now!");
            }
            if (doorCloseTarget !=null)
            {
                slideMetalDoorManagerCloseState.CloseDoor();
                Debug.LogWarning("DOOR CLOSED NIGGA22");
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