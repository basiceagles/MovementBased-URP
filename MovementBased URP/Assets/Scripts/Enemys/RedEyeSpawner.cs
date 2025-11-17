using UnityEngine;
using System.Collections.Generic;

public class RandomSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private GameObject objectPrefab;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private int maxObjectsToSpawn = 5;
    [SerializeField] private bool spawnOnStart = true;
    [SerializeField] [Range(0, 100)] private float spawnChance = 100f;

    [Header("Advanced Settings")]
    [SerializeField] private bool parentToSpawnPoint = true;
    [SerializeField] private bool avoidOverlappingSpawns = true;
    [Header("Spawn Type Settings")]
    [SerializeField] private bool redEyeSpawner = false;
    [SerializeField] private bool nodeSpawner = false;

    private PerkSelectorScript perkSelectorScript;
    private GameObject perkSelectorScriptTarget;

    private List<GameObject> spawnedObjects = new List<GameObject>();

    void Awake()
    {
        perkSelectorScriptTarget = GameObject.Find("PerkSelectorManager");
        perkSelectorScript = perkSelectorScriptTarget.GetComponent<PerkSelectorScript>();

        if (redEyeSpawner) spawnChance = perkSelectorScript.redEyeSpawnChance;
        if (nodeSpawner) spawnChance = perkSelectorScript.nodeSpawnChance;

        if (perkSelectorScript.spawnNodes && nodeSpawner)
        {
            SpawnObjects();
        }

        if (perkSelectorScript.spawnRedEye && redEyeSpawner)
        {
            SpawnObjects();
        }
    }

    /// <summary>
    /// Spawns random objects on spawn points
    /// </summary>
    public void SpawnObjects()
    {
        if (objectPrefab == null)
        {
            Debug.LogWarning("Object prefab is not assigned!");
            return;
        }

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogWarning("No spawn points assigned!");
            return;
        }

        // Calculate how many objects we can actually spawn
        int actualSpawnCount = Mathf.Min(maxObjectsToSpawn, spawnPoints.Length);

        // Create a list of available spawn points
        List<Transform> availableSpawnPoints = new List<Transform>(spawnPoints);

        for (int i = 0; i < actualSpawnCount; i++)
        {
            if (availableSpawnPoints.Count == 0) break;

            // Check spawn chance
            float randomValue = Random.Range(0f, 100f);
            if (randomValue > spawnChance)
            {
                continue; // Skip this spawn based on chance
            }

            // Pick a random spawn point from available ones
            int randomIndex = Random.Range(0, availableSpawnPoints.Count);
            Transform spawnPoint = availableSpawnPoints[randomIndex];

            // Spawn the object
            GameObject spawnedObject = Instantiate(
                objectPrefab, 
                spawnPoint.position, 
                spawnPoint.rotation
            );

            // Parent to spawn point if enabled
            if (parentToSpawnPoint)
            {
                spawnedObject.transform.SetParent(spawnPoint);
            }

            spawnedObjects.Add(spawnedObject);

            // Remove this spawn point from available list to avoid overlapping spawns
            if (avoidOverlappingSpawns)
            {
                availableSpawnPoints.RemoveAt(randomIndex);
            }
        }

        Debug.Log($"Spawned {spawnedObjects.Count} objects (Chance: {spawnChance}%)");
    }

    /// <summary>
    /// Spawns objects with custom parameters
    /// </summary>
    public void SpawnObjects(GameObject customPrefab, int customMaxObjects)
    {
        objectPrefab = customPrefab;
        maxObjectsToSpawn = customMaxObjects;
        SpawnObjects();
    }

    /// <summary>
    /// Spawns objects with custom parameters including spawn chance
    /// </summary>
    public void SpawnObjects(GameObject customPrefab, int customMaxObjects, float customSpawnChance)
    {
        objectPrefab = customPrefab;
        maxObjectsToSpawn = customMaxObjects;
        spawnChance = customSpawnChance;
        SpawnObjects();
    }

    /// <summary>
    /// Sets the spawn chance percentage
    /// </summary>
    public void SetSpawnChance(float chance)
    {
        spawnChance = Mathf.Clamp(chance, 0f, 100f);
    }

    /// <summary>
    /// Gets the current spawn chance percentage
    /// </summary>
    public float GetSpawnChance()
    {
        return spawnChance;
    }
}