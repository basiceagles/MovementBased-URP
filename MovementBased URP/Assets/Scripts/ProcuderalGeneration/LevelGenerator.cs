using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    [Header("Level Prefabs")]
    public GameObject startRoomPrefab; // Reference to the existing start prefab in scene
    public GameObject endRoomPrefab;
    public List<GameObject> levelPrefabs = new List<GameObject>();

    [Header("Generation Settings")]
    public int numberOfLevels = 4;
    public bool generateOnStart = true;

    [Header("Generated Levels")]
    public List<GameObject> generatedLevels = new List<GameObject>();

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

        if (startRoomPrefab == null || endRoomPrefab == null || levelPrefabs.Count == 0)
        {
            Debug.LogError("Missing required prefabs!");
            return;
        }

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
            if (levelPrefabs.Count == 0) break;

            // Select random level prefab
            GameObject randomLevelPrefab = levelPrefabs[Random.Range(0, levelPrefabs.Count)];
            
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
        
        // Find and rename the End child to Start if it exists
        Transform endPoint = FindChildByName(currentEndRoom.transform, "End");
        if (endPoint != null)
        {
            endPoint.name = "Start";
        }

        // Find and rename the Start child to End if it exists (swap the markers)
        Transform startPoint = FindChildByName(currentEndRoom.transform, "Start");
        if (startPoint != null)
        {
            startPoint.name = "End";
        }

        // Update the startRoomPrefab reference to the renamed object
        startRoomPrefab = currentEndRoom;

        // Start level generation again
        GenerateLevels();
    }

    // Alternative simpler approach - uncomment this method and call it instead if you prefer
    public void GenerateLevelsSimple()
    {
        ClearGeneratedLevels();

        if (startRoomPrefab == null || endRoomPrefab == null || levelPrefabs.Count == 0)
        {
            Debug.LogError("Missing required prefabs!");
            return;
        }

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
            GameObject randomPrefab = levelPrefabs[Random.Range(0, levelPrefabs.Count)];
            
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

        // Add EndRoom
        Transform lastEnd = FindEndPoint(currentLevel);
        GameObject endRoom = Instantiate(endRoomPrefab, Vector3.zero, Quaternion.identity);
        endRoom.name = "EndRoom";
        
        // Position EndRoom so its Start aligns with the last level's End
        Transform endRoomStart = FindStartPoint(endRoom);
        Vector3 endRoomPosition = lastEnd.position - endRoomStart.localPosition;
        endRoom.transform.position = endRoomPosition;
        
        generatedLevels.Add(endRoom);

        Debug.Log($"Generated {generatedLevels.Count - 1} levels: Using existing StartRoom + {numberOfLevels} levels + EndRoom");
    }

    private Transform FindStartPoint(GameObject levelObject)
    {
        // Handle both prefabs and scene objects
        Transform startPoint = null;
        if (levelObject.scene.name != null) // It's a scene object
        {
            startPoint = FindChildByName(levelObject.transform, "Start");
        }
        else // It's a prefab
        {
            // For prefabs, we need to search in the prefab's transform
            startPoint = FindChildByName(levelObject.transform, "Start");
        }
        
        if (startPoint == null)
        {
            Debug.LogError($"No 'Start' GameObject found in {levelObject.name}!");
            return levelObject.transform;
        }
        return startPoint;
    }

    private Transform FindEndPoint(GameObject levelObject)
    {
        // Handle both prefabs and scene objects
        Transform endPoint = null;
        if (levelObject.scene.name != null) // It's a scene object
        {
            endPoint = FindChildByName(levelObject.transform, "End");
        }
        else // It's a prefab
        {
            // For prefabs, we need to search in the prefab's transform
            endPoint = FindChildByName(levelObject.transform, "End");
        }
        
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
            
            // Search recursively in children
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
        // Create a list to avoid modification during iteration
        List<GameObject> levelsToDestroy = new List<GameObject>();
        List<GameObject> levelsToKeep = new List<GameObject>();

        foreach (GameObject level in generatedLevels)
        {
            if (level != null)
            {
                // Keep the original start room prefab and optionally the end room
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

    // Editor button for easy testing
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
}