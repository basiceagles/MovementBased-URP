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

        if (perkSelectorScript.spawnNodes && nodeSpawner)
        {
            SpawnObjects();
        }

        if (redEyeSpawner && spawnBlueEyeSpawner)
        {
            if (perkSelectorScript.spawnRedEye || perkSelectorScript.spawnBlueEye)
            {
                SpawnObjects();
            }
        }
        else
        {
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

    public void SpawnObjects()
    {
        if (objectPrefab == null)
        {
            return;
        }

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            return;
        }

        int actualSpawnCount = Mathf.Min(maxObjectsToSpawn, spawnPoints.Length);

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

            int randomIndex = Random.Range(0, availableSpawnPoints.Count);
            Transform spawnPoint = availableSpawnPoints[randomIndex];

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

            if (allowRed && allowBlue)
            {
                if (redEyePrefab != null)
                {
                    float redRoll = Random.Range(0f, 100f);
                    if (redRoll <= perkSelectorScript.redEyeSpawnChance)
                    {
                        prefabToSpawn = redEyePrefab;
                        shouldSpawn = true;
                    }
                }

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
                float randomValue = Random.Range(0f, 100f);
                if (randomValue <= spawnChance)
                {
                    shouldSpawn = true;
                }
            }

            if (!shouldSpawn)
            {
                continue; 
            }

            GameObject spawnedObject = Instantiate(
                prefabToSpawn,
                spawnPoint.position, 
                spawnPoint.rotation
            );

            if (parentToSpawnPoint)
            {
                spawnedObject.transform.SetParent(spawnPoint);
            }

            spawnedObjects.Add(spawnedObject);

            if (avoidOverlappingSpawns)
            {
                availableSpawnPoints.RemoveAt(randomIndex);
            }
        }
    }

    public void SpawnObjects(GameObject customPrefab, int customMaxObjects)
    {
        objectPrefab = customPrefab;
        maxObjectsToSpawn = customMaxObjects;
        SpawnObjects();
    }

    public void SpawnObjects(GameObject customPrefab, int customMaxObjects, float customSpawnChance)
    {
        objectPrefab = customPrefab;
        maxObjectsToSpawn = customMaxObjects;
        spawnChance = customSpawnChance;
        SpawnObjects();
    }

    public void SetSpawnChance(float chance)
    {
        spawnChance = Mathf.Clamp(chance, 0f, 100f);
    }

    public float GetSpawnChance()
    {
        return spawnChance;
    }
}