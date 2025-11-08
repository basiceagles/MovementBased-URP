using UnityEngine;

public class RunnerSpawnerTrigger : MonoBehaviour
{
    private EnemyManager enemyManager;
    private GameObject enemyManagerTarget;
    public GameObject spawnPoint;

    void Start()
    {
        enemyManagerTarget = GameObject.Find("EnemySpawnerManager");
        enemyManager = enemyManagerTarget.GetComponent<EnemyManager>();
    }

    public void CallSpawnRunner()
    {
        enemyManager.runnerSpawnPoint = spawnPoint;
        enemyManager.SpawnRunner();
    } 

}