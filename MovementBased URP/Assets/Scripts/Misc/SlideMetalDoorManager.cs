using UnityEngine;

public class SlideMetalDoorManager : MonoBehaviour
{
    [Header("Animator Reference")]
    public Animator doorAnimator;
    
    [Header("Animation Triggers")]
    public string normalAnimationTrigger = "MetalSlideOpen";
    public string brokenAnimationTrigger = "MetalSlideOpenBroken";
    public string spawnOpenAnimationTrigger = "MetalSlideSpawnOpen";
    public string closeAnimationTrigger = "MetalSlideClose";
    
    [Header("Animation Chances")]
    [Range(0, 100)]
    public float normalAnimationChance = 70f;
    [Range(0, 100)]
    public float brokenAnimationChance = 30f;

    [Header("Debug")]
    public bool showDebug = false;
    public bool spawnOpen = false;
    
    [Header("Audio Sources")]
    public AudioSource normalAudioSource;
    public AudioSource brokenAudioSource;
    public AudioSource closeAudioSource;
    private bool alreadyChecked = false;

    void Start()
    {
        ValidateChances();
        SpawnOpenDoor();
    }

    void OnEnable()
    {
        ValidateChances();
        SpawnOpenDoor();
    }

    public void OpenSlideDoor()
    {
        string triggerToPlay = GetRandomAnimationTrigger();
        doorAnimator.SetTrigger(triggerToPlay);
        PlayAudioForAnimation(triggerToPlay);
    }

    string GetRandomAnimationTrigger()
    {
        float randomValue = Random.Range(0f, 100f);
        if (randomValue <= normalAnimationChance)
        {
            return normalAnimationTrigger;
        }
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
        float totalChance = normalAnimationChance + brokenAnimationChance;
        if (totalChance > 100f)
        {
            float normalizationFactor = 100f / totalChance;
            normalAnimationChance *= normalizationFactor;
            brokenAnimationChance *= normalizationFactor;
        }
    }

    public void SpawnOpenDoor()
    {
        if (spawnOpen)
        {
            doorAnimator.SetBool("MetalSlideSpawnOpen", true);
            doorAnimator.SetBool("MetalSlideClose", false);
        }
        
    }

    public void CloseDoor()
    {
        doorAnimator.SetBool("MetalSlideSpawnOpen", false);
        doorAnimator.SetBool("MetalSlideClose", true);
        closeAudioSource.Play();
    }
    
}