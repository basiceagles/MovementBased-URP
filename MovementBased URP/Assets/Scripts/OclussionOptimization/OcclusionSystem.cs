using UnityEngine;
using System.Collections.Generic;

public class OcclusionSystem : MonoBehaviour
{
    [Header("Occlusion Settings")]
    public float maxDistance = 50f;
    public float checkInterval = 0.5f;
    public LayerMask occlusionLayers = -1;
    
    private List<GameObject> occludableObjects = new List<GameObject>();
    private float checkTimer = 0f;

    void Start()
    {
        Invoke("FindOccludableObjects", 0.1f);
    }

    private void OnEnable()
    {
        LevelGenerator.RestartGenerationEvent += FindOccludableObjects;
    }
    private void OnDisable()
    {
        LevelGenerator.RestartGenerationEvent -= FindOccludableObjects;
    }

    void FindOccludableObjects()
    {
        GameObject[] foundObjects = GameObject.FindGameObjectsWithTag("StaticForOcclusion");
        occludableObjects.AddRange(foundObjects);
    }

    void Update()
    {
        checkTimer += Time.deltaTime;
        
        if (checkTimer >= checkInterval)
        {
            UpdateOcclusion();
            checkTimer = 0f;
        }
    }

    void UpdateOcclusion()
    {
        foreach (GameObject obj in occludableObjects)
        {
            if (obj == null) continue;
            
            float distance = Vector3.Distance(transform.position, obj.transform.position);
            
            if (distance > maxDistance)
            {
                if (obj.activeInHierarchy)
                {
                    obj.SetActive(false);
                }
            }
            else
            {
                if (!obj.activeInHierarchy)
                {
                    obj.SetActive(true);
                }
            }
        }
    }

    public void AddOccludableObject(GameObject obj)
    {
        if (!occludableObjects.Contains(obj))
        {
            occludableObjects.Add(obj);
        }
    }

    public void RemoveOccludableObject(GameObject obj)
    {
        if (occludableObjects.Contains(obj))
        {
            occludableObjects.Remove(obj);
        }
    }
}