using System.Collections;
using UnityEngine;

public class ScannerScript : MonoBehaviour
{
    [Header("Scanner Settings")]
    [SerializeField] private GameObject scannerPrefab;
    [SerializeField] private float growSpeed = 1f;
    [SerializeField] private float destroyDelay = 3f;

    [Header("Spawn Settings")]
    [SerializeField] private int scanCount = 3;
    [SerializeField] private float spawnDelay = 0.1f;

    private bool isScanning;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F) && !isScanning)
        {
            StartCoroutine(SpawnScans());
        }
    }

    private IEnumerator SpawnScans()
    {
        isScanning = true;

        for (int i = 0; i < scanCount; i++)
        {
            GameObject scan = Instantiate(scannerPrefab, transform.position, Quaternion.identity);
            StartCoroutine(GrowScan(scan));
            Destroy(scan, destroyDelay);

            yield return new WaitForSeconds(spawnDelay);
        }

        isScanning = false;
    }

    private IEnumerator GrowScan(GameObject scan)
    {
        while (scan != null)
        {
            scan.transform.localScale += Vector3.one * growSpeed * Time.deltaTime;
            yield return null;
        }
    }
}
