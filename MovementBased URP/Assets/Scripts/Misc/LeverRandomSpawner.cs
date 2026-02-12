using UnityEngine;
using System.Collections.Generic;

public class LeverRandomSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject leverPrefab;
    public int maxLeversToSpawn = 2;
    [SerializeField] private bool useMaxLeversToSpawnFromPerkManager = false;
    
    [Header("Spawn Points")]
    public GameObject[] spawnPoints;

    private GameObject perkSelectorScriptTarget;
    private PerkSelectorScript perkSelectorScript;
    
    
    private List<GameObject> spawnedLevers = new List<GameObject>();
    
    void Awake()
    {
        if (useMaxLeversToSpawnFromPerkManager)
        {
            FindPerkManager();
        }
        SpawnLevers();
    }
    
    void FindPerkManager()
    {
        perkSelectorScriptTarget = GameObject.Find("PerkSelectorManager");
        perkSelectorScript = perkSelectorScriptTarget.GetComponent<PerkSelectorScript>();

        if (perkSelectorScript != null)
        {
            maxLeversToSpawn = perkSelectorScript.leversToSpawnDefault;
        }
    }

    void SpawnLevers()
    {
        int actualSpawnCount = Mathf.Min(maxLeversToSpawn, spawnPoints.Length);
        
        List<GameObject> selectedSpawnPoints = GetRandomSpawnPoints(actualSpawnCount);
        
        foreach (GameObject spawnPoint in selectedSpawnPoints)
        {
            if (spawnPoint != null)
            {
                GameObject spawnedLever = Instantiate(leverPrefab, spawnPoint.transform.position, spawnPoint.transform.rotation, spawnPoint.transform);
                spawnedLever.name = "Lever_" + spawnPoint.name;
                spawnedLevers.Add(spawnedLever);
            }
        }
    }
    
    List<GameObject> GetRandomSpawnPoints(int count)
    {
        List<GameObject> selectedPoints = new List<GameObject>();
        List<GameObject> availablePoints = new List<GameObject>(spawnPoints);
        
        availablePoints.RemoveAll(point => point == null);
        
        if (count >= availablePoints.Count)
        {
            return availablePoints;
        }
        
        for (int i = 0; i < count; i++)
        {
            if (availablePoints.Count == 0) break;
            
            int randomIndex = Random.Range(0, availablePoints.Count);
            selectedPoints.Add(availablePoints[randomIndex]);
            availablePoints.RemoveAt(randomIndex);
        }
        
        return selectedPoints;
    }
    
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
    
    public void RespawnLevers()
    {
        ClearSpawnedLevers();
        SpawnLevers();
    }
    
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