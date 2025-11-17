using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;

public class PerkSelectorScript : MonoBehaviour
{
    [System.Serializable]
    public class SpawnableObject
    {
        public GameObject gameObject;
        public bool spawnAtMenuOpen = true;
        public int timesAmountToDeactivate = 0;
        [HideInInspector] public int currentDeactivationCount = 0;
    }

    [System.Serializable]
    public class ButtonHierarchy
    {
        public GameObject parentButton;
        public GameObject[] childButtons;
        public GameObject[] objectsToEnable; // Specific objects from objectsArray to enable when parent is clicked
        [HideInInspector] public bool isActive = false;
    }

    [Header("Object Settings")]
    public SpawnableObject[] objectsArray; // Assign your GameObjects here in inspector
    public int amountToActivate = 2; // Adjustable amount of objects to activate
    
    [Header("Button Hierarchy System")]
    public ButtonHierarchy[] buttonHierarchies; // Define button parent-child relationships
    
    private GameObject mainCamera;
    private CameraController cameraController;
    [Header("Prop Selector Animator")]
    [SerializeField] private AnimationClip animationClip;
    [SerializeField] private Animator animator;
    [Header("Node Perk References")]
    [Range(0, 100)] public float nodeSpawnChance = 25f;
    [Range(0, 100)] public float grinderSpawnChance = 75f;
    [Range(0, 100)] public int nodeRoomSpawnChance = 15;
    public bool spawnGrinder = false;
    public bool spawnNodes = false;
    [Header("Red Eye Perk References")]
    [Range(0, 100)] public float redEyeSpawnChance = 50f;
    public bool spawnRedEye = false;
    [Header("Prop Spawn References")]
    [Range(0, 100)] public float propSpawnChance = 50f;
    public bool spawnProps = false;
    [Header("Swing Levels Perk References")]
    public bool spawnSwingLevels = false;
    public bool useSwing = false;
    [SerializeField] private GameObject swingGameobject;
    [Range(0, 100)] [SerializeField] private int swingChance = 25;
    [Header("Grapple Levels Perk References")]
    public bool spawnGrappleLevels = false;
    public bool useGrapple = false;
    [SerializeField] private GameObject grappleGameobject;
    [Range(0, 100)] [SerializeField] private int grappleChance = 25;
    [Header("Add Levels Perk References")]
    [SerializeField] private int levelsToAdd1;
    [SerializeField] private int levelsToAdd2;
    [SerializeField] private int levelsToAdd3;
    [SerializeField] private int timeToAdd1;
    [SerializeField] private int timeToAdd2;
    [SerializeField] private int timeToAdd3;

    private GameObject targetProcuderalGeneration;
    private LevelGenerator levelGenerator;
    private GameObject cameraTarget;
    private RedEyeDetector redEyeDetector;
    private PlayerController playerController;
    private ItemPickup itemPickup;
    private GameObject timerTarget;
    private Timer timer;
    [Header("Debug Only")]
    [SerializeField] public bool isPerkMenuOpen;

    void Start()
    {
        mainCamera = GameObject.Find("Main Camera");
        cameraController = mainCamera.GetComponent<CameraController>();

        targetProcuderalGeneration = GameObject.Find("ProcuderalGeneration");
        levelGenerator = targetProcuderalGeneration.GetComponent<LevelGenerator>();

        cameraTarget = GameObject.Find("Main Camera");
        redEyeDetector = cameraTarget.GetComponent<RedEyeDetector>();
        itemPickup = cameraTarget.GetComponent<ItemPickup>();

        timerTarget = GameObject.Find("TimerManager");
        timer = timerTarget.GetComponent<Timer>();


        InitializeButtonHierarchy();
    }

    private void InitializeButtonHierarchy()
    {
        // Deactivate all child buttons at start
        foreach (ButtonHierarchy hierarchy in buttonHierarchies)
        {
            foreach (GameObject childButton in hierarchy.childButtons)
            {
                if (childButton != null)
                    childButton.SetActive(false);
            }
        }
    }



    public void OpenPerkMenu()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        cameraController.enabled = false;
        ActivateRandomObjects();
        //itemPickup.enabled = false; DELETE
        isPerkMenuOpen = true;
    }
    
    public void ClosePerkMenu()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        cameraController.enabled = true;
        
        // Reset all button animations before playing the close animation
        ResetAllButtonAnimations();
        
        animator.Play(animationClip.name);
        //itemPickup.enabled = true; DELETE

        levelGenerator.RestartGeneration();
        redEyeDetector.FindRedEyeGameobjects();
        
        // Reset button hierarchy when closing menu
        Invoke("ResetButtonHierarchy", 1f);
        isPerkMenuOpen = false;
    }
    
    private void ResetAllButtonAnimations()
    {
        // Reset all buttons in the hierarchy to normal state
        foreach (ButtonHierarchy hierarchy in buttonHierarchies)
        {
            // Reset parent button
            if (hierarchy.parentButton != null)
            {
                ResetSingleButtonAnimation(hierarchy.parentButton);
            }
            
            // Reset all child buttons
            foreach (GameObject childButton in hierarchy.childButtons)
            {
                if (childButton != null)
                {
                    ResetSingleButtonAnimation(childButton);
                }
            }
        }
        
        // Also reset any active perk buttons from objectsArray
        foreach (SpawnableObject spawnable in objectsArray)
        {
            if (spawnable.gameObject != null && spawnable.gameObject.activeInHierarchy)
            {
                ResetSingleButtonAnimation(spawnable.gameObject);
            }
        }
    }

    private void ResetSingleButtonAnimation(GameObject buttonObject)
    {
        // Reset Button component state first
        Button button = buttonObject.GetComponent<Button>();
        if (button != null)
        {
            // Force button to exit highlighted/pressed states
            button.OnPointerExit(null);
            button.OnDeselect(null);
        }

        // Reset Animator state
        Animator anim = buttonObject.GetComponent<Animator>();
        if (anim != null)
        {
            // Method 1: Rebind completely to reset to initial state
            anim.Rebind();
            anim.Update(0f);
            
            // Method 2: Force play the normal state
            anim.Play("Normal", 0, 0f);
            anim.Update(0f);
            
            // Method 3: Reset common animator parameters
            anim.SetBool("Highlighted", false);
            anim.SetBool("Pressed", false);
            anim.SetBool("Selected", false);
            anim.SetTrigger("Normal");
        }
    }
    
    // Call this method to activate random objects
    public void ActivateRandomObjects()
    {
        // First, deactivate all objects that have spawnAtMenuOpen set to false
        foreach (SpawnableObject spawnable in objectsArray)
        {
            if (spawnable.gameObject != null && !spawnable.spawnAtMenuOpen)
                spawnable.gameObject.SetActive(false);
        }
        
        // Check if we have enough objects in the array
        if (objectsArray.Length == 0)
        {
            Debug.LogWarning("No objects in objectsArray!");
            return;
        }
        
        // Check if amount to activate is 0 or less - don't spawn anything
        if (amountToActivate <= 0)
        {
            Debug.LogWarning("amountToActivate is 0 or less, no objects will be activated.");
            return;
        }
        
        // Create a list of available objects that can be spawned (spawnAtMenuOpen = true)
        List<SpawnableObject> availableObjects = new List<SpawnableObject>();
        foreach (SpawnableObject spawnable in objectsArray)
        {
            if (spawnable.gameObject != null && spawnable.spawnAtMenuOpen)
            {
                availableObjects.Add(spawnable);
            }
        }
        
        // If no available objects, return
        if (availableObjects.Count == 0)
        {
            Debug.LogWarning("No available objects with spawnAtMenuOpen = true!");
            return;
        }
        
        // Ensure we don't try to activate more objects than available
        int actualAmountToActivate = Mathf.Min(amountToActivate, availableObjects.Count);
        
        // Shuffle the available objects using Fisher-Yates algorithm
        ShuffleList(availableObjects);
        
        // Deactivate all objects first
        foreach (SpawnableObject spawnable in objectsArray)
        {
            if (spawnable.gameObject != null)
                spawnable.gameObject.SetActive(false);
        }
        
        // Activate the first 'actualAmountToActivate' objects from the shuffled list
        for (int i = 0; i < actualAmountToActivate; i++)
        {
            if (i < availableObjects.Count)
            {
                availableObjects[i].gameObject.SetActive(true);
                Debug.Log($"Activated: {availableObjects[i].gameObject.name}");
            }
        }
        
        Debug.Log($"Activated {actualAmountToActivate} out of {availableObjects.Count} available objects");
    }

    // Proper Fisher-Yates shuffle implementation
    private void ShuffleList<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            T temp = list[i];
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }


    // Button hierarchy methods
    public void OnParentButtonClick(int hierarchyIndex)
    {
        if (hierarchyIndex >= 0 && hierarchyIndex < buttonHierarchies.Length)
        {
            ButtonHierarchy hierarchy = buttonHierarchies[hierarchyIndex];
            
            // Activate child buttons for this hierarchy
            foreach (GameObject childButton in hierarchy.childButtons)
            {
                if (childButton != null)
                    childButton.SetActive(true);
            }
            
            // Set spawnAtMenuOpen to true for all objects assigned to this parent button
            foreach (GameObject objectToEnable in hierarchy.objectsToEnable)
            {
                SetSpawnAtMenuOpenForObject(objectToEnable, true);
            }
            
            hierarchy.isActive = true;
            
            Debug.Log($"Parent button clicked in hierarchy {hierarchyIndex}. Activated {hierarchy.childButtons.Length} child buttons and enabled {hierarchy.objectsToEnable.Length} objects.");
        }
        else
        {
            Debug.LogWarning($"Invalid hierarchy index: {hierarchyIndex}");
        }
    }

    public void OnChildButtonClick(int childIndex, int hierarchyIndex)
    {
        if (hierarchyIndex >= 0 && hierarchyIndex < buttonHierarchies.Length)
        {
            ButtonHierarchy hierarchy = buttonHierarchies[hierarchyIndex];
            
            if (childIndex >= 0 && childIndex < hierarchy.childButtons.Length)
            {
                GameObject clickedChildButton = hierarchy.childButtons[childIndex];
                
                // FIXED: Enable spawnAtMenuOpen for ALL child buttons in objectsArray when any child button is clicked
                EnableAllChildButtonsInObjectsArray();
                
                Debug.Log($"Child button {childIndex} clicked in hierarchy {hierarchyIndex}. Enabled spawnAtMenuOpen for ALL child buttons in objectsArray.");
                
                // Deactivate parent and all child buttons after selection
                if (hierarchy.parentButton != null)
                    hierarchy.parentButton.SetActive(false);
                    
                foreach (GameObject childButton in hierarchy.childButtons)
                {
                    if (childButton != null)
                        childButton.SetActive(false);
                }
                
                hierarchy.isActive = false;
            }
        }
    }

    // NEW METHOD: Enable spawnAtMenuOpen for ALL objects in objectsArray that are child buttons in ANY hierarchy
    private void EnableAllChildButtonsInObjectsArray()
    {
        // Create a set of all child buttons from all hierarchies
        HashSet<GameObject> allChildButtons = new HashSet<GameObject>();
        foreach (ButtonHierarchy hierarchy in buttonHierarchies)
        {
            foreach (GameObject childButton in hierarchy.childButtons)
            {
                if (childButton != null)
                {
                    allChildButtons.Add(childButton);
                }
            }
        }

        // Enable spawnAtMenuOpen for all objects in objectsArray that are child buttons
        int enabledCount = 0;
        foreach (SpawnableObject spawnable in objectsArray)
        {
            if (spawnable.gameObject != null && allChildButtons.Contains(spawnable.gameObject))
            {
                spawnable.spawnAtMenuOpen = true;
                enabledCount++;
                Debug.Log($"Enabled spawnAtMenuOpen for child button: {spawnable.gameObject.name}");
            }
        }
        
        Debug.Log($"Enabled spawnAtMenuOpen for {enabledCount} child buttons in objectsArray");
    }

    private void SetSpawnAtMenuOpenForObject(GameObject targetObject, bool value)
    {
        foreach (SpawnableObject spawnable in objectsArray)
        {
            if (spawnable.gameObject == targetObject)
            {
                spawnable.spawnAtMenuOpen = value;
                Debug.Log($"Set spawnAtMenuOpen to {value} for object: {targetObject.name}");
                break;
            }
        }
    }

   // New method to deactivate specific GameObject by index with timesAmountToDeactivate logic
    public void DeactivateObjectByIndex(int objectIndex)
    {
        if (objectIndex >= 0 && objectIndex < objectsArray.Length)
        {
            SpawnableObject spawnable = objectsArray[objectIndex];
            if (spawnable.gameObject != null && spawnable.spawnAtMenuOpen)
            {
                // Decrease timesAmountToDeactivate by 1
                if (spawnable.timesAmountToDeactivate > 0)
                {
                    spawnable.timesAmountToDeactivate--;
                    
                    // If this was the last deactivation (reached 0), set spawnAtMenuOpen to false
                    if (spawnable.timesAmountToDeactivate == 0)
                    {
                        spawnable.spawnAtMenuOpen = false;
                        Debug.Log($"Fully deactivated object at index {objectIndex}: {spawnable.gameObject.name}. No more spawns allowed.");
                    }
                    else
                    {
                        Debug.Log($"Decreased deactivation count for object at index {objectIndex}: {spawnable.gameObject.name}. Remaining deactivations: {spawnable.timesAmountToDeactivate}");
                    }
                }
                else
                {
                    // If timesAmountToDeactivate is already 0, set spawnAtMenuOpen to false
                    spawnable.spawnAtMenuOpen = false;
                    Debug.Log($"Fully deactivated object at index {objectIndex}: {spawnable.gameObject.name}. No more spawns allowed.");
                }
                
                // Always deactivate the GameObject immediately
                spawnable.gameObject.SetActive(false);
            }
        }
        else
        {
            Debug.LogWarning($"Invalid object index: {objectIndex}");
        }
    }

    // Alternative method to deactivate by GameObject reference
    public void DeactivateObjectByReference(GameObject targetObject)
    {
        foreach (SpawnableObject spawnable in objectsArray)
        {
            if (spawnable.gameObject == targetObject && spawnable.spawnAtMenuOpen)
            {
                // Decrease timesAmountToDeactivate by 1
                if (spawnable.timesAmountToDeactivate > 0)
                {
                    spawnable.timesAmountToDeactivate--;
                    
                    // If this was the last deactivation (reached 0), set spawnAtMenuOpen to false
                    if (spawnable.timesAmountToDeactivate == 0)
                    {
                        spawnable.spawnAtMenuOpen = false;
                        Debug.Log($"Fully deactivated object: {targetObject.name}. No more spawns allowed.");
                    }
                    else
                    {
                        Debug.Log($"Decreased deactivation count for object: {targetObject.name}. Remaining deactivations: {spawnable.timesAmountToDeactivate}");
                    }
                }
                else
                {
                    // If timesAmountToDeactivate is already 0, set spawnAtMenuOpen to false
                    spawnable.spawnAtMenuOpen = false;
                    Debug.Log($"Fully deactivated object: {targetObject.name}. No more spawns allowed.");
                }
                
                // Always deactivate the GameObject immediately
                spawnable.gameObject.SetActive(false);
                return;
            }
        }
        Debug.LogWarning($"Object not found in objectsArray or already deactivated: {targetObject.name}");
    }

    private void ResetButtonHierarchy()
    {
        foreach (ButtonHierarchy hierarchy in buttonHierarchies)
        {
            // Deactivate all child buttons
            foreach (GameObject childButton in hierarchy.childButtons)
            {
                if (childButton != null)
                    childButton.SetActive(false);
            }
            
            // Reactivate parent button
            if (hierarchy.parentButton != null)
                hierarchy.parentButton.SetActive(true);
            
            hierarchy.isActive = false;
        }
    }

    public void ButtonNodeHandlerPerk()
    {
        spawnGrinder = true;
        spawnNodes = true;
        levelGenerator.numberOfLevelsToAdd = levelGenerator.numberOfLevelsToAdd + 2;
        levelGenerator.nodeExchangerChance = levelGenerator.nodeExchangerChance + nodeRoomSpawnChance;
    }
    public void ButtonGrapplePerk()
    {
        grappleGameobject.SetActive(true);
        levelGenerator.swingChance = swingChance;
        levelGenerator.numberOfLevelsToAdd = levelGenerator.numberOfLevelsToAdd + 3;
    }
    public void ButtonSwingPerk()
    {
        swingGameobject.SetActive(true);
        levelGenerator.grappleChance = grappleChance;
    }
    public void ButtonAdditionalPerksPerk()
    {
        amountToActivate++;
        levelGenerator.numberOfLevelsToAdd++;
    }
    public void ButtonAdditionalLevels1Perk()
    {
        levelGenerator.numberOfLevels = levelGenerator.numberOfLevels + levelsToAdd1;
        timer.totalSeconds = timer.totalSeconds + timeToAdd1;
    }
    public void ButtonAdditionalLevels2Perk()
    {
        levelGenerator.numberOfLevels = levelGenerator.numberOfLevels + levelsToAdd2;
        timer.totalSeconds = timer.totalSeconds + timeToAdd2;
    }
    public void ButtonAdditionalLevels3Perk()
    {
        levelGenerator.numberOfLevels = levelGenerator.numberOfLevels + levelsToAdd2;
        timer.totalSeconds = timer.totalSeconds + timeToAdd3;
    }
}