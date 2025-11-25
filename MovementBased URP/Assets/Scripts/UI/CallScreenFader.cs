using UnityEngine;

public class CallScreenFader : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private AnimationClip animationClipFadeIn;
    [SerializeField] private AnimationClip animationClipFadeOut;
    [SerializeField] private bool FadeOutOnStart = false;

    void Start()
    {
        if (FadeOutOnStart)
        {
            CallScreenFadeOut();
        }
    }

    public void CallScreenFadeIn()
    {
        animator.SetTrigger("FadeIn");
    }

    public void CallScreenFadeOut()
    {
        animator.SetTrigger("FadeOut");
    }
}
