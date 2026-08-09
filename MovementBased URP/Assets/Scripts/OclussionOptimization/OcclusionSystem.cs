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
        occludableObjects.Clear();
        GameObject[] foundObjects = GameObject.FindGameObjectsWithTag("StaticForOcclusion");
        if (foundObjects != null)
        {
            occludableObjects.AddRange(foundObjects);
        }
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
        float maxDistSqr = maxDistance * maxDistance;
        Vector3 currentPos = transform.position;

        for (int i = occludableObjects.Count - 1; i >= 0; i--)
        {
            GameObject obj = occludableObjects[i];
            if (obj == null)
            {
                occludableObjects.RemoveAt(i);
                continue;
            }
            
            float distSqr = (currentPos - obj.transform.position).sqrMagnitude;
            
            if (distSqr > maxDistSqr)
            {
                if (obj.activeSelf)
                {
                    obj.SetActive(false);
                }
            }
            else
            {
                if (!obj.activeSelf)
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