using UnityEngine;
using UnityEngine.Events;
using System.Collections;

[System.Serializable]
public class TimedEvent
{
    public UnityEvent unityEvent;
    public float delay = 0f;
}

public class DelayedEvents : MonoBehaviour
{
    [SerializeField] private TimedEvent[] events;
    [SerializeField] private bool callOnStart = true;

    void Start()
    {
        if (callOnStart && events != null)
        {
            for (int i = 0; i < events.Length; i++)
            {
                if (events[i] != null && events[i].unityEvent != null)
                {
                    StartCoroutine(ExecuteEvent(events[i].unityEvent, events[i].delay));
                }
            }
        }
    }

    private IEnumerator ExecuteEvent(UnityEvent unityEvent, float delay)
    {
        if (delay > 0f)
        {
            yield return new WaitForSeconds(delay);
        }
        unityEvent?.Invoke();
    }

    public void CallDelayedEvent()
    {
        if (events == null) return;

        for (int i = 0; i < events.Length; i++)
        {
            if (events[i] != null && events[i].unityEvent != null)
            {
                StartCoroutine(ExecuteEvent(events[i].unityEvent, events[i].delay));
            }
        }
    }
}