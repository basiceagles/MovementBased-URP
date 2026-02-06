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
    [Header("Spawn Type Settings!")]
    [SerializeField] private bool redEyeSpawner = false;
    [SerializeField] private bool spawnBlueEyeSpawner = false;
    [SerializeField] private bool nodeSpawner = false;

    [Header("Red Eye Settings")]
    [SerializeField] private GameObject redEyePrefab;

    [Header("Blue Eye Settings")]
    [SerializeField] private GameObject blueEyePrefab;

    private PerkSelectorScript perkSelectorScript;
    private GameObject perkSelectorScriptTarget;

    private List<GameObject> spawnedObjects = new List<GameObject>();

    void Awake()
    {
        perkSelectorScriptTarget = GameObject.Find("PerkSelectorManager");
        perkSelectorScript = perkSelectorScriptTarget.GetComponent<PerkSelectorScript>();

        // Configure prefab and spawn chance based on spawner type
        if (redEyeSpawner)
        {
            if (redEyePrefab != null)
            {
                objectPrefab = redEyePrefab;
            }
            spawnChance = perkSelectorScript.redEyeSpawnChance;
        }

        if (spawnBlueEyeSpawner)
        {
            if (blueEyePrefab != null)
            {
                objectPrefab = blueEyePrefab;
            }
            spawnChance = perkSelectorScript.blueEyeSpawnChance;
        }

        if (nodeSpawner)
        {
            spawnChance = perkSelectorScript.nodeSpawnChance;
        }

        // Node spawning stays independent
        if (perkSelectorScript.spawnNodes && nodeSpawner)
        {
            SpawnObjects();
        }

        // If this spawner is configured to handle BOTH RedEye and BlueEye,
        // call SpawnObjects only once and let the mixed logic inside decide
        if (redEyeSpawner && spawnBlueEyeSpawner)
        {
            if (perkSelectorScript.spawnRedEye || perkSelectorScript.spawnBlueEye)
            {
                SpawnObjects();
            }
        }
        else
        {
            // Separate RedEye-only and BlueEye-only spawners keep their own calls
            if (perkSelectorScript.spawnRedEye && redEyeSpawner)
            {
                SpawnObjects();
            }

            if (perkSelectorScript.spawnBlueEye && spawnBlueEyeSpawner)
            {
                SpawnObjects();
            }
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

        // Calculate how many objects we can actually spawn this call
        int actualSpawnCount = Mathf.Min(maxObjectsToSpawn, spawnPoints.Length);

        // Create a list of available spawn points that have not reached their max children
        List<Transform> availableSpawnPoints = new List<Transform>();
        foreach (Transform sp in spawnPoints)
        {
            if (sp == null) continue;
            
            int currentChildren = parentToSpawnPoint ? sp.childCount : 0;
            if (currentChildren < maxObjectsToSpawn)
            {
                availableSpawnPoints.Add(sp);
            }
        }

        for (int i = 0; i < actualSpawnCount; i++)
        {
            if (availableSpawnPoints.Count == 0) break;

            // Pick a random spawn point from available ones
            int randomIndex = Random.Range(0, availableSpawnPoints.Count);
            Transform spawnPoint = availableSpawnPoints[randomIndex];

            // Double-check the current number of children on this spawn point
            int currentChildren = parentToSpawnPoint ? spawnPoint.childCount : 0;
            if (currentChildren >= maxObjectsToSpawn)
            {
                availableSpawnPoints.RemoveAt(randomIndex);
                i--;
                continue;
            }

            GameObject prefabToSpawn = objectPrefab;
            bool shouldSpawn = false;

            bool allowRed = redEyeSpawner && perkSelectorScript.spawnRedEye;
            bool allowBlue = spawnBlueEyeSpawner && perkSelectorScript.spawnBlueEye;

            // Mixed RedEye/BlueEye logic: when both types are allowed by perks
            if (allowRed && allowBlue)
            {
                // First try RedEye
                if (redEyePrefab != null)
                {
                    float redRoll = Random.Range(0f, 100f);
                    if (redRoll <= perkSelectorScript.redEyeSpawnChance)
                    {
                        prefabToSpawn = redEyePrefab;
                        shouldSpawn = true;
                    }
                }

                // If RedEye did not spawn, try BlueEye
                if (!shouldSpawn && blueEyePrefab != null)
                {
                    float blueRoll = Random.Range(0f, 100f);
                    if (blueRoll <= perkSelectorScript.blueEyeSpawnChance)
                    {
                        prefabToSpawn = blueEyePrefab;
                        shouldSpawn = true;
                    }
                }
            }
            else if (allowRed)
            {
                // Only RedEye is allowed
                if (redEyePrefab != null)
                {
                    float redRoll = Random.Range(0f, 100f);
                    if (redRoll <= perkSelectorScript.redEyeSpawnChance)
                    {
                        prefabToSpawn = redEyePrefab;
                        shouldSpawn = true;
                    }
                }
            }
            else if (allowBlue)
            {
                // Only BlueEye is allowed
                if (blueEyePrefab != null)
                {
                    float blueRoll = Random.Range(0f, 100f);
                    if (blueRoll <= perkSelectorScript.blueEyeSpawnChance)
                    {
                        prefabToSpawn = blueEyePrefab;
                        shouldSpawn = true;
                    }
                }
            }
            else
            {
                // Fallback: original single-type behavior using spawnChance and objectPrefab
                float randomValue = Random.Range(0f, 100f);
                if (randomValue <= spawnChance)
                {
                    shouldSpawn = true;
                }
            }

            if (!shouldSpawn)
            {
                continue; // Skip this spawn based on chance logic
            }

            // Spawn the object
            GameObject spawnedObject = Instantiate(
                prefabToSpawn,
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