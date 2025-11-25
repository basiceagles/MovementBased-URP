using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using SmallHedge.SoundManager;

public class StartAGameInHub : MonoBehaviour
{
    [SerializeField] private Animator animatorUI;
    [SerializeField] private AnimationClip animationClipUI;
    [SerializeField] private AnimationClip animationClipUIClose;
    [SerializeField] private CameraController cameraController;
    [SerializeField] private float fadeDuration;
    [Header("Scene Names")]
    [SerializeField] private string demoSceneName;

    public void ShowGameUI()
    {
        animatorUI.Play(animationClipUI.name);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        cameraController.enabled = false;
    }

    public void HideGameUI()
    {
        animatorUI.Play(animationClipUIClose.name);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        cameraController.enabled = true;
    }

    public void StartDemo()
    {
        SceneManager.LoadScene(demoSceneName); // Load the demo scene
    }

    public void StartDemoAfterDelay()
    {
        StartCoroutine(DelayedCallCoroutine());
    }

    IEnumerator DelayedCallCoroutine()
    {
        // Wait for the specified duration
        yield return new WaitForSeconds(fadeDuration);
        
        // Call your method after delay
        StartDemo();
    }
}
