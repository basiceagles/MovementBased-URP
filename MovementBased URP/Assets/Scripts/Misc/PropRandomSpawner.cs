using UnityEngine;

public class PropRandomSpawner : MonoBehaviour
{
    [SerializeField] private GameObject[] prefabs;
    [SerializeField] [Range(0, 100)] private float spawnChance = 50f;

    [SerializeField] private bool isGrinderSpawner = false;
    [SerializeField] private bool isPropSpawner = false;

    private PerkSelectorScript perkSelectorScript;
    private GameObject perkSelectorScriptTarget;


    void Start()
    {
        perkSelectorScriptTarget = GameObject.Find("PerkSelectorManager");
        perkSelectorScript = perkSelectorScriptTarget.GetComponent<PerkSelectorScript>();

        if (isGrinderSpawner) spawnChance = perkSelectorScript.grinderSpawnChance;

        // Set unique name for this spawner
        //SetUniqueSpawnerName();

        if (perkSelectorScript.spawnGrinder && isGrinderSpawner)
        {
            SpawnProp();
        }

        if (perkSelectorScript.spawnProps && isPropSpawner)
        {
            SpawnProp();
        }
    }

    private void SpawnProp()
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

    private void SetUniqueSpawnerName()
    {
        string baseName = "PropSpawner";
        int counter = 1;
        string newName = baseName;

        // Find all PropRandomSpawner objects in the scene
        PropRandomSpawner[] allSpawners = FindObjectsOfType<PropRandomSpawner>();
        
        // Keep trying names until we find a unique one
        while (IsNameTaken(newName, allSpawners, this))
        {
            newName = $"{baseName} ({counter})";
            counter++;
        }

        // Set the unique name
        gameObject.name = newName;
    }

    private bool IsNameTaken(string name, PropRandomSpawner[] allSpawners, PropRandomSpawner currentSpawner)
    {
        foreach (PropRandomSpawner spawner in allSpawners)
        {
            // Check if another spawner (not this one) has the same name
            if (spawner != currentSpawner && spawner.gameObject.name == name)
            {
                return true;
            }
        }
        return false;
    }
}