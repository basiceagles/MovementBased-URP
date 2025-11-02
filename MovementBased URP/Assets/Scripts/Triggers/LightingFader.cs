using UnityEngine;
using System.Collections;

public class LightingFader : MonoBehaviour
{
    [Header("Lighting Colors")]
    public Vector3 outsideColor = new Vector3(121f, 121f, 121f); // Sky blue
    public Vector3 roomColor = new Vector3(57f, 57f, 57f); // Warm white
    
    [Header("Fade Settings")]
    public float fadeTime = 1.5f;
    
    private Coroutine currentFadeCoroutine;
    
    // Call this method to fade from outsideColor to roomColor
    public void CallFadeIn()
    {
        if (currentFadeCoroutine != null)
        {
            StopCoroutine(currentFadeCoroutine);
        }
        
        Color startColor = Vector3ToColor(outsideColor);
        Color targetColor = Vector3ToColor(roomColor);
        
        currentFadeCoroutine = StartCoroutine(FadeLightingColor(startColor, targetColor, fadeTime));
    }
    
    // Call this method to fade from roomColor to outsideColor
    public void CallFadeOut()
    {
        if (currentFadeCoroutine != null)
        {
            StopCoroutine(currentFadeCoroutine);
        }
        
        Color startColor = Vector3ToColor(roomColor);
        Color targetColor = Vector3ToColor(outsideColor);
        
        currentFadeCoroutine = StartCoroutine(FadeLightingColor(startColor, targetColor, fadeTime));
    }
    
    // Coroutine to handle the fade transition
    private IEnumerator FadeLightingColor(Color startColor, Color targetColor, float duration)
    {
        float elapsedTime = 0f;
        
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);
            
            // Lerp between colors
            Color currentColor = Color.Lerp(startColor, targetColor, t);
            
            // Apply to lighting environment
            RenderSettings.ambientLight = currentColor;
            
            yield return null;
        }
        
        // Ensure we end exactly with the target color
        RenderSettings.ambientLight = targetColor;
        currentFadeCoroutine = null;
    }
    
    // Helper method to convert Vector3 to Color
    private Color Vector3ToColor(Vector3 colorVector)
    {
        // Convert from 0-255 range to 0-1 range for Color
        return new Color(colorVector.x / 255f, colorVector.y / 255f, colorVector.z / 255f);
    }
}