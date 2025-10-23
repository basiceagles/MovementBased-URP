using UnityEngine;

public class PropRandomSpawner : MonoBehaviour
{
    [SerializeField] private GameObject[] prefabs;

    void Awake()
    {
        if (prefabs != null && prefabs.Length > 0)
        {
            int randomIndex = Random.Range(0, prefabs.Length);
            if (prefabs[randomIndex] != null)
            {
                Instantiate(prefabs[randomIndex], transform.position, transform.rotation);
            }
        }
    }
}