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
        
        // Spawn levers at selected points
        foreach (GameObject spawnPoint in selectedSpawnPoints)
        {
            if (spawnPoint != null)
            {
                GameObject spawnedLever = Instantiate(leverPrefab, spawnPoint.transform.position, spawnPoint.transform.rotation);
                spawnedLever.name = "Lever_" + spawnPoint.name;
                spawnedLevers.Add(spawnedLever);
                
            }
        }
        
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
}