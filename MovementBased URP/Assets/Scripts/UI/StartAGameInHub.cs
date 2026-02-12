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
    [SerializeField] private string IntroScenename;  
    [SerializeField] private string Intro2Scenename;
    [SerializeField] private string TutorialRegularSceneName;
    public void ShowGameUI()
    {
        animatorUI.Play(animationClipUI.name);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        cameraController.SetCameraMovementEnabled(false);
    }

    public void HideGameUI()
    {
        animatorUI.Play(animationClipUIClose.name);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        cameraController.SetCameraMovementEnabled(true);
    }

    public void StartDemoAfterDelay()
    {
        StartCoroutine(DelayedCallCoroutine(demoSceneName));
    }

    public void StartTutorialAfterDelay()
    {
        StartCoroutine(DelayedCallCoroutine(tutorialSceneName));
    }

    public void StartScene3AfterDelay()
    {
        StartCoroutine(DelayedCallCoroutine(IntroScenename));
    }

    public void StartScene4AfterDelay()
    {
        StartCoroutine(DelayedCallCoroutine(Intro2Scenename));
    }

    public void StartTutorialRegularAfterDelay()
    {
        StartCoroutine(DelayedCallCoroutine(TutorialRegularSceneName));
    }

    IEnumerator DelayedCallCoroutine(string sceneName)
    {
        yield return new WaitForSeconds(fadeDuration);

        SceneManager.LoadScene(sceneName);
    }
}