using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    [Header("Runner Settings")]
    public GameObject runnerPrefab;
    public GameObject runnerSpawnPoint;
    public float runnerSpeed;
    public float runnerRushSpeed;
    public float runnerSearchRadius;
    private GameObject spawnedRunnerGameobject;
    private RunnerEnemy runnerEnemy;

    void Start()
    {
        
    }

    public void SpawnRunner()
    {
        //Спавним и берем рефернсы для раннера, чтобы их потом менять
        if (runnerPrefab != null)
        {
            Transform spawnpointTransform;
            spawnpointTransform = runnerSpawnPoint.GetComponent<Transform>();
            spawnedRunnerGameobject = Instantiate(runnerPrefab, spawnpointTransform.position, runnerPrefab.transform.rotation);

            //от этого скрипта будут подвязаны все показатели врагов, поэтому это обязательно
            runnerEnemy = spawnedRunnerGameobject.GetComponent<RunnerEnemy>();

            runnerEnemy.moveSpeed = runnerSpeed;
            runnerEnemy.searchRadius = runnerSearchRadius;
        }
    }

    //раннер летит как бешенный когда игрок дошел до триггера сейфзоны
    public void SpeedUpRunner()
    {
        runnerEnemy.moveSpeed = runnerRushSpeed;
    }
}
