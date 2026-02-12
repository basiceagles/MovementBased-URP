using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class LevelGenerator : MonoBehaviour
{
    public enum LevelType
    {
        Standard,
        Swing,
        Grapple,
        Water,
        DoubleJump,
        LowGravity,
        Planet,
        Holes,
        RedEye,
        RedWalls,
        Platforms,
        NodeExchanger
    }

    public enum DifficultyType
    {
        Easy,
        Medium,
        Hard,
        Closed,
        ClosedMedium,
        ClosedHard
    }

    [System.Serializable]
    public class LevelData
    {
        public GameObject levelPrefab;
        public LevelType levelType;
    }

    [Header("Level Prefabs")]
    public GameObject startRoomPrefab; 
    public GameObject endRoomPrefab;

    [Header("Level Arrays")]
    public List<LevelData> easyLevels = new List<LevelData>();
    public List<LevelData> mediumLevels = new List<LevelData>();
    public List<LevelData> hardLevels = new List<LevelData>();
    public List<LevelData> closedLevels = new List<LevelData>();
    public List<LevelData> closedMediumLevels = new List<LevelData>();
    public List<LevelData> closedHardLevels = new List<LevelData>();
    public GameObject startClosedRoom;
    public GameObject endClosedRoom;

    [Header("Level Spawn Flags")]
    public bool spawnEasyLevels = true;
    public bool spawnMediumLevels = true;
    public bool spawnHardLevels = true;
    public bool spawnClosedLevels = true;
    public bool spawnClosedMediumLevels = true;
    public bool spawnClosedHardLevels = true;

    [Header("Runner Prefabs")]
    public GameObject[] runnerStartPrefabs;
    public GameObject[] runnerEndPrefabs;

    [Header("Spawn Chances")]
    [Range(0, 100)]
    public int easyChance = 25;
    [Range(0, 100)]
    public int mediumChance = 25;
    [Range(0, 100)]
    public int hardChance = 25;
    [Range(0, 100)]
    public int closedChance = 25;
    [Range(0, 100)]
    public int closedMediumChance = 25;
    [Range(0, 100)]
    public int closedHardChance = 25;
    [Range(0, 100)]
    public int runnerStartChance = 25;

    [Header("Level Type Spawn Chances")]
    [Range(0, 100)]
    public int standardChance = 25;
    [Range(0, 100)]
    public int swingChance = 25;
    [Range(0, 100)]
    public int grappleChance = 25;
    [Range(0, 100)]
    public int waterChance = 25;
    [Range(0, 100)]
    public int doubleJumpChance = 25;
    [Range(0, 100)]
    public int lowGravityChance = 25;
    [Range(0, 100)]
    public int planetChance = 25;
    [Range(0, 100)]
    public int holesChance = 25;
    [Range(0, 100)]
    public int redEyeChance = 25;
    [Range(0, 100)]
    public int redWallsChance = 25;
    [Range(0, 100)]
    public int platformsChance = 25;
    [Range(0, 100)]
    public int nodeExchangerChance = 25;

    [Header("Generation Settings")]
    public int numberOfLevels = 4;
    public int numberOfLevelsToAdd = 1;
    public bool generateOnStart = true;
    public int cycle = 0;
    public int maxClosedRooms = 3;

    [Header("Runner Settings")]
    public int runnerMinRooms = 2;
    public int runnerMaxRooms = 4;

    [Header("Dynamic Batching Optimization")]
    public bool enableDynamicBatching = true;
    public bool enableGPUInstancing = true;
    public bool useSharedMaterials = true;
    public Material overrideMaterial; 

    [Header("Generated Levels")]
    public List<GameObject> generatedLevels = new List<GameObject>();

    private bool isInClosedSection = false;
    private int closedRoomsSpawned = 0;
    private int currentLevelIndex = 0;

    private bool isInRunnerSection = false;
    private int runnerRoomsSpawned = 0;
    private int runnerRoomsRequired = 0;
    private bool hasSpawnedStartClosedRoom = false; 

    private Dictionary<string, Material> materialCache = new Dictionary<string, Material>();

    [Header("Other References")]
    [SerializeField] private TextMeshProUGUI cycleText;

    public delegate void RestartGenerationDelegate();
    public static event RestartGenerationDelegate RestartGenerationEvent;
    
    public delegate void LevelCompletedDelegate(DifficultyType difficultyType);
    public static event LevelCompletedDelegate OnLevelCompleted;

    [Header("Current Difficulty Tracking")]
    public DifficultyType currentDifficultyType = DifficultyType.Easy;
    public int currentLevelInSequence = 0;

    private void Start()
    {
        InitializeBatchingSettings();
        ToggleGrappleAndSwingToRotation();

        if (generateOnStart)
        {
            GenerateLevels();
        }
    }

    void ToggleGrappleAndSwingToRotation()
    {
        bool grappleBought = PlayerPrefs.GetInt("Meta_GrappleEnabled", 0) == 1;
        bool swingBought = PlayerPrefs.GetInt("Meta_SwingEnabled", 0) == 1;

        if (grappleBought)
        {
            grappleChance = 25;
        }
        else
        {
            grappleChance = 0;
        }

        if (swingBought)
        {
            swingChance = 25;
        }
        else
        {
            swingChance = 0;
        }
    }

    public void UpdateLevelsCounts()
    {
        numberOfLevels = numberOfLevels + numberOfLevelsToAdd;
    }

    public void UpdateCycleCount()
    {
        cycle++;
        cycleText.text = cycle.ToString();
    }

    private void InitializeBatchingSettings()
    {
    }

    public void GenerateLevels()
    {
        ClearGeneratedLevels();

        if (startRoomPrefab == null || endRoomPrefab == null)
        {
            return;
        }

        isInClosedSection = false;
        isInRunnerSection = false;
        closedRoomsSpawned = 0;
        runnerRoomsSpawned = 0;
        runnerRoomsRequired = 0;
        hasSpawnedStartClosedRoom = false;
        currentLevelIndex = 0;

        GameObject startRoom = startRoomPrefab;
        startRoom.name = "StartRoom";

        if (enableDynamicBatching)
        {
            OptimizeForBatching(startRoom);
        }

        if (!generatedLevels.Contains(startRoom))
        {
            generatedLevels.Add(startRoom);
        }

        Transform previousEndPoint = FindEndPoint(startRoom);
        Vector3 currentEndPosition = previousEndPoint.position;

        for (int i = 0; i < numberOfLevels; i++)
        {
            currentLevelIndex = i;

            GameObject randomLevelPrefab = GetRandomLevelPrefab();

            if (randomLevelPrefab == null)
            {
                break;
            }

            GameObject level = Instantiate(randomLevelPrefab, currentEndPosition, Quaternion.identity);
            level.name = $"Level_{i + 1}";

            Transform levelStartPoint = FindStartPoint(level);
            
            Vector3 moveOffset = currentEndPosition - levelStartPoint.position;
            level.transform.position += moveOffset;

            if (enableDynamicBatching)
            {
                OptimizeForBatching(level);
            }

            generatedLevels.Add(level);

            currentEndPosition = FindEndPoint(level).position;
        }

        if (isInRunnerSection)
        {
            SpawnRunnerEndRoom(ref currentEndPosition);
        }

        if (isInClosedSection)
        {
            SpawnEndClosedRoom(ref currentEndPosition);
        }

        GameObject endRoomPrefabInstance = Instantiate(endRoomPrefab, currentEndPosition, Quaternion.identity);
        endRoomPrefabInstance.name = "EndRoom";

        Transform endRoomStart = FindStartPoint(endRoomPrefabInstance);
        
        Vector3 endRoomOffset = currentEndPosition - endRoomStart.position;
        endRoomPrefabInstance.transform.position += endRoomOffset;

        if (enableDynamicBatching)
        {
            OptimizeForBatching(endRoomPrefabInstance);
        }

        generatedLevels.Add(endRoomPrefabInstance);

        if (enableDynamicBatching)
        {
            LogBatchingStatistics();
        }
    }

    private void OptimizeForBatching(GameObject level)
    {
        if (level == null) return;

        MeshRenderer[] renderers = level.GetComponentsInChildren<MeshRenderer>(true);

        foreach (MeshRenderer renderer in renderers)
        {
            if (renderer == null) continue;

            if (useSharedMaterials && renderer.sharedMaterial != null)
            {
                string materialKey = renderer.sharedMaterial.name;

                if (overrideMaterial != null)
                {
                    renderer.sharedMaterial = overrideMaterial;
                    materialKey = "OverrideMaterial";
                }

                if (!materialCache.ContainsKey(materialKey))
                {
                    materialCache[materialKey] = renderer.sharedMaterial;
                }
                else
                {
                    renderer.sharedMaterial = materialCache[materialKey];
                }

                if (enableGPUInstancing)
                {
                    renderer.sharedMaterial.enableInstancing = true;
                }
            }
        }
    }

    private void LogBatchingStatistics()
    {
    }

    private GameObject GetRandomLevelPrefab()
    {
        if (isInRunnerSection)
        {
            return GetRunnerRoomPrefab();
        }

        if (isInClosedSection)
        {
            return GetClosedRoomPrefab();
        }

        List<string> availableTypes = new List<string>();
        List<int> weights = new List<int>();

        if (spawnEasyLevels && easyLevels.Count > 0 && easyChance > 0)
        {
            availableTypes.Add("Easy");
            weights.Add(easyChance);
        }

        if (spawnMediumLevels && mediumLevels.Count > 0 && mediumChance > 0)
        {
            availableTypes.Add("Medium");
            weights.Add(mediumChance);
        }

        if (spawnHardLevels && hardLevels.Count > 0 && hardChance > 0)
        {
            availableTypes.Add("Hard");
            weights.Add(hardChance);
        }

        if (spawnClosedLevels && closedLevels.Count > 0 && startClosedRoom != null && endClosedRoom != null && closedChance > 0)
        {
            availableTypes.Add("Closed");
            weights.Add(closedChance);
        }

        if (spawnClosedMediumLevels && closedMediumLevels.Count > 0 && closedMediumChance > 0)
        {
            availableTypes.Add("ClosedMedium");
            weights.Add(closedMediumChance);
        }

        if (spawnClosedHardLevels && closedHardLevels.Count > 0 && closedHardChance > 0)
        {
            availableTypes.Add("ClosedHard");
            weights.Add(closedHardChance);
        }

        if (availableTypes.Count == 0)
        {
            return null;
        }

        string selectedType = GetWeightedRandomType(availableTypes, weights);

        if (selectedType == "Closed" || selectedType == "ClosedMedium" || selectedType == "ClosedHard")
        {
            currentDifficultyType = StringToDifficultyType(selectedType);
            return StartClosedSection(selectedType);
        }

        currentDifficultyType = StringToDifficultyType(selectedType);
        
        return GetRandomLevelFromType(selectedType);
    }

    private string GetWeightedRandomType(List<string> types, List<int> weights)
    {
        int totalWeight = 0;
        foreach (int weight in weights)
        {
            totalWeight += weight;
        }

        if (totalWeight == 0)
        {
            return types[Random.Range(0, types.Count)];
        }

        int randomValue = Random.Range(0, totalWeight);
        int currentWeight = 0;

        for (int i = 0; i < types.Count; i++)
        {
            currentWeight += weights[i];
            if (randomValue < currentWeight)
            {
                return types[i];
            }
        }

        return types[0]; 
    }

    private GameObject GetRandomLevelFromType(string type)
    {
        List<LevelData> levelList = null;

        switch (type)
        {
            case "Easy":
                levelList = easyLevels;
                break;
            case "Medium":
                levelList = mediumLevels;
                break;
            case "Hard":
                levelList = hardLevels;
                break;
            case "Closed":
                levelList = closedLevels;
                break;
            case "ClosedMedium":
                levelList = closedMediumLevels;
                break;
            case "ClosedHard":
                levelList = closedHardLevels;
                break;
        }

        if (levelList == null || levelList.Count == 0)
        {
            return null;
        }

        List<GameObject> weightedPrefabs = new List<GameObject>();
        foreach (LevelData levelData in levelList)
        {
            int spawnChance = GetSpawnChanceForLevelType(levelData.levelType);
            if (spawnChance > 0)
            {
                for (int i = 0; i < spawnChance; i++)
                {
                    weightedPrefabs.Add(levelData.levelPrefab);
                }
            }
        }

        if (weightedPrefabs.Count == 0)
        {
            return null;
        }

        return weightedPrefabs[Random.Range(0, weightedPrefabs.Count)];
    }

    private int GetSpawnChanceForLevelType(LevelType levelType)
    {
        switch (levelType)
        {
            case LevelType.Standard: return standardChance;
            case LevelType.Swing: return swingChance;
            case LevelType.Grapple: return grappleChance;
            case LevelType.Water: return waterChance;
            case LevelType.DoubleJump: return doubleJumpChance;
            case LevelType.LowGravity: return lowGravityChance;
            case LevelType.Planet: return planetChance;
            case LevelType.Holes: return holesChance;
            case LevelType.RedEye: return redEyeChance;
            case LevelType.RedWalls: return redWallsChance;
            case LevelType.Platforms: return platformsChance;
            case LevelType.NodeExchanger: return nodeExchangerChance;
            default: return 25;
        }
    }

    private GameObject StartClosedSection(string closedType)
    {
        int remainingLevels = numberOfLevels - currentLevelIndex;

        if (remainingLevels <= 1)
        {
            return null;
        }

        int availableClosedRooms = Mathf.Min(maxClosedRooms, remainingLevels - 1); 

        if (availableClosedRooms <= 0)
        {
            return null;
        }

        isInClosedSection = true;
        closedRoomsSpawned = 0;
        hasSpawnedStartClosedRoom = false; 

        hasSpawnedStartClosedRoom = true;
        return startClosedRoom;
    }

    private GameObject GetClosedRoomPrefab()
    {
        if (hasSpawnedStartClosedRoom && ShouldStartRunnerSection())
        {
            return StartRunnerSection();
        }

        closedRoomsSpawned++;

        int remainingLevels = numberOfLevels - currentLevelIndex;

        bool shouldEnd = !isInRunnerSection && (closedRoomsSpawned >= maxClosedRooms || remainingLevels <= 1);

        if (shouldEnd)
        {
            isInClosedSection = false;
            hasSpawnedStartClosedRoom = false;
            return endClosedRoom;
        }

        return GetRandomLevelFromType("Closed");
    }

    private bool ShouldStartRunnerSection()
    {
        if (runnerStartPrefabs == null || runnerStartPrefabs.Length == 0 || runnerEndPrefabs == null || runnerEndPrefabs.Length == 0)
        {
            return false;
        }

        if (!isInClosedSection)
        {
            return false;
        }

        int remainingLevels = numberOfLevels - currentLevelIndex;
        int requiredSpace = runnerMinRooms + 2; 

        if (remainingLevels < requiredSpace)
        {
            return false;
        }

        float chanceMultiplier = Mathf.Clamp01((float)remainingLevels / numberOfLevels);
        int adjustedChance = Mathf.RoundToInt(runnerStartChance * chanceMultiplier);
        
        adjustedChance = Mathf.Max(10, adjustedChance);

        bool shouldStart = Random.Range(0, 100) < adjustedChance;
        
        return shouldStart;
    }

    private GameObject StartRunnerSection()
    {
        isInRunnerSection = true;
        runnerRoomsSpawned = 0;

        runnerRoomsRequired = Random.Range(runnerMinRooms, runnerMaxRooms + 1);

        return GetRandomRunnerStartPrefab();
    }

    private GameObject GetRunnerRoomPrefab()
    {
        runnerRoomsSpawned++;

        if (runnerRoomsSpawned > runnerRoomsRequired)
        {
            isInRunnerSection = false;
            return GetRandomRunnerEndPrefab();
        }

        return GetRandomLevelFromType("Closed");
    }

    private GameObject GetRandomRunnerStartPrefab()
    {
        if (runnerStartPrefabs == null || runnerStartPrefabs.Length == 0)
        {
            return null;
        }

        return runnerStartPrefabs[Random.Range(0, runnerStartPrefabs.Length)];
    }

    private GameObject GetRandomRunnerEndPrefab()
    {
        if (runnerEndPrefabs == null || runnerEndPrefabs.Length == 0)
        {
            return null;
        }

        return runnerEndPrefabs[Random.Range(0, runnerEndPrefabs.Length)];
    }

    private void SpawnEndClosedRoom(ref Vector3 currentEndPosition)
    {
        if (endClosedRoom != null)
        {
            GameObject level = Instantiate(endClosedRoom, currentEndPosition, Quaternion.identity);
            level.name = $"Level_ClosedEnd_{currentLevelIndex}";

            Transform endRoomStart = FindStartPoint(level);
            
            Vector3 offset = currentEndPosition - endRoomStart.position;
            level.transform.position += offset;

            if (enableDynamicBatching)
            {
                OptimizeForBatching(level);
            }

            generatedLevels.Add(level);

            currentEndPosition = FindEndPoint(level).position;
        }

        isInClosedSection = false;
        hasSpawnedStartClosedRoom = false;
    }

    private void SpawnRunnerEndRoom(ref Vector3 currentEndPosition)
    {
        GameObject runnerEndPrefab = GetRandomRunnerEndPrefab();
        if (runnerEndPrefab != null)
        {
            GameObject level = Instantiate(runnerEndPrefab, currentEndPosition, Quaternion.identity);
            level.name = $"Level_RunnerEnd_{currentLevelIndex}";

            Transform endRoomStart = FindStartPoint(level);
            
            Vector3 offset = currentEndPosition - endRoomStart.position;
            level.transform.position += offset;

            if (enableDynamicBatching)
            {
                OptimizeForBatching(level);
            }

            generatedLevels.Add(level);

            currentEndPosition = FindEndPoint(level).position;
        }

        isInRunnerSection = false;
    }

    public void RestartGeneration()
    {
        UpdateLevelsCounts();

        GameObject currentEndRoom = null;
        foreach (GameObject level in generatedLevels)
        {
            if (level != null && level.name == "EndRoom")
            {
                currentEndRoom = level;
                break;
            }
        }

        if (currentEndRoom == null)
        {
            return;
        }

        GameObject newStartRoom = currentEndRoom;

        ClearGeneratedLevels(keepSpecificRoom: newStartRoom);

        newStartRoom.name = "StartRoom";
        startRoomPrefab = newStartRoom;

        ResetLevelSequence();
        
        GenerateLevels();

        Invoke("DelayedRestartGeneration", 0.5f);
    }
    
    private void DelayedRestartGeneration()
    {
        RestartGenerationEvent?.Invoke();
    }

    public void GenerateLevelsSimple()
    {
        ClearGeneratedLevels();

        if (startRoomPrefab == null || endRoomPrefab == null)
        {
            return;
        }

        isInClosedSection = false;
        isInRunnerSection = false;
        closedRoomsSpawned = 0;
        runnerRoomsSpawned = 0;
        runnerRoomsRequired = 0;
        hasSpawnedStartClosedRoom = false;
        currentLevelIndex = 0;

        GameObject currentLevel = startRoomPrefab;
        currentLevel.name = "StartRoom";

        if (enableDynamicBatching)
        {
            OptimizeForBatching(currentLevel);
        }

        if (!generatedLevels.Contains(currentLevel))
        {
            generatedLevels.Add(currentLevel);
        }

        for (int i = 0; i < numberOfLevels; i++)
        {
            currentLevelIndex = i;
            GameObject randomPrefab = GetRandomLevelPrefab();

            if (randomPrefab == null)
            {
                break;
            }

            Transform currentEnd = FindEndPoint(currentLevel);

            GameObject newLevel = Instantiate(randomPrefab, currentEnd.position, Quaternion.identity);
            newLevel.name = $"Level_{i + 1}";

            Transform newLevelStart = FindStartPoint(newLevel);
            
            Vector3 offset = currentEnd.position - newLevelStart.position;
            newLevel.transform.position += offset;

            if (enableDynamicBatching)
            {
                OptimizeForBatching(newLevel);
            }

            generatedLevels.Add(newLevel);

            currentLevel = newLevel;
        }

        if (isInClosedSection)
        {
            Transform lastEnd = FindEndPoint(currentLevel);
            if (endClosedRoom != null)
            {
                GameObject closedEnd = Instantiate(endClosedRoom, lastEnd.position, Quaternion.identity);
                closedEnd.name = "Level_ClosedEnd_Final";

                Transform endRoomStart = FindStartPoint(closedEnd);
                
                Vector3 offset = lastEnd.position - endRoomStart.position;
                closedEnd.transform.position += offset;

                if (enableDynamicBatching)
                {
                    OptimizeForBatching(closedEnd);
                }

                generatedLevels.Add(closedEnd);
                currentLevel = closedEnd;
            }
        }

        if (isInRunnerSection)
        {
            Transform lastEnd = FindEndPoint(currentLevel);
            GameObject runnerEndPrefab = GetRandomRunnerEndPrefab();
            if (runnerEndPrefab != null)
            {
                GameObject runnerEnd = Instantiate(runnerEndPrefab, lastEnd.position, Quaternion.identity);
                runnerEnd.name = "Level_RunnerEnd_Final";

                Transform runnerEndStart = FindStartPoint(runnerEnd);
                
                Vector3 offset = lastEnd.position - runnerEndStart.position;
                runnerEnd.transform.position += offset;

                if (enableDynamicBatching)
                {
                    OptimizeForBatching(runnerEnd);
                }

                generatedLevels.Add(runnerEnd);
                currentLevel = runnerEnd;
            }
        }

        Transform finalEnd = FindEndPoint(currentLevel);
        GameObject finalEndRoom = Instantiate(endRoomPrefab, finalEnd.position, Quaternion.identity);
        finalEndRoom.name = "EndRoom";

        Transform endRoomStartPoint = FindStartPoint(finalEndRoom);
        
        Vector3 endRoomOffset = finalEnd.position - endRoomStartPoint.position;
        finalEndRoom.transform.position += endRoomOffset;

        if (enableDynamicBatching)
        {
            OptimizeForBatching(finalEndRoom);
        }

        generatedLevels.Add(finalEndRoom);
    }

    private Transform FindStartPoint(GameObject levelObject)
    {
        Transform startPoint = FindChildByName(levelObject.transform, "Start");

        if (startPoint == null)
        {
            return levelObject.transform;
        }
        return startPoint;
    }

    private Transform FindEndPoint(GameObject levelObject)
    {
        Transform endPoint = FindChildByName(levelObject.transform, "End");

        if (endPoint == null)
        {
            return levelObject.transform;
        }
        return endPoint;
    }

    private Transform FindChildByName(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name.ToLower() == name.ToLower())
            {
                return child;
            }

            Transform found = FindChildByName(child, name);
            if (found != null)
                return found;
        }
        return null;
    }

    public void ClearLevels()
    {
        ClearGeneratedLevels();
        materialCache.Clear(); 
    }

    private void ClearGeneratedLevels(bool keepEndRoom = false, GameObject keepSpecificRoom = null)
    {
        List<GameObject> levelsToDestroy = new List<GameObject>();
        List<GameObject> levelsToKeep = new List<GameObject>();

        foreach (GameObject level in generatedLevels)
        {
            if (level != null)
            {
                bool shouldKeep = (level == startRoomPrefab) || 
                                 (keepEndRoom && level.name == "EndRoom") ||
                                 (keepSpecificRoom != null && level == keepSpecificRoom);

                if (shouldKeep)
                {
                    levelsToKeep.Add(level);
                }
                else
                {
                    levelsToDestroy.Add(level);
                }
            }
        }

        generatedLevels = levelsToKeep;

        foreach (GameObject level in levelsToDestroy)
        {
            if (level != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(level);
                }
                else
                {
                    DestroyImmediate(level);
                }
            }
        }
    }

    [ContextMenu("Generate Levels Now")]
    private void GenerateLevelsEditor()
    {
        GenerateLevels();
    }

    [ContextMenu("Generate Levels Simple")]
    private void GenerateLevelsSimpleEditor()
    {
        GenerateLevelsSimple();
    }

    [ContextMenu("Clear All Levels")]
    private void ClearLevelsEditor()
    {
        ClearLevels();
    }

    [ContextMenu("Restart Generation")]
    private void RestartGenerationEditor()
    {
        RestartGeneration();
    }

    [ContextMenu("Log Batching Statistics")]
    private void LogBatchingStatisticsEditor()
    {
        LogBatchingStatistics();
    }

    private DifficultyType StringToDifficultyType(string type)
    {
        switch (type)
        {
            case "Easy": return DifficultyType.Easy;
            case "Medium": return DifficultyType.Medium;
            case "Hard": return DifficultyType.Hard;
            case "Closed": return DifficultyType.Closed;
            case "EasyClosed": return DifficultyType.Closed;
            case "MediumClosed": return DifficultyType.ClosedMedium;
            case "HardClosed": return DifficultyType.ClosedHard;
            case "ClosedMedium": return DifficultyType.ClosedMedium;
            case "ClosedHard": return DifficultyType.ClosedHard;
            default: return DifficultyType.Easy;
        }
    }

    public void NotifyLevelCompleted()
    {
        currentLevelInSequence++;
        OnLevelCompleted?.Invoke(currentDifficultyType);
    }

    public void ResetLevelSequence()
    {
        currentLevelInSequence = 0;
    }

    public DifficultyType GetCurrentDifficultyType()
    {
        return currentDifficultyType;
    }
}