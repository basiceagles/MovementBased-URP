using System.Collections.Generic;
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
        NodeExchanger
    }

    [System.Serializable]
    public class LevelData
    {
        public GameObject levelPrefab;
        public LevelType levelType;
    }

    [Header("Level Prefabs")]
    [Tooltip("START SCENE PREFAB MUST BE USED FROM SCENE!")]
    public GameObject startRoomPrefab; // Reference to the existing start prefab in scene
    public GameObject endRoomPrefab;

    [Header("Level Arrays")]
    public List<LevelData> easyLevels = new List<LevelData>();
    public List<LevelData> closedLevels = new List<LevelData>();
    public GameObject startClosedRoom;
    public GameObject endClosedRoom;

    [Header("Level Spawn Flags")]
    public bool spawnEasyLevels = true;
    public bool spawnClosedLevels = true;

    [Header("Runner Prefabs")]
    public GameObject runnerStartPrefab;
    public GameObject runnerEndPrefab;

    [Header("Spawn Chances")]
    [Range(0, 100)]
    public int easyChance = 25;
    [Range(0, 100)]
    public int closedChance = 25;
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
    public Material overrideMaterial; // Optional: Force all levels to use this material

    [Header("Generated Levels")]
    public List<GameObject> generatedLevels = new List<GameObject>();

    // Private variables for closed room tracking
    private bool isInClosedSection = false;
    private int closedRoomsSpawned = 0;
    private int currentLevelIndex = 0;

    // Private variables for runner section tracking
    private bool isInRunnerSection = false;
    private int runnerRoomsSpawned = 0;
    private int runnerRoomsRequired = 0;
    private bool hasSpawnedStartClosedRoom = false; // Track if we've spawned the start closed room

    // Material cache for batching optimization
    private Dictionary<string, Material> materialCache = new Dictionary<string, Material>();

    //Events
    public delegate void RestartGenerationDelegate();
    public static event RestartGenerationDelegate RestartGenerationEvent;
    

    private void Start()
    {
        InitializeBatchingSettings();

        if (generateOnStart)
        {
            GenerateLevels();
        }
    }

    public void UpdateLevelsCounts()
    {
        numberOfLevels = numberOfLevels + numberOfLevelsToAdd;
    }

    public void UpdateCycleCount()
    {
        cycle++;
    }

    private void InitializeBatchingSettings()
    {
        if (enableGPUInstancing)
        {
            Debug.Log("GPU Instancing optimization enabled");
        }
    }

    public void GenerateLevels()
    {
        // Clear existing levels (but keep the start room if it exists)
        ClearGeneratedLevels();

        if (startRoomPrefab == null || endRoomPrefab == null)
        {
            Debug.LogError("Missing required prefabs!");
            return;
        }

        // Reset room tracking
        isInClosedSection = false;
        isInRunnerSection = false;
        closedRoomsSpawned = 0;
        runnerRoomsSpawned = 0;
        runnerRoomsRequired = 0;
        hasSpawnedStartClosedRoom = false;
        currentLevelIndex = 0;

        // Use existing Start Room instead of instantiating a new one
        GameObject startRoom = startRoomPrefab;
        startRoom.name = "StartRoom";

        // Optimize existing start room for batching
        if (enableDynamicBatching)
        {
            OptimizeForBatching(startRoom);
        }

        // Only add to generated levels if it's not already there
        if (!generatedLevels.Contains(startRoom))
        {
            generatedLevels.Add(startRoom);
        }

        // Get the End point from the existing StartRoom
        Transform previousEndPoint = FindEndPoint(startRoom);
        Vector3 currentEndPosition = previousEndPoint.position;

        // Generate Random Levels
        for (int i = 0; i < numberOfLevels; i++)
        {
            currentLevelIndex = i;

            // Select random level prefab based on cycle and chances
            GameObject randomLevelPrefab = GetRandomLevelPrefab();

            if (randomLevelPrefab == null)
            {
                Debug.LogWarning("No suitable level prefab found, skipping level generation.");
                break;
            }

            // SIMPLE AND CONSISTENT APPROACH: Instantiate then adjust
            GameObject level = Instantiate(randomLevelPrefab, currentEndPosition, Quaternion.identity);
            level.name = $"Level_{i + 1}";

            // Get the Start point of the instantiated level
            Transform levelStartPoint = FindStartPoint(level);
            
            // Calculate how much we need to move the level so its Start point matches currentEndPosition
            Vector3 moveOffset = currentEndPosition - levelStartPoint.position;
            level.transform.position += moveOffset;

            // Optimize for dynamic batching
            if (enableDynamicBatching)
            {
                OptimizeForBatching(level);
            }

            generatedLevels.Add(level);

            // Update the current end position to be the End point of this level
            currentEndPosition = FindEndPoint(level).position;
            
            Debug.Log($"Level {i+1}: Start at {FindStartPoint(level).position}, End at {currentEndPosition}");
        }

        // If we're still in a closed section at the end, close it
        if (isInClosedSection)
        {
            SpawnEndClosedRoom(ref currentEndPosition);
        }

        // If we're still in a runner section at the end, end it
        if (isInRunnerSection)
        {
            SpawnRunnerEndRoom(ref currentEndPosition);
        }

        // Generate End Room using same consistent approach
        GameObject endRoomPrefabInstance = Instantiate(endRoomPrefab, currentEndPosition, Quaternion.identity);
        endRoomPrefabInstance.name = "EndRoom";

        // Get the Start point of the instantiated EndRoom
        Transform endRoomStart = FindStartPoint(endRoomPrefabInstance);
        
        // Calculate the offset needed to align the Start point with the current end position
        Vector3 endRoomOffset = currentEndPosition - endRoomStart.position;
        endRoomPrefabInstance.transform.position += endRoomOffset;

        // Optimize end room for batching
        if (enableDynamicBatching)
        {
            OptimizeForBatching(endRoomPrefabInstance);
        }

        generatedLevels.Add(endRoomPrefabInstance);

        Debug.Log($"Generated {generatedLevels.Count - 1} levels: Using existing StartRoom + {numberOfLevels} levels + EndRoom");

        // Log batching statistics
        if (enableDynamicBatching)
        {
            LogBatchingStatistics();
        }
    }

    private void OptimizeForBatching(GameObject level)
    {
        if (level == null) return;

        MeshRenderer[] renderers = level.GetComponentsInChildren<MeshRenderer>(true);
        int optimizedCount = 0;

        foreach (MeshRenderer renderer in renderers)
        {
            if (renderer == null) continue;

            // Ensure uniform scale for dynamic batching
            if (renderer.transform.lossyScale != Vector3.one)
            {
                Debug.LogWarning($"Non-uniform scale on {renderer.name} may prevent batching: {renderer.transform.lossyScale}");
            }

            // Material optimization
            if (useSharedMaterials && renderer.sharedMaterial != null)
            {
                string materialKey = renderer.sharedMaterial.name;

                // Use override material if specified
                if (overrideMaterial != null)
                {
                    renderer.sharedMaterial = overrideMaterial;
                    materialKey = "OverrideMaterial";
                }

                // Cache and share materials
                if (!materialCache.ContainsKey(materialKey))
                {
                    materialCache[materialKey] = renderer.sharedMaterial;
                }
                else
                {
                    renderer.sharedMaterial = materialCache[materialKey];
                }

                // Enable GPU instancing on materials
                if (enableGPUInstancing)
                {
                    renderer.sharedMaterial.enableInstancing = true;
                }
            }

            // Check mesh vertex count for dynamic batching
            MeshFilter meshFilter = renderer.GetComponent<MeshFilter>();
            if (meshFilter != null && meshFilter.sharedMesh != null)
            {
                int vertexCount = meshFilter.sharedMesh.vertexCount;
                if (vertexCount > 300)
                {
                    Debug.LogWarning($"Mesh {meshFilter.sharedMesh.name} has {vertexCount} vertices (>300), may not batch dynamically");
                }
            }

            optimizedCount++;
        }

        if (optimizedCount > 0)
        {
            Debug.Log($"Optimized {optimizedCount} renderers in {level.name} for batching");
        }
    }

    private void LogBatchingStatistics()
    {
        int totalRenderers = 0;
        int batchedRenderers = 0;
        Dictionary<string, int> materialUsage = new Dictionary<string, int>();

        foreach (GameObject level in generatedLevels)
        {
            if (level == null) continue;

            MeshRenderer[] renderers = level.GetComponentsInChildren<MeshRenderer>(true);
            foreach (MeshRenderer renderer in renderers)
            {
                if (renderer == null) continue;

                totalRenderers++;

                // Count material usage
                if (renderer.sharedMaterial != null)
                {
                    string matName = renderer.sharedMaterial.name;
                    if (materialUsage.ContainsKey(matName))
                    {
                        materialUsage[matName]++;
                    }
                    else
                    {
                        materialUsage[matName] = 1;
                    }

                    // Consider renderer as "batchable" if it uses a shared material
                    if (useSharedMaterials && materialUsage[matName] > 1)
                    {
                        batchedRenderers++;
                    }
                }
            }
        }

        Debug.Log($"Batching Statistics: {batchedRenderers}/{totalRenderers} renderers optimized for batching");
        Debug.Log($"Material usage: {materialUsage.Count} unique materials");

        foreach (var kvp in materialUsage)
        {
            if (kvp.Value > 1)
            {
                Debug.Log($"  {kvp.Key}: used {kvp.Value} times (batchable)");
            }
        }
    }

    private GameObject GetRandomLevelPrefab()
    {
        // Check if we're in a runner section
        if (isInRunnerSection)
        {
            return GetRunnerRoomPrefab();
        }

        // Check if we're in a closed section and need to continue spawning closed rooms
        if (isInClosedSection)
        {
            return GetClosedRoomPrefab();
        }

        // Calculate available level types based on cycle and build weighted list
        List<string> availableTypes = new List<string>();
        List<int> weights = new List<int>();

        // Easy levels - check spawn flag
        if (spawnEasyLevels && easyLevels.Count > 0)
        {
            availableTypes.Add("Easy");
            weights.Add(easyChance);
        }

        // Closed levels - check spawn flag and special prefabs
        if (spawnClosedLevels && closedLevels.Count > 0 && startClosedRoom != null && endClosedRoom != null)
        {
            availableTypes.Add("Closed");
            weights.Add(closedChance);
        }

        if (availableTypes.Count == 0)
        {
            Debug.LogError("No available level types!");
            return null;
        }

        // Select random level type based on weights
        string selectedType = GetWeightedRandomType(availableTypes, weights);

        // Handle closed levels separately
        if (selectedType == "Closed")
        {
            return StartClosedSection();
        }

        // Return random level from the selected type
        return GetRandomLevelFromType(selectedType);
    }

    private string GetWeightedRandomType(List<string> types, List<int> weights)
    {
        int totalWeight = 0;
        foreach (int weight in weights)
        {
            totalWeight += weight;
        }

        // If total weight is 0, distribute equally
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

        return types[0]; // Fallback
    }

    private GameObject GetRandomLevelFromType(string type)
    {
        List<LevelData> levelList = null;

        switch (type)
        {
            case "Easy":
                levelList = easyLevels;
                break;
            case "Closed":
                levelList = closedLevels;
                break;
        }

        if (levelList == null || levelList.Count == 0)
        {
            Debug.LogError($"No levels available in {type} category!");
            return null;
        }

        // Create weighted list based on level type spawn chances
        List<GameObject> weightedPrefabs = new List<GameObject>();
        foreach (LevelData levelData in levelList)
        {
            int spawnChance = GetSpawnChanceForLevelType(levelData.levelType);
            for (int i = 0; i < spawnChance; i++)
            {
                weightedPrefabs.Add(levelData.levelPrefab);
            }
        }

        if (weightedPrefabs.Count == 0)
        {
            Debug.LogError($"No weighted prefabs available for {type} category!");
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
            case LevelType.NodeExchanger: return nodeExchangerChance;
            default: return 25;
        }
    }

    private GameObject StartClosedSection()
    {
        // Check if we can start a closed section based on remaining levels
        int remainingLevels = numberOfLevels - currentLevelIndex;

        // If this is the last level, don't start a closed section
        if (remainingLevels <= 1)
        {
            Debug.Log("Cannot start closed section on last level, falling back to easy level");
            return GetRandomLevelFromType("Easy");
        }

        // Calculate how many closed rooms we can actually spawn
        int availableClosedRooms = Mathf.Min(maxClosedRooms, remainingLevels - 1); // -1 for the end closed room

        if (availableClosedRooms <= 0)
        {
            Debug.Log("Not enough space for closed section, falling back to easy level");
            return GetRandomLevelFromType("Easy");
        }

        // Start closed section
        isInClosedSection = true;
        closedRoomsSpawned = 0;
        hasSpawnedStartClosedRoom = false; // Reset for new closed section

        Debug.Log($"Starting closed section at level {currentLevelIndex + 1}, will spawn up to {availableClosedRooms} closed rooms");

        // Return the start closed room
        hasSpawnedStartClosedRoom = true;
        return startClosedRoom;
    }

    private GameObject GetClosedRoomPrefab()
    {
        // Check if we should start a runner section (only after startClosedRoom has been spawned)
        if (hasSpawnedStartClosedRoom && ShouldStartRunnerSection())
        {
            return StartRunnerSection();
        }

        closedRoomsSpawned++;

        int remainingLevels = numberOfLevels - currentLevelIndex;

        // Check if we need to end the closed section
        bool shouldEnd = closedRoomsSpawned >= maxClosedRooms || remainingLevels <= 1;

        if (shouldEnd)
        {
            Debug.Log($"Ending closed section after {closedRoomsSpawned} rooms, remaining levels: {remainingLevels}");
            isInClosedSection = false;
            hasSpawnedStartClosedRoom = false;
            return endClosedRoom;
        }

        // Return a random closed level based on level type spawn chances
        return GetRandomLevelFromType("Closed");
    }

    private bool ShouldStartRunnerSection()
    {
        // Check if runner prefabs are available
        if (runnerStartPrefab == null || runnerEndPrefab == null)
        {
            return false;
        }

        // Check if we're in a closed section (runner can only spawn inside closed section)
        if (!isInClosedSection)
        {
            return false;
        }

        // Check if we have enough space left for a runner section
        int remainingLevels = numberOfLevels - currentLevelIndex;
        int requiredSpace = runnerMinRooms + 2; // +2 for start and end prefabs

        if (remainingLevels < requiredSpace)
        {
            return false;
        }

        // Check if we're approaching the end (don't start runner if we're too close to maxClosedRooms)
        int levelsUsed = currentLevelIndex;
        int maxAllowedForRunner = maxClosedRooms - 1; // Leave room for at least one non-runner level

        if (levelsUsed >= maxAllowedForRunner)
        {
            return false;
        }

        // Random chance to start runner section
        return Random.Range(0, 100) < runnerStartChance;
    }

    private GameObject StartRunnerSection()
    {
        isInRunnerSection = true;
        runnerRoomsSpawned = 0;

        // Determine how many closed rooms to spawn between start and end
        runnerRoomsRequired = Random.Range(runnerMinRooms, runnerMaxRooms + 1);

        Debug.Log($"Starting runner section at level {currentLevelIndex + 1}, will spawn {runnerRoomsRequired} closed rooms before end");

        return runnerStartPrefab;
    }

    private GameObject GetRunnerRoomPrefab()
    {
        runnerRoomsSpawned++;

        // Check if we've spawned enough closed rooms to end the runner section
        if (runnerRoomsSpawned > runnerRoomsRequired)
        {
            Debug.Log($"Ending runner section after {runnerRoomsSpawned - 1} closed rooms");
            isInRunnerSection = false;
            return runnerEndPrefab;
        }

        // Return a random closed level for the middle rooms based on level type spawn chances
        return GetRandomLevelFromType("Closed");
    }

    private void SpawnEndClosedRoom(ref Vector3 currentEndPosition)
    {
        if (endClosedRoom != null)
        {
            // Use the same consistent approach: instantiate then adjust
            GameObject level = Instantiate(endClosedRoom, currentEndPosition, Quaternion.identity);
            level.name = $"Level_ClosedEnd_{currentLevelIndex}";

            // Get the Start point of the instantiated level
            Transform endRoomStart = FindStartPoint(level);
            
            // Calculate the offset needed to align the Start point with the current end position
            Vector3 offset = currentEndPosition - endRoomStart.position;
            level.transform.position += offset;

            // Optimize for batching
            if (enableDynamicBatching)
            {
                OptimizeForBatching(level);
            }

            generatedLevels.Add(level);

            // Update end position using the existing waypoints from the prefab
            currentEndPosition = FindEndPoint(level).position;
        }

        isInClosedSection = false;
        hasSpawnedStartClosedRoom = false;
    }

    private void SpawnRunnerEndRoom(ref Vector3 currentEndPosition)
    {
        if (runnerEndPrefab != null)
        {
            // Use the same consistent approach: instantiate then adjust
            GameObject level = Instantiate(runnerEndPrefab, currentEndPosition, Quaternion.identity);
            level.name = $"Level_RunnerEnd_{currentLevelIndex}";

            // Get the Start point of the instantiated level
            Transform endRoomStart = FindStartPoint(level);
            
            // Calculate the offset needed to align the Start point with the current end position
            Vector3 offset = currentEndPosition - endRoomStart.position;
            level.transform.position += offset;

            // Optimize for batching
            if (enableDynamicBatching)
            {
                OptimizeForBatching(level);
            }

            generatedLevels.Add(level);

            // Update end position using the existing waypoints from the prefab
            currentEndPosition = FindEndPoint(level).position;
        }

        isInRunnerSection = false;
    }

    public void RestartGeneration()
    {
        RestartGenerationEvent?.Invoke();
        
        UpdateLevelsCounts();
        UpdateCycleCount();

        // Find the current end room (which will become the new start)
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
            Debug.LogError("No EndRoom found to restart from!");
            return;
        }

        // Store the reference to the current end room before clearing
        GameObject newStartRoom = currentEndRoom;

        // Clear all generated levels EXCEPT the current end room
        ClearGeneratedLevels(keepSpecificRoom: newStartRoom);

        // Rename End to Start and update the reference
        newStartRoom.name = "StartRoom";
        startRoomPrefab = newStartRoom;

        // Reset the end room prefab reference so we can spawn a new one
        // If you want to keep using the same end prefab, make sure endRoomPrefab is still set in the inspector

        Debug.Log($"Restarting generation: EndRoom '{newStartRoom.name}' is now the new StartRoom");

        // Start level generation again - this will use the new start room and generate new levels
        GenerateLevels();
    }

    public void GenerateLevelsSimple()
    {
        ClearGeneratedLevels();

        if (startRoomPrefab == null || endRoomPrefab == null)
        {
            Debug.LogError("Missing required prefabs!");
            return;
        }

        // Reset room tracking
        isInClosedSection = false;
        isInRunnerSection = false;
        closedRoomsSpawned = 0;
        runnerRoomsSpawned = 0;
        runnerRoomsRequired = 0;
        hasSpawnedStartClosedRoom = false;
        currentLevelIndex = 0;

        // Use existing StartRoom
        GameObject currentLevel = startRoomPrefab;
        currentLevel.name = "StartRoom";

        // Optimize existing start room
        if (enableDynamicBatching)
        {
            OptimizeForBatching(currentLevel);
        }

        if (!generatedLevels.Contains(currentLevel))
        {
            generatedLevels.Add(currentLevel);
        }

        // Add random levels using the same consistent approach
        for (int i = 0; i < numberOfLevels; i++)
        {
            currentLevelIndex = i;
            GameObject randomPrefab = GetRandomLevelPrefab();

            if (randomPrefab == null)
            {
                Debug.LogWarning("No suitable level prefab found, skipping level generation.");
                break;
            }

            // Get the End point of the current level
            Transform currentEnd = FindEndPoint(currentLevel);

            // Use the same consistent approach: instantiate then adjust
            GameObject newLevel = Instantiate(randomPrefab, currentEnd.position, Quaternion.identity);
            newLevel.name = $"Level_{i + 1}";

            // Get the Start point of the instantiated level
            Transform newLevelStart = FindStartPoint(newLevel);
            
            // Calculate the offset needed to align the Start point with the End point
            Vector3 offset = currentEnd.position - newLevelStart.position;
            newLevel.transform.position += offset;

            // Optimize for batching
            if (enableDynamicBatching)
            {
                OptimizeForBatching(newLevel);
            }

            generatedLevels.Add(newLevel);

            currentLevel = newLevel;
        }

        // If we're still in a closed section at the end, close it
        if (isInClosedSection)
        {
            Transform lastEnd = FindEndPoint(currentLevel);
            if (endClosedRoom != null)
            {
                // Use the same consistent approach: instantiate then adjust
                GameObject closedEnd = Instantiate(endClosedRoom, lastEnd.position, Quaternion.identity);
                closedEnd.name = "Level_ClosedEnd_Final";

                // Get the Start point of the instantiated level
                Transform endRoomStart = FindStartPoint(closedEnd);
                
                // Calculate the offset needed to align the Start point with the End point
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

        // If we're still in a runner section at the end, end it
        if (isInRunnerSection)
        {
            Transform lastEnd = FindEndPoint(currentLevel);
            if (runnerEndPrefab != null)
            {
                // Use the same consistent approach: instantiate then adjust
                GameObject runnerEnd = Instantiate(runnerEndPrefab, lastEnd.position, Quaternion.identity);
                runnerEnd.name = "Level_RunnerEnd_Final";

                // Get the Start point of the instantiated level
                Transform runnerEndStart = FindStartPoint(runnerEnd);
                
                // Calculate the offset needed to align the Start point with the End point
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

        // Add EndRoom using the same consistent approach
        Transform finalEnd = FindEndPoint(currentLevel);
        GameObject finalEndRoom = Instantiate(endRoomPrefab, finalEnd.position, Quaternion.identity);
        finalEndRoom.name = "EndRoom";

        // Get the Start point of the instantiated EndRoom
        Transform endRoomStartPoint = FindStartPoint(finalEndRoom);
        
        // Calculate the offset needed to align the Start point with the End point
        Vector3 endRoomOffset = finalEnd.position - endRoomStartPoint.position;
        finalEndRoom.transform.position += endRoomOffset;

        // Optimize end room
        if (enableDynamicBatching)
        {
            OptimizeForBatching(finalEndRoom);
        }

        generatedLevels.Add(finalEndRoom);

        Debug.Log($"Generated {generatedLevels.Count - 1} levels: Using existing StartRoom + {numberOfLevels} levels + EndRoom");

        // Log batching statistics
        if (enableDynamicBatching)
        {
            LogBatchingStatistics();
        }
    }

    private Transform FindStartPoint(GameObject levelObject)
    {
        Transform startPoint = FindChildByName(levelObject.transform, "Start");

        if (startPoint == null)
        {
            Debug.LogError($"No 'Start' GameObject found in {levelObject.name}!");
            return levelObject.transform;
        }
        return startPoint;
    }

    private Transform FindEndPoint(GameObject levelObject)
    {
        Transform endPoint = FindChildByName(levelObject.transform, "End");

        if (endPoint == null)
        {
            Debug.LogError($"No 'End' GameObject found in {levelObject.name}!");
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
        materialCache.Clear(); // Clear material cache when clearing levels
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
}