using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadScene : MonoBehaviour
{
    public bool loadScene = true;
    public bool restartScene = false;
    public string sceneName;
    private string currentScene;
    
    [Header("Money Settings")]
    [Tooltip("If true, will save money before restarting (player death)")]
    public bool saveMoneyOnRestart = true;

    void Start()
    {
        currentScene = SceneManager.GetActiveScene().name;
    }
    
    public void LoadSceneSpecific()
    {
        Debug.Log("[LoadScene] LoadSceneSpecific called!");
        
        if (MoneyManager.Instance != null)
        {
            Debug.Log("[LoadScene] Calling MoneyManager.OnPlayerDeath()");
            MoneyManager.Instance.OnPlayerDeath();
        }
        else
        {
            Debug.LogError("[LoadScene] MoneyManager.Instance is NULL!");
        }
        
        if (loadScene)
        {
            Debug.Log($"[LoadScene] Loading scene: {sceneName}");
            SceneManager.LoadScene(sceneName);
        }
        if (restartScene)
        {
            Debug.Log($"[LoadScene] Restarting scene: {currentScene}");
            SceneManager.LoadScene(currentScene);
        }
    }

    public void RestartWithMoneySave()
    {
        if (MoneyManager.Instance != null)
        {
            MoneyManager.Instance.OnPlayerDeath();
        }
        SceneManager.LoadScene(currentScene);
    }
    
    public void RestartWithoutMoneySave()
    {
        SceneManager.LoadScene(currentScene);
    }
}
