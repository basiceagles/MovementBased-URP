using UnityEngine;

public class PropRandomSpawner : MonoBehaviour
{
    [SerializeField] private GameObject[] prefabs;
    [SerializeField] [Range(0, 100)] private float spawnChance = 50f;

    void Awake()
    {
        if (prefabs != null && prefabs.Length > 0)
        {
            // Check if we should spawn based on chance
            float randomValue = Random.Range(0f, 100f);
            if (randomValue <= spawnChance)
            {
                int randomIndex = Random.Range(0, prefabs.Length);
                if (prefabs[randomIndex] != null)
                {
                    Instantiate(prefabs[randomIndex], transform.position, transform.rotation, transform);
                }
            }
        }
    }
}