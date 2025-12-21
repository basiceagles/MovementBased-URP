using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class LoadingScreen : MonoBehaviour
{
    [Header("UI References")]
    public GameObject loadingScreenHolder;
    public TMP_Text loadingText;

    [Header("Loading Animation")]
    private readonly string[] loadingAnim = { "/", "--", "|", "--", "\\", "|" };

    private Coroutine loadingTextCoroutine;

    /// <summary>
    /// Call this to load a scene asynchronously
    /// </summary>
    public void LoadSceneAsync(string sceneName)
    {
        loadingScreenHolder.SetActive(true);

        if (loadingTextCoroutine != null)
            StopCoroutine(loadingTextCoroutine);

        loadingTextCoroutine = StartCoroutine(LoadingTextRoutine());
        StartCoroutine(LoadSceneRoutine(sceneName));
    }

    private IEnumerator LoadSceneRoutine(string sceneName)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        operation.allowSceneActivation = false;

        while (operation.progress < 0.9f)
        {
            int percent = Mathf.RoundToInt(operation.progress / 0.9f * 100f);
            UpdateLoadingText(percent);
            yield return null;
        }

        // Loading finished
        UpdateLoadingText(100);

        yield return new WaitForSeconds(0.2f);

        operation.allowSceneActivation = true;
    }

    private IEnumerator LoadingTextRoutine()
    {
        int animIndex = 0;

        while (true)
        {
            loadingText.text += $" {loadingAnim[animIndex]}";
            animIndex = (animIndex + 1) % loadingAnim.Length;
            yield return new WaitForSeconds(0.1f);
        }
    }

    private void UpdateLoadingText(int percent)
    {
        loadingText.text = $"Loading is in progress {percent}%";
    }
}