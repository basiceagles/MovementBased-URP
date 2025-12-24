using UnityEngine;

public class ScannerSript : MonoBehaviour
{
    [Header("Scanner Settings")]
    [SerializeField] private GameObject scannerPrefab;
    [SerializeField] private float growSpeed = 1f;
    [SerializeField] private float destroyDelay = 3f;

    private GameObject spawnedScanner;

    void Update()
    {
        // Spawn scanner on F press
        if (Input.GetKeyDown(KeyCode.F) && spawnedScanner == null)
        {
            spawnedScanner = Instantiate(scannerPrefab, transform.position, Quaternion.identity);
            Destroy(spawnedScanner, destroyDelay);
        }

        // Grow scanner while it exists
        if (spawnedScanner != null)
        {
            float growAmount = growSpeed * Time.deltaTime;
            spawnedScanner.transform.localScale += Vector3.one * growAmount;
        }
    }
}
