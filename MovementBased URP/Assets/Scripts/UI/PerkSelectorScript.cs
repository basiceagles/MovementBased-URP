using UnityEngine;
using TMPro;
using System.Collections.Generic;

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

    private GameObject targetProcuderalGeneration;
    private LevelGenerator levelGenerator;
    private GameObject cameraTarget;
    private RedEyeDetector redEyeDetector;
    private PlayerController playerController;
    private ItemPickup itemPickup;


    void Start()
    {
        mainCamera = GameObject.Find("Main Camera");
        cameraController = mainCamera.GetComponent<CameraController>();

        targetProcuderalGeneration = GameObject.Find("ProcuderalGeneration");
        levelGenerator = targetProcuderalGeneration.GetComponent<LevelGenerator>();

        cameraTarget = GameObject.Find("Main Camera");
        redEyeDetector = cameraTarget.GetComponent<RedEyeDetector>();
        itemPickup = cameraTarget.GetComponent<ItemPickup>();


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
        itemPickup.enabled = false;
    }
    
    public void ClosePerkMenu()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        cameraController.enabled = true;
        animator.Play(animationClip.name);
        itemPickup.enabled = true;

        levelGenerator.RestartGeneration();
        redEyeDetector.FindRedEyeGameobjects();

        UpdateDeactivationCounts();
        
        // Reset button hierarchy when closing menu
        ResetButtonHierarchy();
    }
    
    // Call this method to activate random objects
    public void ActivateRandomObjects()
    {
        // First, deactivate objects that have spawnAtMenuOpen set to false
        foreach (SpawnableObject spawnable in objectsArray)
        {
            if (spawnable.gameObject != null && !spawnable.spawnAtMenuOpen)
                spawnable.gameObject.SetActive(false);
        }
        
        // Check if we have enough objects in the array
        if (objectsArray.Length == 0)
        {
            return;
        }
        
        // Check if amount to activate is 0 or less - don't spawn anything
        if (amountToActivate <= 0)
        {
            return;
        }
        
        if (amountToActivate > objectsArray.Length)
        {
            amountToActivate = objectsArray.Length;
        }
        
        // Activate random objects
        for (int i = 0; i < amountToActivate; i++)
        {
            GameObject randomObject = GetRandomInactiveObject();
            if (randomObject != null)
            {
                randomObject.SetActive(true);
            }
        }
    }
    
    private GameObject GetRandomInactiveObject()
    {
        // Create a list of inactive objects
        System.Collections.Generic.List<GameObject> inactiveObjects = new System.Collections.Generic.List<GameObject>();
        
        foreach (SpawnableObject spawnable in objectsArray)
        {
            if (spawnable.gameObject != null && !spawnable.gameObject.activeInHierarchy && spawnable.spawnAtMenuOpen)
            {
                inactiveObjects.Add(spawnable.gameObject);
            }
        }
        
        // If no inactive objects found, return null
        if (inactiveObjects.Count == 0)
            return null;
        
        // Return a random inactive object
        int randomIndex = Random.Range(0, inactiveObjects.Count);
        return inactiveObjects[randomIndex];
    }

    private void UpdateDeactivationCounts()
    {
        foreach (SpawnableObject spawnable in objectsArray)
        {
            if (spawnable.gameObject != null && spawnable.gameObject.activeInHierarchy && spawnable.spawnAtMenuOpen)
            {
                spawnable.currentDeactivationCount++;
                
                // Check if we've reached the deactivation limit
                if (spawnable.timesAmountToDeactivate > 0 && 
                    spawnable.currentDeactivationCount >= spawnable.timesAmountToDeactivate)
                {
                    spawnable.spawnAtMenuOpen = false;
                }
            }
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
            
            // Set spawnAtMenuOpen to true for specific objects assigned to this parent button
            foreach (GameObject objectToEnable in hierarchy.objectsToEnable)
            {
                SetSpawnAtMenuOpenForObject(objectToEnable, true);
            }
            
            hierarchy.isActive = true;
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
                
                // You can add specific logic here for when a specific child button is clicked
                Debug.Log($"Child button {childIndex} clicked in hierarchy {hierarchyIndex}");
                
                // Example: Set specific objects based on which child button was clicked
                // You can expand this logic based on your needs
                
                // Deactivate parent and all child buttons after selection
                if (hierarchy.parentButton != null)
                    hierarchy.parentButton.SetActive(false);
                    
                foreach (GameObject childButton in hierarchy.childButtons)
                {
                    if (childButton != null)
                        childButton.SetActive(false);
                }
            }
        }
    }

    private void SetSpawnAtMenuOpenForObject(GameObject targetObject, bool value)
    {
        foreach (SpawnableObject spawnable in objectsArray)
        {
            if (spawnable.gameObject == targetObject)
            {
                spawnable.spawnAtMenuOpen = value;
                break;
            }
        }
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
    }
    public void ButtonGrapplePerk()
    {
        grappleGameobject.SetActive(true);
        levelGenerator.swingChance = swingChance;
    }
    public void ButtonSwingPerk()
    {
        swingGameobject.SetActive(true);
        levelGenerator.grappleChance = grappleChance;
    }
}