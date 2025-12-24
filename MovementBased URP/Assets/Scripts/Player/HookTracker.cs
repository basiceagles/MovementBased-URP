using System.Collections.Generic;
using UnityEngine;

public class HookTracker : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Радиус")]
    [SerializeField] private float activationRadius = 10f;
    
    [Tooltip("UI Tip")]
    [SerializeField] private GameObject hookTarget;
    
    [Header("Debug")]
    [SerializeField] private bool showGizmos = true;
    
    private Transform player;
    private Transform cam;
    private Grappling grapplingScript;
    private Swinging swingingScript;
    private PlayerController pc;
    private bool isActive = false;
    private bool isPriority = false; 
    private bool wasGrapplingActive = false;
    private bool wasSwingingActive = false;
    private static List<HookTracker> allTrackers = new List<HookTracker>();
    
    private void Awake()
    {
        if (!allTrackers.Contains(this))
        {
            allTrackers.Add(this);
        }
    }
    
    private void OnDestroy()
    {
        allTrackers.Remove(this);
    }
    
    private void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            cam = Camera.main?.transform;
            grapplingScript = playerObj.GetComponentInChildren<Grappling>();
            swingingScript = playerObj.GetComponentInChildren<Swinging>();
            pc = playerObj.GetComponent<PlayerController>();
        }
        if (hookTarget != null)
        {
            hookTarget.SetActive(false);
        }
    }
    
    private void Update()
    {
        if (player == null || hookTarget == null)
            return;
        
        float distance = Vector3.Distance(transform.position, player.position);
        bool shouldBeActive = distance <= activationRadius;
        
        if (shouldBeActive != isActive)
        {
            isActive = shouldBeActive;
            hookTarget.SetActive(isActive);
        }
        
        if (!IsAnyHookActive())
        {
            UpdatePriority();
        }
    }
    
    private bool IsAnyHookActive()
    {
        if (grapplingScript != null && grapplingScript.IsGrapplingActive())
        {
            return true;
        }
        
        if (swingingScript != null && swingingScript.IsSwinging())
        {
            return true;
        }
        
        return false;
    }
    
    private void UpdatePriority()
    {
        if (cam == null || hookTarget == null || !hookTarget.activeInHierarchy)
        {
            isPriority = false;
            return;
        }
        
        List<HookTracker> activeTrackers = new List<HookTracker>();
        foreach (var tracker in allTrackers)
        {
            if (tracker != null && tracker.hookTarget != null && tracker.hookTarget.activeInHierarchy)
            {
                activeTrackers.Add(tracker);
            }
        }
        
        if (activeTrackers.Count == 0)
        {
            isPriority = false;
            return;
        }
        
        if (activeTrackers.Count == 1)
        {
            isPriority = (activeTrackers[0] == this);
            return;
        }
        
        HookTracker bestTracker = null;
        float bestDot = -1f;
        
        foreach (var tracker in activeTrackers)
        {
            Vector3 dirToTarget = (tracker.hookTarget.transform.position - cam.position).normalized;
            float dot = Vector3.Dot(cam.forward, dirToTarget);
            
            if (dot > bestDot)
            {
                bestDot = dot;
                bestTracker = tracker;
            }
        }
        
        isPriority = (bestTracker == this);
    }
    
    private void LateUpdate()
    {
        bool anyMagnetActive = false;
        foreach (var tracker in allTrackers)
        {
            if (tracker != null && tracker.isActive && tracker.hookTarget != null && tracker.hookTarget.activeInHierarchy)
            {
                anyMagnetActive = true;
                break;
            }
        }
        
        if (swingingScript != null)
        {
            swingingScript.bypassLayerCheck = anyMagnetActive;
        }
        
        if (isPriority && hookTarget != null)
        {
            OverrideHookPoints();
            ForceHookFunctionality();
        }
    }
    
    private void OverrideHookPoints()
    {
        Vector3 targetPosition = hookTarget.transform.position;
        
        if (grapplingScript != null && grapplingScript.IsGrapplingActive())
        {
            grapplingScript.SetGrapplePoint(targetPosition);
        }
        
        if (swingingScript != null && swingingScript.IsSwinging())
        {
            swingingScript.SetSwingPoint(targetPosition);
        }
    }
    
    private void ForceHookFunctionality()
    {
        if (grapplingScript != null && pc != null)
        {
            bool isGrappling = grapplingScript.IsGrapplingActive();
            
            if (isGrappling && !wasGrapplingActive)
            {
                grapplingScript.ForceActivatePulling();
                pc.activeGrappling = true;
            }
            
            wasGrapplingActive = isGrappling;
        }
        
        if (swingingScript != null && pc != null)
        {
            bool isSwinging = swingingScript.IsSwinging();
            
            if (isSwinging && !wasSwingingActive)
            {
                swingingScript.ForceActivateSwinging();
                pc.activeGrappling = true;
            }
            
            wasSwingingActive = isSwinging;
        }
    }
    
    private void OnDrawGizmos()
    {
        if (!showGizmos)
        {
            return;
        }
        
        if (isPriority)
        {
            Gizmos.color = Color.green;
        }
        else if (isActive)
        {
            Gizmos.color = Color.yellow;
        }
        else
        {
            Gizmos.color = Color.gray;
        }
            
        Gizmos.DrawWireSphere(transform.position, activationRadius);
        
        if (hookTarget != null)
        {
            Gizmos.color = isPriority ? Color.cyan : Color.gray;
            Gizmos.DrawLine(transform.position, hookTarget.transform.position);
            Gizmos.DrawWireSphere(hookTarget.transform.position, 0.5f);
        }
    }
}

