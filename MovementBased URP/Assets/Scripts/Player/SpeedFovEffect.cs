using UnityEngine;

public class SpeedFOVEffect : MonoBehaviour
{
    [Header("FOV Settings")]
    public float normalFOV = 80f;
    public float minFOV = 40f;
    public float speedThreshold = 20f;
    public float shrinkMultiplier = 0.5f;
    
    [Header("Smoothing")]
    public float fovSmoothTime = 0.2f;

    [Header("Player Speed")]
    public float currentSpeed;
    
    [Header("VFX")]
    public GameObject vfxGameobject;
    
    [Header("Audio - Wing Effect")]
    public AudioSource wingAudioSource;
    public float audioSpeedThreshold = 15f; 
    public float maxVolume = 0.8f;
    public float audioSmoothTime = 0.3f;
    
    private Camera mainCamera;
    private float targetFOV;
    private float fovVelocity;
    private PlayerController playerController;
    private bool vfxWasActive; 
    private float audioVolumeVelocity; 

    void Start()
    {
        playerController = FindObjectOfType<PlayerController>();
        mainCamera = GetComponent<Camera>();
        
        if (mainCamera != null)
        {
            mainCamera.fieldOfView = normalFOV;
            targetFOV = normalFOV;
        }

        if (vfxGameobject != null)
        {
            vfxGameobject.SetActive(false);
            vfxWasActive = false;
        }

        InitializeAudio();
    }

    void Update()
    {
        currentSpeed = playerController.currentSpeed;
        UpdateFOVBasedOnSpeed();
        UpdateVFXBasedOnSpeed();
        UpdateAudioBasedOnSpeed();
    }

    private void InitializeAudio()
    {
        if (wingAudioSource == null)
        {
            wingAudioSource = GetComponent<AudioSource>();
        }

        if (wingAudioSource == null)
        {
            wingAudioSource = gameObject.AddComponent<AudioSource>();
        }

        if (wingAudioSource != null)
        {
            wingAudioSource.volume = 0f;
            wingAudioSource.loop = true;
            wingAudioSource.playOnAwake = false;
            
            if (!wingAudioSource.isPlaying)
            {
                wingAudioSource.Play();
            }
        }
    }

    private void UpdateFOVBasedOnSpeed()
    {
        if (mainCamera == null) return;

        if (currentSpeed <= speedThreshold)
        {
            targetFOV = normalFOV;
        }
        else
        {
            float excessSpeed = currentSpeed - speedThreshold;
            float fovReduction = excessSpeed * shrinkMultiplier;
            targetFOV = normalFOV - fovReduction;
            targetFOV = Mathf.Max(targetFOV, minFOV);
        }

        mainCamera.fieldOfView = Mathf.SmoothDamp(mainCamera.fieldOfView, targetFOV, ref fovVelocity, fovSmoothTime);
    }

    private void UpdateVFXBasedOnSpeed()
    {
        if (vfxGameobject == null) return;

        bool shouldBeActive = currentSpeed > speedThreshold;

        if (shouldBeActive != vfxWasActive)
        {
            vfxGameobject.SetActive(shouldBeActive);
            vfxWasActive = shouldBeActive;
        }
    }

    private void UpdateAudioBasedOnSpeed()
    {
        if (wingAudioSource == null) return;

        float targetVolume = 0f;

        if (currentSpeed > audioSpeedThreshold)
        {
            float excessSpeed = currentSpeed - audioSpeedThreshold;
            float volumeRange = Mathf.Max(speedThreshold - audioSpeedThreshold, 1f);
            float volumeIntensity = Mathf.Clamp01(excessSpeed / volumeRange);
            targetVolume = volumeIntensity * maxVolume;
        }

        wingAudioSource.volume = Mathf.SmoothDamp(wingAudioSource.volume, targetVolume, ref audioVolumeVelocity, audioSmoothTime);

        if (wingAudioSource.volume < 0.01f && wingAudioSource.volume > 0f)
        {
            wingAudioSource.volume = 0f;
        }
    }

    public float GetCurrentFOV()
    {
        return mainCamera != null ? mainCamera.fieldOfView : 0f;
    }

    public void ResetFOV()
    {
        if (mainCamera != null)
        {
            mainCamera.fieldOfView = normalFOV;
            targetFOV = normalFOV;
        }
        currentSpeed = 0f;

        if (vfxGameobject != null)
        {
            vfxGameobject.SetActive(false);
            vfxWasActive = false;
        }

        if (wingAudioSource != null)
        {
            wingAudioSource.volume = 0f;
        }
    }

    public void SetVFXActive(bool active)
    {
        if (vfxGameobject != null)
        {
            vfxGameobject.SetActive(active);
            vfxWasActive = active;
        }
    }

    public void SetWingAudioClip(AudioClip clip)
    {
        if (wingAudioSource != null)
        {
            wingAudioSource.clip = clip;
            if (!wingAudioSource.isPlaying)
            {
                wingAudioSource.Play();
            }
        }
    }

    public void SetMaxVolume(float volume)
    {
        maxVolume = Mathf.Clamp01(volume);
    }

    public void SetAudioSmoothTime(float smoothTime)
    {
        audioSmoothTime = Mathf.Max(0.01f, smoothTime);
    }

    public void SetAudioSpeedThreshold(float threshold)
    {
        audioSpeedThreshold = Mathf.Max(0f, threshold);
    }
}