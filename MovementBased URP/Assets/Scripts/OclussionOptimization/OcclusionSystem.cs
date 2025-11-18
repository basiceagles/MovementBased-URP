using UnityEngine;
using System.Collections.Generic;

public class OcclusionSystem : MonoBehaviour
{
    [Header("Occlusion Settings")]
    public float maxDistance = 50f;
    public float checkInterval = 0.5f;
    public LayerMask occlusionLayers = -1; // Which layers to occlude
    
    [Header("Debug")]
    public bool showDebugGizmos = true;
    
    private List<GameObject> occludableObjects = new List<GameObject>();
    private float checkTimer = 0f;

    void Start()
    {
        // Find all objects with the Occludable tag
        GameObject[] foundObjects = GameObject.FindGameObjectsWithTag("StaticForOcclusion");
        occludableObjects.AddRange(foundObjects);
        
        Debug.Log($"Found {occludableObjects.Count} occludable objects");
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
                // Object is too far - disable it
                if (obj.activeInHierarchy)
                {
                    obj.SetActive(false);
                }
            }
            else
            {
                // Object is within range - enable it
                if (!obj.activeInHierarchy)
                {
                    obj.SetActive(true);
                }
            }
        }
    }

    // Manually add objects to the occlusion system
    public void AddOccludableObject(GameObject obj)
    {
        if (!occludableObjects.Contains(obj))
        {
            occludableObjects.Add(obj);
        }
    }

    // Manually remove objects from the occlusion system
    public void RemoveOccludableObject(GameObject obj)
    {
        if (occludableObjects.Contains(obj))
        {
            occludableObjects.Remove(obj);
        }
    }

    // Draw debug gizmos in the scene view
    void OnDrawGizmosSelected()
    {
        if (!showDebugGizmos) return;
        
        // Draw occlusion radius
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, maxDistance);
        
        // Draw lines to occluded objects
        Gizmos.color = Color.red;
        foreach (GameObject obj in occludableObjects)
        {
            if (obj != null && !obj.activeInHierarchy)
            {
                Gizmos.DrawLine(transform.position, obj.transform.position);
            }
        }
    }
}