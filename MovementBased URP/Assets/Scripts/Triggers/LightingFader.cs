using UnityEngine;
using System.Collections;

public class LightingFader : MonoBehaviour
{
    [Header("Lighting Colors")]
    public Vector3 outsideColor = new Vector3(121f, 121f, 121f); 
    public Vector3 outsideFogColor = new Vector3(91f, 91f, 91f); 
    public Vector3 roomColor = new Vector3(57f, 57f, 57f); 
    public Vector3 roomFogColor = new Vector3(35f, 35f, 35f);
    
    [Header("Fog Settings")]
    public bool enableFogTransition = true;
    public float outsideFogDensity = 0.05f;
    public float roomFogDensity = 0.05f;
    
    [Header("Fade Settings")]
    public float fadeTime = 1.5f;
    
    private Coroutine currentFadeCoroutine;
    
    public void CallFadeIn()
    {
        if (currentFadeCoroutine != null)
        {
            StopCoroutine(currentFadeCoroutine);
        }
        
        Color startLightColor = Vector3ToColor(outsideColor);
        Color targetLightColor = Vector3ToColor(roomColor);
        Color startFogColor = Vector3ToColor(outsideFogColor);
        Color targetFogColor = Vector3ToColor(roomFogColor);
        
        currentFadeCoroutine = StartCoroutine(FadeLightingAndFog(
            startLightColor, targetLightColor, 
            startFogColor, targetFogColor,
            outsideFogDensity, roomFogDensity, 
            fadeTime));
    }
    
    public void CallFadeOut()
    {
        if (currentFadeCoroutine != null)
        {
            StopCoroutine(currentFadeCoroutine);
        }
        
        Color startLightColor = Vector3ToColor(roomColor);
        Color targetLightColor = Vector3ToColor(outsideColor);
        Color startFogColor = Vector3ToColor(roomFogColor);
        Color targetFogColor = Vector3ToColor(outsideFogColor);
        
        currentFadeCoroutine = StartCoroutine(FadeLightingAndFog(
            startLightColor, targetLightColor, 
            startFogColor, targetFogColor,
            roomFogDensity, outsideFogDensity, 
            fadeTime));
    }
    
    private IEnumerator FadeLightingAndFog(Color startLightColor, Color targetLightColor, 
                                         Color startFogColor, Color targetFogColor,
                                         float startFogDensity, float targetFogDensity, 
                                         float duration)
    {
        float elapsedTime = 0f;
        
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);
            
            Color currentLightColor = Color.Lerp(startLightColor, targetLightColor, t);
            RenderSettings.ambientLight = currentLightColor;
            
            if (enableFogTransition)
            {
                Color currentFogColor = Color.Lerp(startFogColor, targetFogColor, t);
                RenderSettings.fogColor = currentFogColor;
                
                float currentFogDensity = Mathf.Lerp(startFogDensity, targetFogDensity, t);
                RenderSettings.fogDensity = currentFogDensity;
            }
            
            yield return null;
        }
        
        RenderSettings.ambientLight = targetLightColor;
        
        if (enableFogTransition)
        {
            RenderSettings.fogColor = targetFogColor;
            RenderSettings.fogDensity = targetFogDensity;
        }
        
        currentFadeCoroutine = null;
    }
    
    private Color Vector3ToColor(Vector3 colorVector)
    {
        return new Color(colorVector.x / 255f, colorVector.y / 255f, colorVector.z / 255f);
    }
    
    private void Start()
    {
        if (enableFogTransition)
        {
            RenderSettings.fog = true;
        }
    }
}