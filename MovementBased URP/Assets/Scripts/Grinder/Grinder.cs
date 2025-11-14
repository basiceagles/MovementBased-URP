using JetBrains.Annotations;
using UnityEngine;

public class Grinder : MonoBehaviour
{
    [Header("Animations")]
    [SerializeField] private AnimationClip enterAnimation;
    [SerializeField] private AnimationClip exitAnimation;
    [SerializeField] private AnimationClip insertAnimation;
    public Animator animator;
    [Header("Animation Settings")]
    [SerializeField] private string playerTag = "Player";
    [Header("Tool Tip Settings")]
    [SerializeField] private GameObject toolTipUI;
    private GameObject targetTimerManager;
    private Timer targetTimer;
    public int timeToAdd = 15;
    [Header("Audio Sources")]
    public AudioSource openAudioSource;
    public AudioSource closeAudioSource;
    public AudioSource grinderAudioSource;
    public AudioSource insertAudioSource;

    void Start()
    {
        targetTimerManager = GameObject.Find("TimerManager");
        if (targetTimerManager != null) targetTimer = targetTimerManager.GetComponent<Timer>();
        else Debug.LogWarning("TimerManager is not set!");
    }
    public void PlayOpenAnimation()
    {
        PlayAnimation(enterAnimation.name);
        openAudioSource.Play();
        grinderAudioSource.Play();
    }

    public void PlayCloseAnimation()
    {
        PlayAnimation(exitAnimation.name);
        closeAudioSource.Play();
        grinderAudioSource.Stop();
    }

    public void ShowUITip()
    {
        toolTipUI.SetActive(true);
    }

    public void HideUITip()
    {
        toolTipUI.SetActive(false);
    }

    private void PlayAnimation(string animationName)
    {
        if (animator == null) return;

        // Play the animation
        animator.Play(animationName);
    }
    
    public void GrinderInsertNode()
    {
        if (targetTimer != null) targetTimer.currentTime = targetTimer.currentTime + timeToAdd;
        insertAudioSource.Play();
        PlayAnimation(insertAnimation.name);
    }
}
