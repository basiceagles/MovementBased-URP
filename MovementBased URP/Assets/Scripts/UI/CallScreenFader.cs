using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CallScreenFader : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image fadeImage;
    [SerializeField] private GameObject fadeImageGameobject; 

    [Header("Fade Settings")]
    [SerializeField] private float fadeInDuration = 1f;
    [SerializeField] private float fadeOutDuration = 1f;
    [SerializeField] private bool fadeOutOnStart = false;

    private Coroutine currentFade;
    private bool isFading = false;

    void Start()
    {
        gameObject.SetActive(true);
        
        if (fadeImage == null)
        {
            return;
        }

        if (fadeImageGameobject == null)
        {
            fadeImageGameobject = fadeImage.gameObject;
        }

        if (fadeOutOnStart)
        {
            fadeImageGameobject.SetActive(true);
            SetAlphaImmediate(1f); 
            CallScreenFadeOut();
        }
        else
        {
            fadeImageGameobject.SetActive(false);
            SetAlphaImmediate(0f);
        }
    }

    public void CallScreenFadeIn()
    {
        if (isFading) return;
        
        StopCurrentFade();
        currentFade = StartCoroutine(FadeIn());
    }

    public void CallScreenFadeOut()
    {
        if (isFading) return;
        
        StopCurrentFade();
        currentFade = StartCoroutine(FadeOut());
    }

    private IEnumerator FadeIn()
    {
        isFading = true;
        
        fadeImageGameobject.SetActive(true);
        
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
    }

    private IEnumerator FadeOut()
    {
        isFading = true;
        
        fadeImageGameobject.SetActive(true);
        
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
            yield return null;
        }

        SetAlphaImmediate(targetAlpha);
        
        fadeImageGameobject.SetActive(false);
        isFading = false;
    }

    private void SetAlpha(float alpha)
    {
        if (fadeImage == null) 
        {
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
            return;
        }
        
        Color c = fadeImage.color;
        c.a = alpha;
        fadeImage.color = c;
        
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
    
    public void DebugSetAlpha(float alpha)
    {
        if (fadeImage != null)
        {
            SetAlphaImmediate(alpha);
        }
    }
    
    public void DebugToggleActive()
    {
        if (fadeImageGameobject != null)
        {
            fadeImageGameobject.SetActive(!fadeImageGameobject.activeSelf);
        }
    }
}