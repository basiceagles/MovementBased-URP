using UnityEngine;

public class TriggerActions : MonoBehaviour
{
    [Header("Animation Call Settings")]
    [SerializeField] private AnimationClip animationClip;
    [SerializeField] private Animator animator;

    [Header("Destroy Settings")]
    [SerializeField] private GameObject objectToDestroy;
    [SerializeField] private float destroyDelay = 5f;

    [Header("Follow Settings")]
    [SerializeField] private GameObject followTargetObject;

    private void Start()
    {
        if (followTargetObject == null)
        {
            followTargetObject = GameObject.Find("RunnerParent");
        }
    }

    private void Update()
    {
        if (followTargetObject != null)
        {
            transform.position = followTargetObject.transform.position;
        }
    }

    public void PlayAnimation()
    {
        if (animator != null && animationClip != null)
        {
            animator.Play(animationClip.name);
        }
    }

    public void CallDestroyAfterTime()
    {
        if (objectToDestroy != null)
        {
            Destroy(objectToDestroy, destroyDelay);
        }
    }

    public void LeaveTheGame()
    {
        Application.Quit();
    }
}
