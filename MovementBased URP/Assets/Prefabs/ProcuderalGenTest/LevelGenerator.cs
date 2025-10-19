using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    [Header("Level Prefabs")]
    public GameObject startRoomPrefab;
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
        // Clear existing levels
        ClearLevels();

        if (startRoomPrefab == null || endRoomPrefab == null || levelPrefabs.Count == 0)
        {
            Debug.LogError("Missing required prefabs!");
            return;
        }

        // Generate Start Room at position zero
        GameObject startRoom = Instantiate(startRoomPrefab, Vector3.zero, Quaternion.identity);
        startRoom.name = "StartRoom";
        generatedLevels.Add(startRoom);

        // Get the End point from the spawned StartRoom
        Transform previousEndPoint = FindEndPoint(startRoom);
        Vector3 currentSpawnPosition = previousEndPoint.position;

        // Generate Random Levels
        for (int i = 0; i < numberOfLevels; i++)
        {
            if (levelPrefabs.Count == 0) break;

            // Select random level prefab
            GameObject randomLevelPrefab = levelPrefabs[Random.Range(0, levelPrefabs.Count)];
            
            // Instantiate the level at the current spawn position
            GameObject level = Instantiate(randomLevelPrefab, currentSpawnPosition, Quaternion.identity);
            level.name = $"Level_{i + 1}";
            generatedLevels.Add(level);

            // Get the Start point of the newly spawned level
            Transform currentLevelStart = FindStartPoint(level);
            Transform currentLevelEnd = FindEndPoint(level);

            // Calculate the offset between where the level actually spawned and where its Start point is
            Vector3 spawnOffset = currentSpawnPosition - currentLevelStart.position;

            // Reposition the level so that its Start point aligns with the previous End point
            level.transform.position += spawnOffset;

            // Update the current spawn position to be the End point of this level
            currentSpawnPosition = FindEndPoint(level).position;
        }

        // Generate End Room
        GameObject endRoom = Instantiate(endRoomPrefab, currentSpawnPosition, Quaternion.identity);
        endRoom.name = "EndRoom";
        generatedLevels.Add(endRoom);

        // Get the Start point of the EndRoom and adjust its position
        Transform endRoomStart = FindStartPoint(endRoom);
        Vector3 endRoomOffset = currentSpawnPosition - endRoomStart.position;
        endRoom.transform.position += endRoomOffset;

        Debug.Log($"Generated {generatedLevels.Count} levels: StartRoom + {numberOfLevels} levels + EndRoom");
    }

    // Alternative simpler approach - uncomment this method and call it instead if you prefer
    public void GenerateLevelsSimple()
    {
        ClearLevels();

        if (startRoomPrefab == null || endRoomPrefab == null || levelPrefabs.Count == 0)
        {
            Debug.LogError("Missing required prefabs!");
            return;
        }

        // Start with StartRoom
        GameObject currentLevel = Instantiate(startRoomPrefab, Vector3.zero, Quaternion.identity);
        currentLevel.name = "StartRoom";
        generatedLevels.Add(currentLevel);

        // Add random levels
        for (int i = 0; i < numberOfLevels; i++)
        {
            GameObject randomPrefab = levelPrefabs[Random.Range(0, levelPrefabs.Count)];
            
            // Get the End point of the current level
            Transform currentEnd = FindEndPoint(currentLevel);
            
            // Instantiate new level at the End point position
            GameObject newLevel = Instantiate(randomPrefab, currentEnd.position, Quaternion.identity);
            newLevel.name = $"Level_{i + 1}";
            generatedLevels.Add(newLevel);

            // Get the Start point of the new level
            Transform newLevelStart = FindStartPoint(newLevel);
            
            // Calculate offset and reposition
            Vector3 offset = currentEnd.position - newLevelStart.position;
            newLevel.transform.position += offset;

            currentLevel = newLevel;
        }

        // Add EndRoom
        Transform lastEnd = FindEndPoint(currentLevel);
        GameObject endRoom = Instantiate(endRoomPrefab, lastEnd.position, Quaternion.identity);
        endRoom.name = "EndRoom";
        
        Transform endRoomStart = FindStartPoint(endRoom);
        Vector3 endOffset = lastEnd.position - endRoomStart.position;
        endRoom.transform.position += endOffset;
        
        generatedLevels.Add(endRoom);

        Debug.Log($"Generated {generatedLevels.Count} levels: StartRoom + {numberOfLevels} levels + EndRoom");
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
            
            // Search recursively in children
            Transform found = FindChildByName(child, name);
            if (found != null)
                return found;
        }
        return null;
    }

    public void ClearLevels()
    {
        // Create a list to avoid modification during iteration
        List<GameObject> levelsToDestroy = new List<GameObject>(generatedLevels);
        generatedLevels.Clear();

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
}