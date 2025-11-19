using UnityEngine;

public class EnemyManagerCaller : MonoBehaviour
{
    private GameObject enemyManagerTarget;
    private EnemyManager enemyManager;
    void Start()
    {
        enemyManagerTarget = GameObject.Find("EnemySpawnerManager");
        enemyManager = enemyManagerTarget.GetComponent<EnemyManager>();
    }

    public void CallSpeedUpRunnerMethod()
    {
        enemyManager.SpeedUpRunner();
    }
}
