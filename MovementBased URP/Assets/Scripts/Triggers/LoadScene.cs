using UnityEngine;
using UnityEngine.SceneManagement;
public class LoadScene : MonoBehaviour
{
    public string sceneName;
    public void LoadSceneSpecific()
    {
        SceneManager.LoadScene(sceneName);
    }
}
