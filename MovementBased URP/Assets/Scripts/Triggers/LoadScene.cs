using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadScene : MonoBehaviour
{
    public bool loadScene = true;
    public bool restartScene = false;
    public string sceneName;
    private string currentScene;
    
    [Header("Money Settings")]
    public bool saveMoneyOnRestart = true;

    void Start()
    {
        currentScene = SceneManager.GetActiveScene().name;
    }
    
    public void LoadSceneSpecific()
    {
        if (MoneyManager.Instance != null)
        {
            MoneyManager.Instance.OnPlayerDeath();
        }
        
        if (loadScene)
        {
            if (currentScene == "Intro3")
            {
                 SceneManager.LoadScene("Intro4");
            }
            else if (sceneName == "Hub" && PlayerPrefs.GetInt("HubReached", 0) == 0)
            {
                SceneManager.LoadScene("Intro1");
            }
            else
            {
                SceneManager.LoadScene(sceneName);
            }
        }
        if (restartScene)
        {
            if (currentScene == "Intro3")
            {
                SceneManager.LoadScene("Intro4");
            }
            else
            {
                SceneManager.LoadScene(currentScene);
            }
        }
    }

    public void RestartWithMoneySave()
    {
        if (MoneyManager.Instance != null)
        {
            MoneyManager.Instance.OnPlayerDeath();
        }

        if (currentScene == "Intro3")
        {
            SceneManager.LoadScene("Intro4");
        }
        else
        {
            SceneManager.LoadScene(currentScene);
        }
    }
    
    public void RestartWithoutMoneySave()
    {
        if (currentScene == "Intro3")
        {
             SceneManager.LoadScene("Intro4");
        }
        else
        {
             SceneManager.LoadScene(currentScene);
        }
    }
}
