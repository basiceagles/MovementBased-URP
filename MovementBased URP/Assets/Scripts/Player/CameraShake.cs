using UnityEngine;
using System.Collections;

public class CameraShake : MonoBehaviour
{
    [Header("Shake Settings")]
    [SerializeField] private float shakeDuration = 0.5f;
    [SerializeField] private float shakeMagnitude = 0.1f;
    [SerializeField] private float dampingSpeed = 1.0f;
    
    private Vector3 initialPosition;
    private Coroutine shakeCoroutine;
    private bool isShaking = false;

    void Start()
    {
        // Store the initial position of the camera
        initialPosition = transform.localPosition;
    }

    /// <summary>
    /// Triggers the camera shake effect
    /// </summary>
    public void Shake()
    {
        // Stop any existing shake coroutine
        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
        }
        
        // Start new shake coroutine
        shakeCoroutine = StartCoroutine(ShakeCoroutine(shakeDuration, shakeMagnitude));
    }

    /// <summary>
    /// Triggers the camera shake effect with custom parameters
    /// </summary>
    /// <param name="duration">How long the shake should last</param>
    /// <param name="magnitude">How intense the shake should be</param>
    public void Shake(float duration, float magnitude)
    {
        // Stop any existing shake coroutine
        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
        }
        
        // Start new shake coroutine with custom parameters
        shakeCoroutine = StartCoroutine(ShakeCoroutine(duration, magnitude));
    }

    /// <summary>
    /// Coroutine that handles the camera shake animation
    /// </summary>
    private IEnumerator ShakeCoroutine(float duration, float magnitude)
    {
        isShaking = true;
        initialPosition = transform.localPosition;
        
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            // Calculate current magnitude with damping
            float currentMagnitude = magnitude * (1f - (elapsed / duration));
            
            // Move camera to a random position within a unit sphere
            transform.localPosition = initialPosition + Random.insideUnitSphere * currentMagnitude;
            
            elapsed += Time.deltaTime * dampingSpeed;
            yield return null;
        }
        
        // Reset camera position
        transform.localPosition = initialPosition;
        isShaking = false;
        shakeCoroutine = null;
    }

    /// <summary>
    /// Stops the camera shake immediately
    /// </summary>
    public void StopShake()
    {
        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
            transform.localPosition = initialPosition;
            isShaking = false;
            shakeCoroutine = null;
        }
    }

    /// <summary>
    /// Checks if the camera is currently shaking
    /// </summary>
    /// <returns>True if camera is shaking</returns>
    public bool IsShaking()
    {
        return isShaking;
    }

    /// <summary>
    /// Smoothly shakes the camera with a more natural falloff
    /// </summary>
    public void ShakeSmooth(float duration, float magnitude, AnimationCurve falloffCurve = null)
    {
        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
        }
        
        shakeCoroutine = StartCoroutine(SmoothShakeCoroutine(duration, magnitude, falloffCurve));
    }

    private IEnumerator SmoothShakeCoroutine(float duration, float magnitude, AnimationCurve falloffCurve)
    {
        isShaking = true;
        initialPosition = transform.localPosition;
        
        float elapsed = 0f;
        
        // Default falloff curve if none provided
        if (falloffCurve == null)
        {
            falloffCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
        }
        
        while (elapsed < duration)
        {
            // Calculate current magnitude using the falloff curve
            float progress = elapsed / duration;
            float currentMagnitude = magnitude * falloffCurve.Evaluate(progress);
            
            // Move camera to a random position
            transform.localPosition = initialPosition + Random.insideUnitSphere * currentMagnitude;
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // Reset camera position
        transform.localPosition = initialPosition;
        isShaking = false;
        shakeCoroutine = null;
    }
}