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
    [SerializeField] private string tutorialSceneName;

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

    public void StartDemoAfterDelay()
    {
        StartCoroutine(DelayedCallCoroutine(demoSceneName));
    }

    public void StartTutorialAfterDelay()
    {
        StartCoroutine(DelayedCallCoroutine(tutorialSceneName));
    }

    IEnumerator DelayedCallCoroutine(string sceneName)
    {
        // Wait for the specified duration
        yield return new WaitForSeconds(fadeDuration);
        
        // Call your method after delay
        SceneManager.LoadScene(sceneName);
    }
}
