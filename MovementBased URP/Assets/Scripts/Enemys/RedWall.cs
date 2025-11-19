using UnityEngine;
using UnityEngine.SceneManagement;

public class RedWall : MonoBehaviour
{
    private GameObject playerTarget;
    private PlayerController playerController;
    private GameObject enemyOverlayManagerTarget;
    private EnemyOverlayManager enemyOverlayManager;
    [SerializeField] private string deadSceneName;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerTarget = GameObject.Find("Player");
        playerController = playerTarget.GetComponent<PlayerController>();
        enemyOverlayManagerTarget = GameObject.Find("EnemyOverlaysManager");
        enemyOverlayManager = enemyOverlayManagerTarget.GetComponent<EnemyOverlayManager>();
    }

    void OnTriggerEnter(Collider other)
    {
        if(!other.gameObject.CompareTag("Player")) return;

        if (playerController.isDamaged && other)
        {
            SceneManager.LoadScene(deadSceneName);
        }
        else
        {
            playerController.isDamaged = true;
            enemyOverlayManager.CallRedEyeOverlay();
        }
    }
}
