using UnityEngine;
using System.Collections.Generic;

public class LeverRandomSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject leverPrefab;
    public int maxLeversToSpawn = 2;
    
    [Header("Spawn Points")]
    public GameObject[] spawnPoints;
    
    
    private List<GameObject> spawnedLevers = new List<GameObject>();
    
    void Awake()
    {
        SpawnLevers();
    }
    
    void SpawnLevers()
    {
        // Calculate how many levers we can actually spawn
        int actualSpawnCount = Mathf.Min(maxLeversToSpawn, spawnPoints.Length);
        
        // If we have more spawn points than max levers, select random points
        List<GameObject> selectedSpawnPoints = GetRandomSpawnPoints(actualSpawnCount);
        
        // Spawn levers at selected points as children
        foreach (GameObject spawnPoint in selectedSpawnPoints)
        {
            if (spawnPoint != null)
            {
                // Spawn lever as child of the spawn point
                GameObject spawnedLever = Instantiate(leverPrefab, spawnPoint.transform.position, spawnPoint.transform.rotation, spawnPoint.transform);
                spawnedLever.name = "Lever_" + spawnPoint.name;
                spawnedLevers.Add(spawnedLever);
                
                // Debug to verify parenting
                Debug.Log($"Spawned lever '{spawnedLever.name}' as child of '{spawnPoint.name}'");
            }
        }
        
        Debug.Log($"Spawned {spawnedLevers.Count} levers as children of spawn points");
    }
    
    List<GameObject> GetRandomSpawnPoints(int count)
    {
        List<GameObject> selectedPoints = new List<GameObject>();
        List<GameObject> availablePoints = new List<GameObject>(spawnPoints);
        
        // Remove any null spawn points
        availablePoints.RemoveAll(point => point == null);
        
        // If we want more points than available, just return all available
        if (count >= availablePoints.Count)
        {
            return availablePoints;
        }
        
        // Randomly select spawn points
        for (int i = 0; i < count; i++)
        {
            if (availablePoints.Count == 0) break;
            
            int randomIndex = Random.Range(0, availablePoints.Count);
            selectedPoints.Add(availablePoints[randomIndex]);
            availablePoints.RemoveAt(randomIndex);
        }
        
        return selectedPoints;
    }
    
    // Method to clear spawned levers
    public void ClearSpawnedLevers()
    {
        foreach (GameObject lever in spawnedLevers)
        {
            if (lever != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(lever);
                }
                else
                {
                    DestroyImmediate(lever);
                }
            }
        }
        spawnedLevers.Clear();
    }
    
    // Method to respawn levers
    public void RespawnLevers()
    {
        ClearSpawnedLevers();
        SpawnLevers();
    }
    
    // Editor convenience methods
    [ContextMenu("Spawn Levers Now")]
    private void SpawnLeversEditor()
    {
        SpawnLevers();
    }
    
    [ContextMenu("Clear Spawned Levers")]
    private void ClearLeversEditor()
    {
        ClearSpawnedLevers();
    }
    
    [ContextMenu("Respawn Levers")]
    private void RespawnLeversEditor()
    {
        RespawnLevers();
    }
}