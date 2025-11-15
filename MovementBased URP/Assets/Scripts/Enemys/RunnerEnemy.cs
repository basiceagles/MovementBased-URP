using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RunnerEnemy : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 3f;
    public float searchRadius = 5f;
    public string waypointTag = "Waypoint";
    public string endWaypointName = "WaypointEnd";

    [Header("Debug")]
    public bool showGizmos = true;
    public Color gizmoColor = Color.red;

    public Transform currentWaypoint;
    public Transform previousWaypoint;
    public bool isMoving = false;

    private List<Transform> visitedWaypoints = new List<Transform>();

    void Start()
    {
        Invoke("FindNextWaypoint", 0.1f);
    }

    void Update()
    {
        if (currentWaypoint != null && isMoving)
        {
            MoveTowardsWaypoint();
        }
    }

    void FindNextWaypoint()
    {
        // Find all waypoints within search radius
        Collider[] collidersInRange = Physics.OverlapSphere(transform.position, searchRadius);
        Transform closestWaypoint = null;
        float closestDistance = Mathf.Infinity;

        foreach (Collider collider in collidersInRange)
        {
            if (collider.CompareTag(waypointTag))
            {
                Transform waypoint = collider.transform;
                float distance = Vector3.Distance(transform.position, waypoint.position);
                
                // Skip if this is our current waypoint
                if (waypoint == currentWaypoint)
                    continue;

                // Skip if this is our previous waypoint
                if (waypoint == previousWaypoint)
                    continue;

                // Skip if we've already visited this waypoint
                if (visitedWaypoints.Contains(waypoint))
                    continue;

                // Find the closest waypoint
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestWaypoint = waypoint;
                }
            }
        }

        if (closestWaypoint != null)
        {
            // Update previous waypoint before setting new current
            if (currentWaypoint != null)
            {
                previousWaypoint = currentWaypoint;
                visitedWaypoints.Add(previousWaypoint);
            }

            currentWaypoint = closestWaypoint;
            isMoving = true;
            Debug.Log("Moving to waypoint: " + currentWaypoint.name);
        }
        else
        {
            // If no waypoints found, stop moving
            Debug.LogWarning("No waypoints found in radius!");
            isMoving = false;
        }
    }

    void MoveTowardsWaypoint()
    {
        // Move towards the current waypoint
        Vector3 direction = (currentWaypoint.position - transform.position).normalized;
        transform.position += direction * moveSpeed * Time.deltaTime;

        // Rotate to face the waypoint (optional)
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction) * Quaternion.Euler(0, -90, 0);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
        }

        // Check if reached the waypoint
        float distanceToWaypoint = Vector3.Distance(transform.position, currentWaypoint.position);
        if (distanceToWaypoint < 0.5f)
        {
            OnReachedWaypoint();
        }
    }

    void OnReachedWaypoint()
    {
        Debug.Log("Reached waypoint: " + currentWaypoint.name);

        // Check if this is the end waypoint
        if (currentWaypoint.name == endWaypointName)
        {
            AudioSource audioSource;
            audioSource = currentWaypoint.GetComponent<AudioSource>();
            audioSource.Play();

            CallCameraShake callCameraShake;
            callCameraShake = currentWaypoint.GetComponent<CallCameraShake>();
            callCameraShake.CallCameraShaker();

            DestroyEnemy();
        }
        else
        {
            // Find next waypoint
            FindNextWaypoint();
        }
    }

    void DestroyEnemy()
    {
        Debug.Log("Reached end waypoint. Destroying enemy.");
        Destroy(gameObject);
        
    }

    // Public method to manually set waypoints (useful for testing)
    public void SetWaypoint(Transform waypoint)
    {
        if (currentWaypoint != null)
        {
            previousWaypoint = currentWaypoint;
            visitedWaypoints.Add(previousWaypoint);
        }
        currentWaypoint = waypoint;
        isMoving = true;
    }

    // Public method to clear visited waypoints (if you want to reset path)
    public void ClearVisitedWaypoints()
    {
        visitedWaypoints.Clear();
        previousWaypoint = null;
    }

    void OnDrawGizmos()
    {
        if (!showGizmos) return;

        // Draw search radius
        Gizmos.color = gizmoColor;
        Gizmos.DrawWireSphere(transform.position, searchRadius);

        // Draw line to current waypoint if moving
        if (currentWaypoint != null && isMoving)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, currentWaypoint.position);
        }

        // Draw line to previous waypoint if exists
        if (previousWaypoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, previousWaypoint.position);
        }
    }

    void OnDrawGizmosSelected()
    {
        if (!showGizmos) return;

        // Draw filled sphere when selected
        Gizmos.color = new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, 0.3f);
        Gizmos.DrawSphere(transform.position, searchRadius);

        // Draw visited waypoints
        Gizmos.color = Color.red;
        foreach (Transform visited in visitedWaypoints)
        {
            if (visited != null)
            {
                Gizmos.DrawWireCube(visited.position, Vector3.one * 0.3f);
            }
        }
    }
}