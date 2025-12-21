using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CallScreenFader : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image fadeImage;
    [SerializeField] private GameObject fadeImageGameobject; // Should be a CHILD GameObject

    [Header("Fade Settings")]
    [SerializeField] private float fadeInDuration = 1f;
    [SerializeField] private float fadeOutDuration = 1f;
    [SerializeField] private bool fadeOutOnStart = false;

    private Coroutine currentFade;
    private bool isFading = false;

    void Start()
    {
        // Make sure THIS GameObject (with the script) stays active
        gameObject.SetActive(true);
        
        if (fadeImage == null)
        {
            Debug.LogError("Fade Image is missing!");
            return;
        }

        if (fadeImageGameobject == null)
        {
            fadeImageGameobject = fadeImage.gameObject;
        }

        if (fadeOutOnStart)
        {
            // Start with image visible and fully opaque
            fadeImageGameobject.SetActive(true);
            SetAlphaImmediate(1f); // Use immediate alpha set
            CallScreenFadeOut();
        }
        else
        {
            // Start with image hidden
            fadeImageGameobject.SetActive(false);
            SetAlphaImmediate(0f);
        }
    }

    public void CallScreenFadeIn()
    {
        if (isFading) return;
        
        Debug.Log("CallScreenFadeIn called");
        StopCurrentFade();
        currentFade = StartCoroutine(FadeIn());
    }

    public void CallScreenFadeOut()
    {
        if (isFading) return;
        
        Debug.Log("CallScreenFadeOut called");
        StopCurrentFade();
        currentFade = StartCoroutine(FadeOut());
    }

    private IEnumerator FadeIn()
    {
        isFading = true;
        Debug.Log("Starting FadeIn - From alpha: 0 to 1");
        
        // Ensure the fade image GameObject is active
        fadeImageGameobject.SetActive(true);
        
        // Always start from 0 alpha for FadeIn
        SetAlphaImmediate(0f);
        
        float elapsed = 0f;
        float startAlpha = 0f;
        float targetAlpha = 1f;

        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / fadeInDuration);
            float alpha = Mathf.Lerp(startAlpha, targetAlpha, progress);
            SetAlpha(alpha);
            yield return null;
        }

        SetAlphaImmediate(targetAlpha);
        isFading = false;
        Debug.Log("FadeIn complete");
    }

    private IEnumerator FadeOut()
    {
        isFading = true;
        Debug.Log("Starting FadeOut - From alpha: 1 to 0");
        
        // Ensure the fade image GameObject is active at the start of fade
        fadeImageGameobject.SetActive(true);
        
        // Always start from 1 alpha for FadeOut
        SetAlphaImmediate(1f);
        
        float elapsed = 0f;
        float startAlpha = 1f;
        float targetAlpha = 0f;

        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / fadeOutDuration);
            float alpha = Mathf.Lerp(startAlpha, targetAlpha, progress);
            SetAlpha(alpha);
            Debug.Log($"FadeOut progress: {progress}, alpha: {alpha}"); // Debug line
            yield return null;
        }

        SetAlphaImmediate(targetAlpha);
        
        // Hide the fade image GameObject after fade completes
        fadeImageGameobject.SetActive(false);
        isFading = false;
        Debug.Log("FadeOut complete - GameObject hidden");
    }

    private void SetAlpha(float alpha)
    {
        if (fadeImage == null) 
        {
            Debug.LogError("FadeImage is null in SetAlpha!");
            return;
        }
        
        Color c = fadeImage.color;
        c.a = alpha;
        fadeImage.color = c;
    }
    
    private void SetAlphaImmediate(float alpha)
    {
        if (fadeImage == null) 
        {
            Debug.LogError("FadeImage is null in SetAlphaImmediate!");
            return;
        }
        
        Color c = fadeImage.color;
        c.a = alpha;
        fadeImage.color = c;
        
        // Force the Image to update
        if (fadeImage.enabled)
        {
            fadeImage.enabled = false;
            fadeImage.enabled = true;
        }
    }

    private void StopCurrentFade()
    {
        if (currentFade != null)
        {
            StopCoroutine(currentFade);
            currentFade = null;
            isFading = false;
        }
    }
    
    // For debugging - call these from buttons to test
    public void DebugSetAlpha(float alpha)
    {
        if (fadeImage != null)
        {
            SetAlphaImmediate(alpha);
            Debug.Log($"Set alpha to: {alpha}, Current color: {fadeImage.color}");
        }
    }
    
    public void DebugToggleActive()
    {
        if (fadeImageGameobject != null)
        {
            fadeImageGameobject.SetActive(!fadeImageGameobject.activeSelf);
            Debug.Log($"GameObject active: {fadeImageGameobject.activeSelf}");
        }
    }
}