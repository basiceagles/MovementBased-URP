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
    public float audioSpeedThreshold = 15f; // Separate threshold for audio
    public float maxVolume = 0.8f;
    public float audioSmoothTime = 0.3f;
    
    private Camera mainCamera;
    private float targetFOV;
    private float fovVelocity;
    private PlayerController playerController;
    private bool vfxWasActive; // Track previous state to avoid unnecessary SetActive calls
    private float audioVolumeVelocity; // Smooth damping for audio volume

    void Start()
    {
        playerController = FindObjectOfType<PlayerController>();
        mainCamera = GetComponent<Camera>();
        
        // Initialize FOV
        if (mainCamera != null)
        {
            mainCamera.fieldOfView = normalFOV;
            targetFOV = normalFOV;
        }

        // Initialize VFX state
        if (vfxGameobject != null)
        {
            vfxGameobject.SetActive(false);
            vfxWasActive = false;
        }

        // Initialize Audio
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
        // If AudioSource is not assigned, try to get it from this GameObject
        if (wingAudioSource == null)
        {
            wingAudioSource = GetComponent<AudioSource>();
        }

        // If still null, create a new AudioSource
        if (wingAudioSource == null)
        {
            wingAudioSource = gameObject.AddComponent<AudioSource>();
        }

        // Configure AudioSource
        if (wingAudioSource != null)
        {
            wingAudioSource.volume = 0f;
            wingAudioSource.loop = true;
            wingAudioSource.playOnAwake = false;
            
            // Start playing the audio (volume will be controlled)
            if (!wingAudioSource.isPlaying)
            {
                wingAudioSource.Play();
            }
        }
    }

    private void UpdateFOVBasedOnSpeed()
    {
        if (mainCamera == null) return;

        // Calculate target FOV based on speed
        if (currentSpeed <= speedThreshold)
        {
            // Normal FOV when speed is 0-20
            targetFOV = normalFOV;
        }
        else
        {
            // Calculate how much above threshold we are
            float excessSpeed = currentSpeed - speedThreshold;
            
            // Calculate FOV reduction using shrink multiplier
            float fovReduction = excessSpeed * shrinkMultiplier;
            
            // Apply reduction to normal FOV and clamp to minimum
            targetFOV = normalFOV - fovReduction;
            targetFOV = Mathf.Max(targetFOV, minFOV);
        }

        // Smoothly transition to target FOV
        mainCamera.fieldOfView = Mathf.SmoothDamp(mainCamera.fieldOfView, targetFOV, ref fovVelocity, fovSmoothTime);
    }

    private void UpdateVFXBasedOnSpeed()
    {
        if (vfxGameobject == null) return;

        bool shouldBeActive = currentSpeed > speedThreshold;

        // Only call SetActive if the state actually changed
        if (shouldBeActive != vfxWasActive)
        {
            vfxGameobject.SetActive(shouldBeActive);
            vfxWasActive = shouldBeActive;
        }
    }

    private void UpdateAudioBasedOnSpeed()
    {
        if (wingAudioSource == null) return;

        // Calculate target volume based on speed using audioSpeedThreshold
        float targetVolume = 0f;

        if (currentSpeed > audioSpeedThreshold)
        {
            // Calculate volume intensity based on how much above audio threshold
            float excessSpeed = currentSpeed - audioSpeedThreshold;
            
            // Calculate the range from audio threshold to FOV threshold (or beyond)
            float volumeRange = Mathf.Max(speedThreshold - audioSpeedThreshold, 1f);
            
            // Scale volume intensity based on how far we are into the range
            float volumeIntensity = Mathf.Clamp01(excessSpeed / volumeRange);
            
            targetVolume = volumeIntensity * maxVolume;
        }

        // Smoothly transition audio volume
        wingAudioSource.volume = Mathf.SmoothDamp(wingAudioSource.volume, targetVolume, ref audioVolumeVelocity, audioSmoothTime);

        // Optional: Stop audio completely when volume is very low to save resources
        if (wingAudioSource.volume < 0.01f && wingAudioSource.volume > 0f)
        {
            wingAudioSource.volume = 0f;
        }
    }

    // Public method to get current FOV for debugging
    public float GetCurrentFOV()
    {
        return mainCamera != null ? mainCamera.fieldOfView : 0f;
    }

    // Reset FOV to normal immediately
    public void ResetFOV()
    {
        if (mainCamera != null)
        {
            mainCamera.fieldOfView = normalFOV;
            targetFOV = normalFOV;
        }
        currentSpeed = 0f;

        // Hide VFX when resetting
        if (vfxGameobject != null)
        {
            vfxGameobject.SetActive(false);
            vfxWasActive = false;
        }

        // Reset audio volume
        if (wingAudioSource != null)
        {
            wingAudioSource.volume = 0f;
        }
    }

    // Optional: Manual control for VFX
    public void SetVFXActive(bool active)
    {
        if (vfxGameobject != null)
        {
            vfxGameobject.SetActive(active);
            vfxWasActive = active;
        }
    }

    // Audio control methods
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