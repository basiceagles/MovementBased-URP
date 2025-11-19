using UnityEngine;

public class SpeedUpRunner : MonoBehaviour
{
    private GameObject enemyManagerTarget;
    private EnemyManager enemyManager;

    void Start()
    {
        enemyManagerTarget = GameObject.Find("EnemySpawnerManager");
        enemyManager = enemyManagerTarget.GetComponent<EnemyManager>();
    }

    public void CallSpeeedUpRunner()
    {
        enemyManager.SpeedUpRunner();
    }
}
