using UnityEngine;

public class SlideMetalDoorManager : MonoBehaviour
{
    [Header("Animator Reference")]
    public Animator doorAnimator;
    
    [Header("Animation Triggers")]
    public string normalAnimationTrigger = "MetalSlideOpen";
    public string brokenAnimationTrigger = "MetalSlideOpenBroken";
    
    [Header("Animation Chances")]
    [Range(0, 100)]
    public float normalAnimationChance = 70f;
    [Range(0, 100)]
    public float brokenAnimationChance = 30f;

    [Header("Debug")]
    public bool showDebug = false;
    
    [Header("Audio Sources")]
    public AudioSource normalAudioSource;
    public AudioSource brokenAudioSource;

    void Start()
    {
        // Validate chances
        ValidateChances();
    }

    public void OpenSlideDoor()
    {
        // Calculate which animation to play based on chances
        string triggerToPlay = GetRandomAnimationTrigger();
        
        // Play the selected animation
        doorAnimator.SetTrigger(triggerToPlay);
        
        // Play the corresponding audio
        PlayAudioForAnimation(triggerToPlay);
    }

    string GetRandomAnimationTrigger()
    {
        // Generate a random number between 0 and 100
        float randomValue = Random.Range(0f, 100f);

        // Check if random value falls within the normal animation chance
        if (randomValue <= normalAnimationChance)
        {
            return normalAnimationTrigger;
        }
        // Otherwise play the broken animation
        else
        {
            return brokenAnimationTrigger;
        }
    }
    
    void PlayAudioForAnimation(string animationTrigger)
    {
        if (animationTrigger == normalAnimationTrigger)
        {
            PlayNormalAudio();
        }
        else if (animationTrigger == brokenAnimationTrigger)
        {
            PlayBrokenAudio();
        }
    }
    
    void PlayNormalAudio()
    {
        normalAudioSource.Play();
    }
    
    void PlayBrokenAudio()
    {
        brokenAudioSource.Play();
    }

    void ValidateChances()
    {
        // Ensure chances don't exceed 100%
        float totalChance = normalAnimationChance + brokenAnimationChance;
        
        if (totalChance > 100f)
        {
            float normalizationFactor = 100f / totalChance;
            normalAnimationChance *= normalizationFactor;
            brokenAnimationChance *= normalizationFactor;
        }
    }
}