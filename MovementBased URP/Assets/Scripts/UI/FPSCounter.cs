using UnityEngine;
using TMPro;

public class FPSCounter : MonoBehaviour
{
    public TMP_Text fpsText;
    public bool showFPS = true;
    
    private float deltaTime = 0.0f;
    private float updateInterval = 0.5f; 
    private float timer = 0.0f;

    void Update()
    {
        if (!showFPS || fpsText == null) return;

        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
        timer += Time.unscaledDeltaTime;

        if (timer >= updateInterval)
        {
            float fps = 1.0f / deltaTime;
            string vsyncStatus = QualitySettings.vSyncCount > 0 ? " [V-SYNC]" : "";
            fpsText.text = $"FPS: {Mathf.Ceil(fps)}{vsyncStatus}";
            timer = 0.0f;
        }
    }

    public void ToggleFPS(bool enabled)
    {
        showFPS = enabled;
        if (fpsText != null)
        {
            fpsText.gameObject.SetActive(enabled);
        }
    }
}
