using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    [Header("Level Prefabs")]
    public GameObject startRoomPrefab; // Reference to the existing start prefab in scene
    public GameObject endRoomPrefab;
    
    [Header("Level Arrays")]
    public List<GameObject> easyLevels = new List<GameObject>();
    public List<GameObject> mediumLevels = new List<GameObject>();
    public List<GameObject> hardLevels = new List<GameObject>();
    public List<GameObject> closedLevels = new List<GameObject>();
    public GameObject startClosedRoom;
    public GameObject endClosedRoom;

    [Header("Spawn Chances")]
    [Range(0, 100)]
    public int easyChance = 25;
    [Range(0, 100)]
    public int mediumChance = 25;
    [Range(0, 100)]
    public int hardChance = 25;
    [Range(0, 100)]
    public int closedChance = 25;

    [Header("Cycle Requirements")]
    public int easyCycleRequired = 0;
    public int mediumCycleRequired = 3;
    public int hardCycleRequired = 5;
    public int closedCycleRequired = 0;

    [Header("Generation Settings")]
    public int numberOfLevels = 4;
    public bool generateOnStart = true;
    public int cycle = 0;
    public int maxClosedRooms = 3;

    [Header("Generated Levels")]
    public List<GameObject> generatedLevels = new List<GameObject>();

    // Private variables for closed room tracking
    private bool isInClosedSection = false;
    private int closedRoomsSpawned = 0;
    private int currentLevelIndex = 0;

    private void Start()
    {
        if (generateOnStart)
        {
            GenerateLevels();
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

        // Reset closed room tracking
        isInClosedSection = false;
        closedRoomsSpawned = 0;
        currentLevelIndex = 0;

        // Use existing Start Room instead of instantiating a new one
        GameObject startRoom = startRoomPrefab;
        startRoom.name = "StartRoom";
        
        // Only add to generated levels if it's not already there
        if (!generatedLevels.Contains(startRoom))
        {
            generatedLevels.Add(startRoom);
        }

        // Get the End point from the existing StartRoom
        Transform previousEndPoint = FindEndPoint(startRoom);
        Vector3 currentSpawnPosition = previousEndPoint.position;

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

            // Get the Start point of the level prefab to use as reference
            Transform levelStartPoint = FindStartPoint(randomLevelPrefab);
            
            // Calculate spawn position so the level's Start point aligns with previous End point
            Vector3 spawnPosition = currentSpawnPosition - levelStartPoint.localPosition;
            
            // Instantiate the level at the calculated spawn position
            GameObject level = Instantiate(randomLevelPrefab, spawnPosition, Quaternion.identity);
            level.name = $"Level_{i + 1}";
            generatedLevels.Add(level);

            // Update the current spawn position to be the End point of this level
            currentSpawnPosition = FindEndPoint(level).position;
        }

        // If we're still in a closed section at the end, close it
        if (isInClosedSection)
        {
            SpawnEndClosedRoom(ref currentSpawnPosition);
        }

        // Generate End Room
        GameObject endRoomPrefabInstance = Instantiate(endRoomPrefab, Vector3.zero, Quaternion.identity);
        endRoomPrefabInstance.name = "EndRoom";
        
        // Get the Start point of the EndRoom and position it correctly
        Transform endRoomStart = FindStartPoint(endRoomPrefabInstance);
        Vector3 endRoomPosition = currentSpawnPosition - endRoomStart.localPosition;
        endRoomPrefabInstance.transform.position = endRoomPosition;
        
        generatedLevels.Add(endRoomPrefabInstance);

        Debug.Log($"Generated {generatedLevels.Count - 1} levels: Using existing StartRoom + {numberOfLevels} levels + EndRoom");
    }

    private GameObject GetRandomLevelPrefab()
    {
        // Check if we're in a closed section and need to continue spawning closed rooms
        if (isInClosedSection)
        {
            return GetClosedRoomPrefab();
        }

        // Calculate available level types based on cycle and build weighted list
        List<string> availableTypes = new List<string>();
        List<int> weights = new List<int>();

        // Easy levels - check cycle requirement
        if (cycle >= easyCycleRequired && easyLevels.Count > 0)
        {
            availableTypes.Add("Easy");
            weights.Add(easyChance);
        }
        
        // Medium levels - check cycle requirement
        if (cycle >= mediumCycleRequired && mediumLevels.Count > 0)
        {
            availableTypes.Add("Medium");
            weights.Add(mediumChance);
        }
            
        // Hard levels - check cycle requirement
        if (cycle >= hardCycleRequired && hardLevels.Count > 0)
        {
            availableTypes.Add("Hard");
            weights.Add(hardChance);
        }
            
        // Closed levels - check cycle requirement and special prefabs
        if (cycle >= closedCycleRequired && closedLevels.Count > 0 && startClosedRoom != null && endClosedRoom != null)
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
        List<GameObject> levelList = null;
        
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
        }

        if (levelList == null || levelList.Count == 0)
        {
            Debug.LogError($"No levels available in {type} category!");
            // Fallback to first available type
            if (easyLevels.Count > 0) return easyLevels[0];
            if (mediumLevels.Count > 0) return mediumLevels[0];
            if (hardLevels.Count > 0) return hardLevels[0];
            return null;
        }

        return levelList[Random.Range(0, levelList.Count)];
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
        
        Debug.Log($"Starting closed section at level {currentLevelIndex + 1}, will spawn up to {availableClosedRooms} closed rooms");
        
        // Return the start closed room
        return startClosedRoom;
    }

    private GameObject GetClosedRoomPrefab()
    {
        closedRoomsSpawned++;
        
        int remainingLevels = numberOfLevels - currentLevelIndex;
        
        // Check if we need to end the closed section
        bool shouldEnd = closedRoomsSpawned >= maxClosedRooms || remainingLevels <= 1;
        
        if (shouldEnd)
        {
            Debug.Log($"Ending closed section after {closedRoomsSpawned} rooms, remaining levels: {remainingLevels}");
            isInClosedSection = false;
            return endClosedRoom;
        }
        
        // Return a random closed level
        if (closedLevels.Count > 0)
        {
            return closedLevels[Random.Range(0, closedLevels.Count)];
        }
        else
        {
            // If no closed levels available, end the section
            Debug.Log("No closed levels available, ending closed section");
            isInClosedSection = false;
            return endClosedRoom;
        }
    }

    private void SpawnEndClosedRoom(ref Vector3 currentSpawnPosition)
    {
        if (endClosedRoom != null)
        {
            Transform endRoomStart = FindStartPoint(endClosedRoom);
            Vector3 spawnPosition = currentSpawnPosition - endRoomStart.localPosition;
            GameObject level = Instantiate(endClosedRoom, spawnPosition, Quaternion.identity);
            level.name = $"Level_ClosedEnd_{currentLevelIndex}";
            generatedLevels.Add(level);
            
            // Update spawn position using the existing waypoints from the prefab
            currentSpawnPosition = FindEndPoint(level).position;
        }
        
        isInClosedSection = false;
    }

    public void RestartGeneration()
    {
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

        // Delete all generated levels except the end room
        ClearGeneratedLevels(keepEndRoom: true);

        // Rename End to Start
        currentEndRoom.name = "StartRoom";
        
        // Update the startRoomPrefab reference to the renamed object
        startRoomPrefab = currentEndRoom;

        // Start level generation again
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

        // Reset closed room tracking
        isInClosedSection = false;
        closedRoomsSpawned = 0;
        currentLevelIndex = 0;

        // Use existing StartRoom
        GameObject currentLevel = startRoomPrefab;
        currentLevel.name = "StartRoom";
        
        if (!generatedLevels.Contains(currentLevel))
        {
            generatedLevels.Add(currentLevel);
        }

        // Add random levels
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
            
            // Get the Start point of the new level prefab
            Transform newLevelStart = FindStartPoint(randomPrefab);
            
            // Calculate spawn position so new level's Start aligns with current level's End
            Vector3 spawnPosition = currentEnd.position - newLevelStart.localPosition;
            
            // Instantiate new level at the calculated position
            GameObject newLevel = Instantiate(randomPrefab, spawnPosition, Quaternion.identity);
            newLevel.name = $"Level_{i + 1}";
            generatedLevels.Add(newLevel);

            currentLevel = newLevel;
        }

        // If we're still in a closed section at the end, close it
        if (isInClosedSection)
        {
            Transform lastEnd = FindEndPoint(currentLevel);
            if (endClosedRoom != null)
            {
                Transform endRoomStart = FindStartPoint(endClosedRoom);
                Vector3 closedEndPosition = lastEnd.position - endRoomStart.localPosition;
                GameObject closedEnd = Instantiate(endClosedRoom, closedEndPosition, Quaternion.identity);
                closedEnd.name = "Level_ClosedEnd_Final";
                generatedLevels.Add(closedEnd);
                currentLevel = closedEnd;
            }
        }

        // Add EndRoom
        Transform finalEnd = FindEndPoint(currentLevel);
        GameObject finalEndRoom = Instantiate(endRoomPrefab, Vector3.zero, Quaternion.identity);
        finalEndRoom.name = "EndRoom";
        
        // Position EndRoom so its Start aligns with the last level's End
        Transform endRoomStartPoint = FindStartPoint(finalEndRoom);
        Vector3 finalEndRoomPosition = finalEnd.position - endRoomStartPoint.localPosition;
        finalEndRoom.transform.position = finalEndRoomPosition;
        
        generatedLevels.Add(finalEndRoom);

        Debug.Log($"Generated {generatedLevels.Count - 1} levels: Using existing StartRoom + {numberOfLevels} levels + EndRoom");
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
    }

    private void ClearGeneratedLevels(bool keepEndRoom = false)
    {
        List<GameObject> levelsToDestroy = new List<GameObject>();
        List<GameObject> levelsToKeep = new List<GameObject>();

        foreach (GameObject level in generatedLevels)
        {
            if (level != null)
            {
                bool shouldKeep = (level == startRoomPrefab) || (keepEndRoom && level.name == "EndRoom");
                
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
    //dd
}