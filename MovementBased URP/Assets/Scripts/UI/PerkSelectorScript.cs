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
        public int moneyMultiplierDelta = 0;
        public TextMeshProUGUI moneyMultiplierText;
    }

    [System.Serializable]
    public class ButtonHierarchy
    {
        public GameObject parentButton;
        public GameObject[] childButtons;
        public GameObject[] objectsToEnable; 
        [HideInInspector] public bool isActive = false;
    }

    [Header("Object Settings")]
    public SpawnableObject[] objectsArray; 
    public int amountToActivate = 2; 
    
    [Header("Special References")]
    public GameObject ButtonPerkNone; 
    [SerializeField] private bool spawnNonePerkButton = false;
    
    [Header("Button Hierarchy System")]
    public ButtonHierarchy[] buttonHierarchies; 
    
    private GameObject mainCamera;

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
    [Range(0, 100)] public int redEyeSpawnChance = 50;
    [SerializeField] private int redEyeTimeToAdd= 20;
    public bool spawnRedEye = false;

    [Header("Blue Eye Perk References")]
    [Range(0, 100)] public int blueEyeSpawnChance = 50;
    [SerializeField] private int blueEyeTimeToAdd= 20;
    public bool spawnBlueEye = false;

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

    [Header("Red Walls Perk References")]
    [Range(0, 100)][SerializeField] private int redWallsSpawnChances;
    [SerializeField] private int redWallTimeToAdd= 20;

    [Header("Pickup Nodes Perk References")]
    [SerializeField] private int nodePickupTimerToRemove = 10;

    [Header("Add Levels Perk References")]
    [SerializeField] private int levelsToAdd1;
    [SerializeField] private int levelsToAdd2;
    [SerializeField] private int levelsToAdd3;
    [SerializeField] private int timeToAdd1;
    [SerializeField] private int timeToAdd2;
    [SerializeField] private int timeToAdd3;

    [Header("Add Falling Platforms Perk References")]
    [Range(0, 100)][SerializeField] private int platformsSpawnChance = 25;

    [Header("Lighting Break References")]
    [Range(0, 100)] public int lightBreakChance = 30;

    [Header("Levers To Spawn References")]
    public int leversToSpawnDefault = 1;

    private GameObject targetProcuderalGeneration;
    private LevelGenerator levelGenerator;
    private GameObject cameraTarget;
    private RedEyeDetector redEyeDetector;
    private PlayerController playerController;
    private ItemPickup itemPickup;
    private GameObject timerTarget;
    private Timer timer;
    private GameObject grappleTarget;

    [Header("Debug Only")]
    [SerializeField] public bool isPerkMenuOpen;

    [Header("Other References")]
    [SerializeField] private Grappling grappling;
    [SerializeField] private MoneyManager moneyManager;

    void Start()
    {
        mainCamera = GameObject.Find("Main Camera");
        itemPickup = mainCamera != null ? mainCamera.GetComponent<ItemPickup>() : null;

        targetProcuderalGeneration = GameObject.Find("ProcuderalGeneration");
        levelGenerator = targetProcuderalGeneration.GetComponent<LevelGenerator>();

        cameraTarget = GameObject.Find("Main Camera");
        redEyeDetector = cameraTarget.GetComponent<RedEyeDetector>();
        itemPickup = cameraTarget.GetComponent<ItemPickup>();

        timerTarget = GameObject.Find("TimerManager");
        timer = timerTarget.GetComponent<Timer>();

        if (moneyManager == null)
        {
            moneyManager = MoneyManager.Instance != null ? MoneyManager.Instance : FindObjectOfType<MoneyManager>();
        }

        InitializeButtonHierarchy();
        InitializeMoneyMultiplierTexts();
    }

    private void InitializeButtonHierarchy()
    {
        foreach (ButtonHierarchy hierarchy in buttonHierarchies)
        {
            foreach (GameObject childButton in hierarchy.childButtons)
            {
                if (childButton != null)
                    childButton.SetActive(false);
            }
        }
    }

    private void InitializeMoneyMultiplierTexts()
    {
        foreach (SpawnableObject spawnable in objectsArray)
        {
            if (spawnable.moneyMultiplierText == null)
            {
                continue;
            }

            int delta = spawnable.moneyMultiplierDelta;
            spawnable.moneyMultiplierText.text = delta.ToString() + "%SC";

            if (delta < 0)
            {
                spawnable.moneyMultiplierText.color = Color.red;
            }
        }
    }

    public void OpenPerkMenu()
    {
        itemPickup.useInteraction = false;

        Cursor.lockState = CursorLockMode.None;
        if (playerController != null)
        {
            playerController.SetCameraMovementEnabled(false);
        }
        CheckAndEnableNonePerk();
        
        ActivateRandomObjects();
        isPerkMenuOpen = true;
    }
    
    private void CheckAndEnableNonePerk()
    {
        if (ButtonPerkNone == null)
        {
            return;
        }

        bool allDisabled = true;
        
        foreach (SpawnableObject spawnable in objectsArray)
        {
            if (spawnable.gameObject != null && spawnable.gameObject != ButtonPerkNone)
            {
                if (spawnable.spawnAtMenuOpen)
                {
                    allDisabled = false;
                    break;
                }
            }
        }
        
        if (allDisabled)
        {
            SetSpawnAtMenuOpenForObject(ButtonPerkNone, true);
        }
        else
        {
            SetSpawnAtMenuOpenForObject(ButtonPerkNone, false);
        }
        
        if (spawnNonePerkButton)
        {
            SetSpawnAtMenuOpenForObject(ButtonPerkNone, true);
        }
    }
    
    public void ClosePerkMenu()
    {
        itemPickup.useInteraction = true;

        if (playerController != null)
        {
            playerController.SetCameraMovementEnabled(true);
        }
        
        ResetAllButtonAnimations();
        
        animator.Play(animationClip.name);

        levelGenerator.RestartGeneration();
        redEyeDetector.FindRedEyeGameobjects();
        
        Invoke("ResetButtonHierarchy", 1f);
        isPerkMenuOpen = false;
    }
    
    private void ResetAllButtonAnimations()
    {
        foreach (ButtonHierarchy hierarchy in buttonHierarchies)
        {
            if (hierarchy.parentButton != null)
            {
                ResetSingleButtonAnimation(hierarchy.parentButton);
            }
            
            foreach (GameObject childButton in hierarchy.childButtons)
            {
                if (childButton != null)
                {
                    ResetSingleButtonAnimation(childButton);
                }
            }
        }
        
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
        Button button = buttonObject.GetComponent<Button>();

        if (button != null)
        {
            button.OnPointerExit(null);
            button.OnDeselect(null);
        }

        Animator anim = buttonObject.GetComponent<Animator>();

        if (anim != null)
        {
            anim.Rebind();
            anim.Update(0f);
            
            anim.Play("Normal", 0, 0f);
            anim.Update(0f);
            
            anim.SetBool("Highlighted", false);
            anim.SetBool("Pressed", false);
            anim.SetBool("Selected", false);
            anim.SetTrigger("Normal");
        }
    }

    public void ActivateRandomObjects()
    {
        foreach (SpawnableObject spawnable in objectsArray)
        {
            if (spawnable.gameObject != null && !spawnable.spawnAtMenuOpen)
                spawnable.gameObject.SetActive(false);
        }

        if (objectsArray.Length == 0)
        {
            return;
        }

        bool shouldForceNonePerk = spawnNonePerkButton && ButtonPerkNone != null;

        if (amountToActivate <= 0 && !shouldForceNonePerk)
        {
            return;
        }

        List<SpawnableObject> availableNonNoneObjects = new List<SpawnableObject>();
        SpawnableObject nonePerkSpawnable = null;

        foreach (SpawnableObject spawnable in objectsArray)
        {
            if (spawnable.gameObject == null || !spawnable.spawnAtMenuOpen)
            {
                continue;
            }

            if (shouldForceNonePerk && spawnable.gameObject == ButtonPerkNone)
            {
                nonePerkSpawnable = spawnable;
                continue;
            }

            availableNonNoneObjects.Add(spawnable);
        }

        if (availableNonNoneObjects.Count == 0 && nonePerkSpawnable == null)
        {
            return;
        }

        ShuffleList(availableNonNoneObjects);

        foreach (SpawnableObject spawnable in objectsArray)
        {
            if (spawnable.gameObject != null)
                spawnable.gameObject.SetActive(false);
        }

        int activatedCount = 0;

        if (nonePerkSpawnable != null)
        {
            nonePerkSpawnable.gameObject.SetActive(true);
            activatedCount++;
        }

        int remainingToActivate = Mathf.Max(0, amountToActivate - activatedCount);
        int actualAmountToActivate = Mathf.Min(remainingToActivate, availableNonNoneObjects.Count);

        for (int i = 0; i < actualAmountToActivate; i++)
        {
            availableNonNoneObjects[i].gameObject.SetActive(true);
        }
    }

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

    public void OnParentButtonClick(int hierarchyIndex)
    {
        if (hierarchyIndex >= 0 && hierarchyIndex < buttonHierarchies.Length)
        {
            ButtonHierarchy hierarchy = buttonHierarchies[hierarchyIndex];
            
            foreach (GameObject childButton in hierarchy.childButtons)
            {
                if (childButton != null)
                    childButton.SetActive(true);
            }
            
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
                
                EnableAllChildButtonsInObjectsArray();
                
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

    private void EnableAllChildButtonsInObjectsArray()
    {
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

        int enabledCount = 0;
        foreach (SpawnableObject spawnable in objectsArray)
        {
            if (spawnable.gameObject != null && allChildButtons.Contains(spawnable.gameObject))
            {
                spawnable.spawnAtMenuOpen = true;
                enabledCount++;
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

    public void DeactivateObjectByIndex(int objectIndex)
    {
        if (objectIndex >= 0 && objectIndex < objectsArray.Length)
        {
            SpawnableObject spawnable = objectsArray[objectIndex];
            if (spawnable.gameObject != null && spawnable.spawnAtMenuOpen)
            {
                if (spawnable.timesAmountToDeactivate > 0)
                {
                    spawnable.timesAmountToDeactivate--;
                    
                    if (spawnable.timesAmountToDeactivate == 0)
                    {
                        spawnable.spawnAtMenuOpen = false;
                    }
                }
                else
                {
                    spawnable.spawnAtMenuOpen = false;
                }
                
                spawnable.gameObject.SetActive(false);
            }
        }
    }

    public void DeactivateObjectByReference(GameObject targetObject)
    {
        foreach (SpawnableObject spawnable in objectsArray)
        {
            if (spawnable.gameObject == targetObject && spawnable.spawnAtMenuOpen)
            {
                if (spawnable.timesAmountToDeactivate > 0)
                {
                    spawnable.timesAmountToDeactivate--;
                    
                    if (spawnable.timesAmountToDeactivate == 0)
                    {
                        spawnable.spawnAtMenuOpen = false;
                    }
                }
                else
                {
                    spawnable.spawnAtMenuOpen = false;
                }
                
                spawnable.gameObject.SetActive(false);
                return;
            }
        }
    }

    public void OnSpawnablePerkButtonClicked(GameObject buttonObject)
    {
        if (buttonObject == null) return;

        if (moneyManager == null)
        {
            moneyManager = MoneyManager.Instance != null ? MoneyManager.Instance : FindObjectOfType<MoneyManager>();
        }

        if (moneyManager == null)
        {
            return;
        }

        foreach (SpawnableObject spawnable in objectsArray)
        {
            if (spawnable.gameObject == buttonObject)
            {
                if (spawnable.moneyMultiplierDelta != 0)
                {
                    float delta = spawnable.moneyMultiplierDelta / 100f;
                    float newMultiplier = moneyManager.moneyMultiplier + delta;
                    moneyManager.SetMoneyMultiplier(newMultiplier);
                }
                break;
            }
        }
    }

    private void ResetButtonHierarchy()
    {
        foreach (ButtonHierarchy hierarchy in buttonHierarchies)
        {
            foreach (GameObject childButton in hierarchy.childButtons)
            {
                if (childButton != null)
                    childButton.SetActive(false);
            }
            
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
        levelGenerator.grappleChance = grappleChance;
        levelGenerator.numberOfLevelsToAdd = levelGenerator.numberOfLevelsToAdd + 3;
    }

    public void ButtonSwingPerk()
    {
        swingGameobject.SetActive(true);
        levelGenerator.swingChance = swingChance;
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

    public void ButtonRedEyeSpawner()
    {
        levelGenerator.redEyeChance = levelGenerator.redEyeChance + redEyeSpawnChance;
        spawnRedEye = true;
        timer.totalSeconds = timer.totalSeconds + redEyeTimeToAdd;
    }

    public void ButtonRedWallsSpawner()
    {
        levelGenerator.redWallsChance = levelGenerator.redWallsChance + redWallsSpawnChances;
        timer.totalSeconds = timer.totalSeconds + redWallTimeToAdd;
    }

    public void ButtonPickupNodesPerk()
    {
        grappling.canPickupNode = true;
        timer.totalSeconds = timer.totalSeconds - nodePickupTimerToRemove;
    }

    public void ButtonBlueEyeSpawner()
    {
        spawnBlueEye = true;
        timer.totalSeconds = timer.totalSeconds + blueEyeTimeToAdd;
    }

    public void ButtonFallingPlatformsSpawner()
    {
        levelGenerator.platformsChance = platformsSpawnChance;
    }

    public void ButtonLightBreakerPerk()
    {
        lightBreakChance = 80;
    }

    public void ButtonLightFixerPerk()
    {
        lightBreakChance = 0;
    }

    public void ButtonLever1Perk()
    {
        leversToSpawnDefault++;
    }

    public void ButtonLever2Perk()
    {
        leversToSpawnDefault++;
    }
}