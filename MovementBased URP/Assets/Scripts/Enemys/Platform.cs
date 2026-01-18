using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Platform : MonoBehaviour
{
    [SerializeField] private GameObject[] platforms; 
    [SerializeField] private float fallDelay;    
    [SerializeField] private float fallSpeed;   
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private float shakeDuration; 
    [SerializeField] private float shakeFrequency;
    [SerializeField] private float shakeAmount; 
    [SerializeField] private float hopHeight;    
    [SerializeField] private Vector3 spinAmount = new Vector3(30, 60, 30);
    [SerializeField] private bool scaleDown = true;    

    private List<GameObject> activePlatforms = new List<GameObject>();
    private bool isUsed;

    private void OnTriggerEnter(Collider other)
    {
        if (isUsed || !other.CompareTag(playerTag)) 
        {
            return;
        }
        isUsed = true;

        foreach (var p in platforms) 
        {
            p.SetActive(false);
        }
        activePlatforms.Clear();

        int countToActivate = Random.Range(2, 5);
        activePlatforms = platforms.OrderBy(x => Random.value).Take(countToActivate).ToList();

        foreach (var p in activePlatforms) 
        {
            p.SetActive(true);
        }

        StartCoroutine(FallSequence());
    }

    private IEnumerator FallSequence()
    {
        yield return new WaitForSeconds(fallDelay);

        int totalActive = activePlatforms.Count;
        int countToFall = Random.Range(1, totalActive);

        var platformsToFall = activePlatforms.OrderBy(x => Random.value).Take(countToFall).ToList();

        foreach (var p in platformsToFall)
        {
            StartCoroutine(AnimateBeautifulFall(p));
            yield return new WaitForSeconds(0.15f); 
        }
    }

    private IEnumerator AnimateBeautifulFall(GameObject platform)
    {
        Vector3 startPos = platform.transform.position;
        Vector3 startScale = platform.transform.localScale;
        float elapsed = 0;

        while (elapsed < shakeDuration)
        {
            elapsed += Time.deltaTime;
            float percent = elapsed / shakeDuration;
            
            float currentShake = shakeAmount * percent;
            Vector3 offset = new Vector3(
                Mathf.Sin(Time.time * shakeFrequency) * currentShake,
                Mathf.Cos(Time.time * shakeFrequency * 1.2f) * currentShake,
                Mathf.Sin(Time.time * shakeFrequency * 0.8f) * currentShake
            );
            
            platform.transform.position = startPos + offset;
            yield return null;
        }

        float hopElapsed = 0;
        float hopDuration = 0.15f;
        while (hopElapsed < hopDuration)
        {
            hopElapsed += Time.deltaTime;
            float t = hopElapsed / hopDuration;
            float height = Mathf.Sin(t * Mathf.PI) * hopHeight;
            platform.transform.position = startPos + Vector3.up * height;
            yield return null;
        }

        float fallElapsed = 0;
        float maxFallTime = 2.5f;
        Vector3 randomSpin = new Vector3(
            Random.Range(-spinAmount.x, spinAmount.x),
            Random.Range(-spinAmount.y, spinAmount.y),
            Random.Range(-spinAmount.z, spinAmount.z)
        );

        while (fallElapsed < maxFallTime)
        {
            fallElapsed += Time.deltaTime;
            float t = fallElapsed / maxFallTime;

            platform.transform.position += Vector3.down * (fallSpeed * fallElapsed * Time.deltaTime);
            
            platform.transform.Rotate(randomSpin * Time.deltaTime);
            if (scaleDown)
            {
                platform.transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);
            }

            yield return null;
        }

        platform.SetActive(false);
        platform.transform.localScale = startScale; 
    }
}
