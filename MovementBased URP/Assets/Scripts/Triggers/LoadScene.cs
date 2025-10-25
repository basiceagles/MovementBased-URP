using UnityEngine;
using UnityEngine.SceneManagement;
public class LoadScene : MonoBehaviour
{
    public bool loadScene = true;
    public bool restartScene = false;
    public string sceneName;
    private string currentScene;

    void Start()
    {
        currentScene = SceneManager.GetActiveScene().name;
    }
    
    public void LoadSceneSpecific()
    {
        if (loadScene)
        {
            SceneManager.LoadScene(sceneName);
        }
        if (restartScene)
        {
            SceneManager.LoadScene(currentScene);
        }
    }
}
