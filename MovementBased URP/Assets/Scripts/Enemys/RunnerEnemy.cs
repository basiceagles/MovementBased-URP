using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

public class RunnerEnemy : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 3f;
    public float searchRadius = 5f;
    public string waypointTag = "Waypoint";
    public string endWaypointName = "WaypointEnd";
    [SerializeField] private GameObject runnerPrefab;

    public Transform currentWaypoint;
    public Transform previousWaypoint;
    public bool isMoving = false;
    private bool alreadyReached = false;

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
        Collider[] collidersInRange = Physics.OverlapSphere(transform.position, searchRadius);
        Transform closestWaypoint = null;
        float closestDistance = Mathf.Infinity;

        foreach (Collider collider in collidersInRange)
        {
            if (collider.CompareTag(waypointTag))
            {
                Transform waypoint = collider.transform;
                float distance = Vector3.Distance(transform.position, waypoint.position);
                
                if (waypoint == currentWaypoint)
                    continue;

                if (waypoint == previousWaypoint)
                    continue;

                if (visitedWaypoints.Contains(waypoint))
                    continue;

                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestWaypoint = waypoint;
                }
            }
        }

        if (closestWaypoint != null)
        {
            if (currentWaypoint != null)
            {
                previousWaypoint = currentWaypoint;
                visitedWaypoints.Add(previousWaypoint);
            }

            currentWaypoint = closestWaypoint;
            isMoving = true;
        }
        else
        {
            isMoving = false;
        }
    }

    void MoveTowardsWaypoint()
    {
        Vector3 direction = (currentWaypoint.position - transform.position).normalized;
        transform.position += direction * moveSpeed * Time.deltaTime;

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction) * Quaternion.Euler(0, -90, 0);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
        }

        float distanceToWaypoint = Vector3.Distance(transform.position, currentWaypoint.position);
        if (distanceToWaypoint < 0.5f)
        {
            OnReachedWaypoint();
        }
    }

    void OnReachedWaypoint()
    {
        if (currentWaypoint.name == endWaypointName)
        {
            if (alreadyReached) return;

            AudioSource audioSource;
            audioSource = currentWaypoint.GetComponent<AudioSource>();
            audioSource.Play();

            CallCameraShake callCameraShake;
            callCameraShake = currentWaypoint.GetComponent<CallCameraShake>();
            callCameraShake.CallHeavyCameraShaker();

            OpenDoor openDoor;
            openDoor = currentWaypoint.GetComponent<OpenDoor>();
            if (openDoor != null) openDoor.CallOpenDoor();

            DestroyEnemy();

            alreadyReached = true;
        }
        else
        {
            FindNextWaypoint();
        }
    }

    void DestroyEnemy()
    {
        Destroy(gameObject);
    }

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

    public void ClearVisitedWaypoints()
    {
        visitedWaypoints.Clear();
        previousWaypoint = null;
    }

    private void OnDestroy()
    {
        CameraShake[] shakers = FindObjectsOfType<CameraShake>();
        foreach (CameraShake shaker in shakers)
        {
            if (shaker != null)
            {
                shaker.ConstantlyShakeStop();
                shaker.ConstantlyShakeStopSmoothly();
                shaker.StopShake();
            }
        }
    }
}