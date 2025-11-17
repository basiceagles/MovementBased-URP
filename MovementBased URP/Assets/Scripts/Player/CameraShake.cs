using UnityEngine;
using System.Collections;

public class CameraShake : MonoBehaviour
{
    [Header("Shake Settings")]
    [SerializeField] private float shakeDuration = 0.5f;
    [SerializeField] private float shakeMagnitude = 0.1f;
    [SerializeField] private float dampingSpeed = 1.0f;
    [SerializeField] private AudioSource earthquakeAS;
    
    private Vector3 initialPosition;
    private Coroutine shakeCoroutine;
    private Coroutine constantShakeCoroutine;
    private Coroutine smoothConstantShakeCoroutine;
    private bool isShaking = false;
    private bool isConstantShaking = false;
    private bool isSmoothConstantShaking = false;

    void Start()
    {
        // Store the initial position of the camera
        initialPosition = transform.localPosition;
    }

    /// <summary>
    /// Starts constant camera shaking that smoothly increases from 0.1 to 1 magnitude over adjustable time
    /// </summary>
    /// <param name="rampUpTime">Time in seconds to smoothly increase from 0.1 to 1 magnitude</param>
    public void ConstantlyShakeStartSmoothly(float rampUpTime = 1.0f)
    {
        if (isSmoothConstantShaking) return;
        
        // Stop any existing smooth constant shake coroutine
        if (smoothConstantShakeCoroutine != null)
        {
            StopCoroutine(smoothConstantShakeCoroutine);
        }
        
        // Start new smooth constant shake coroutine
        smoothConstantShakeCoroutine = StartCoroutine(SmoothConstantShakeCoroutine(rampUpTime));
        isSmoothConstantShaking = true;
    }

    /// <summary>
    /// Coroutine that handles smooth constant camera shaking with ramp up
    /// </summary>
    private IEnumerator SmoothConstantShakeCoroutine(float rampUpTime)
    {
        isSmoothConstantShaking = true;
        initialPosition = transform.localPosition;
        
        float elapsed = 0f;
        float startMagnitude = 0.1f;
        float targetMagnitude = 1.0f;
        
        // Ramp up phase
        while (elapsed < rampUpTime)
        {
            // Calculate current magnitude with smooth interpolation
            float currentMagnitude = Mathf.Lerp(startMagnitude, targetMagnitude, elapsed / rampUpTime);
            
            // Move camera to a random position within a unit sphere
            transform.localPosition = initialPosition + Random.insideUnitSphere * currentMagnitude;
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // Constant phase - maintain target magnitude
        while (true)
        {
            transform.localPosition = initialPosition + Random.insideUnitSphere * targetMagnitude;
            yield return null;
        }
    }

    /// <summary>
    /// Stops the smooth constant camera shaking
    /// </summary>
    public void ConstantlyShakeStopSmoothly()
    {
        if (smoothConstantShakeCoroutine != null)
        {
            StopCoroutine(smoothConstantShakeCoroutine);
            transform.localPosition = initialPosition;
            isSmoothConstantShaking = false;
            smoothConstantShakeCoroutine = null;
        }
    }

    /// <summary>
    /// Starts constant camera shaking using the already set settings
    /// </summary>
    public void ConstantlyShakeStart()
    {
        if (isConstantShaking) return;
        
        // Stop any existing constant shake coroutine
        if (constantShakeCoroutine != null)
        {
            StopCoroutine(constantShakeCoroutine);
        }
        
        // Start new constant shake coroutine
        constantShakeCoroutine = StartCoroutine(ConstantShakeCoroutine());
        isConstantShaking = true;

        earthquakeAS.Play();
    }

    /// <summary>
    /// Starts constant camera shaking with custom parameters
    /// </summary>
    /// <param name="magnitude">How intense the shake should be</param>
    public void ConstantlyShakeStart(float magnitude)
    {
        if (isConstantShaking) return;
        
        // Stop any existing constant shake coroutine
        if (constantShakeCoroutine != null)
        {
            StopCoroutine(constantShakeCoroutine);
        }
        
        // Start new constant shake coroutine with custom magnitude
        constantShakeCoroutine = StartCoroutine(ConstantShakeCoroutine(magnitude));
        isConstantShaking = true;

        earthquakeAS.Play();
    }

    /// <summary>
    /// Stops the constant camera shaking
    /// </summary>
    public void ConstantlyShakeStop()
    {
        if (constantShakeCoroutine != null)
        {
            StopCoroutine(constantShakeCoroutine);
            transform.localPosition = initialPosition;
            isConstantShaking = false;
            constantShakeCoroutine = null;
        }

        earthquakeAS.Stop();
    }

    /// <summary>
    /// Coroutine that handles constant camera shaking
    /// </summary>
    private IEnumerator ConstantShakeCoroutine(float? customMagnitude = null)
    {
        isConstantShaking = true;
        initialPosition = transform.localPosition;
        
        float magnitude = customMagnitude ?? shakeMagnitude;
        
        while (true)
        {
            // Move camera to a random position within a unit sphere
            transform.localPosition = initialPosition + Random.insideUnitSphere * magnitude;
            yield return null;
        }
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
        
        // Also stop constant shaking if active
        ConstantlyShakeStop();
        ConstantlyShakeStopSmoothly();
    }

    /// <summary>
    /// Checks if the camera is currently shaking
    /// </summary>
    /// <returns>True if camera is shaking</returns>
    public bool IsShaking()
    {
        return isShaking || isConstantShaking || isSmoothConstantShaking;
    }

    /// <summary>
    /// Checks if the camera is currently constant shaking
    /// </summary>
    /// <returns>True if camera is constant shaking</returns>
    public bool IsConstantShaking()
    {
        return isConstantShaking || isSmoothConstantShaking;
    }

    /// <summary>
    /// Checks if the camera is currently smooth constant shaking
    /// </summary>
    /// <returns>True if camera is smooth constant shaking</returns>
    public bool IsSmoothConstantShaking()
    {
        return isSmoothConstantShaking;
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