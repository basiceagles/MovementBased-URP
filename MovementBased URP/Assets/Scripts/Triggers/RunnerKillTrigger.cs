using UnityEngine;
using UnityEngine.Events;
using System;
using System.Collections;
using System.Collections.Generic;

public class RunnerKillTrigger : MonoBehaviour
{
    [Header("Trigger Settings")]
    [SerializeField] bool destroyOnTriggerEvent;
    [SerializeField] bool clearEventsOnTriggerEvent;
    [SerializeField] string tagFilter;
    
    [Header("On Trigger Enter Events")]
    [SerializeField] DelayedEvent[] onTriggerEnterEvents;
    
    [Header("On Trigger Exit Events")]
    [SerializeField] DelayedEvent[] onTriggerExitEvents;
    private GameObject playerTarget;
    private PlayerController playerController;

    void Start()
    {
        playerTarget = GameObject.Find("Player");
        playerController = playerTarget.GetComponent<PlayerController>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (!String.IsNullOrEmpty(tagFilter) && !other.gameObject.CompareTag(tagFilter)) return;

        if (playerController.isSafeFromRunner) return;
        
        ExecuteEventsWithDelay(onTriggerEnterEvents);
        
        if (destroyOnTriggerEvent)
        {
            Destroy(gameObject);
        }
        if (clearEventsOnTriggerEvent)
        {
            ClearAllEvents();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (!String.IsNullOrEmpty(tagFilter) && !other.gameObject.CompareTag(tagFilter)) return;
        
        ExecuteEventsWithDelay(onTriggerExitEvents);
    }

    void ExecuteEventsWithDelay(DelayedEvent[] events)
    {
        foreach (DelayedEvent delayedEvent in events)
        {
            if (delayedEvent.unityEvent != null)
            {
                StartCoroutine(ExecuteSingleEvent(delayedEvent.unityEvent, delayedEvent.delay));
            }
        }
    }

    IEnumerator ExecuteSingleEvent(UnityEvent unityEvent, float delay)
    {
        if (delay > 0)
        {
            yield return new WaitForSeconds(delay);
        }

        unityEvent.Invoke();
    }
    
    public void ClearAllEvents()
    {
        onTriggerEnterEvents = new DelayedEvent[0];
        onTriggerExitEvents = new DelayedEvent[0];
    }
}