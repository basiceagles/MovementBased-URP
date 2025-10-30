using UnityEngine;
using System.Collections;

public class Soul : MonoBehaviour
{
    [SerializeField] private float timeToAdd = 10f;
    [SerializeField] private Renderer objectRenderer;
    [SerializeField] private float effectDuration = 1f;
    [SerializeField] private GameObject collectSound;

    private Material material;
    private bool collected = false;
    private static readonly int ValueProperty = Shader.PropertyToID("_Value");
    
    private void Start()
    {
        if (objectRenderer != null)
        {
            material = objectRenderer.material;
            material.SetFloat(ValueProperty, 0f);
        }
    }
    
    public void Collect()
    {
        if (collected) return;
        collected = true;
        
        Timer timer = FindObjectOfType<Timer>();
        if (timer != null)
        {
            //timer.AddTime(timeToAdd);
        }
        
        if (collectSound != null)
        {
            collectSound.GetComponent<AudioSource>().Play();
        }
        
        StartCoroutine(DissolveEffect());
    }
    
    private IEnumerator DissolveEffect()
    {
        if (material != null)
        {
            float elapsed = 0f;
            while (elapsed < effectDuration)
            {
                elapsed += Time.deltaTime;
                float value = Mathf.Lerp(0f, 1f, elapsed / effectDuration);
                material.SetFloat(ValueProperty, value);
                yield return null;
            }
            
            material.SetFloat(ValueProperty, 1f);
        }
        
        Destroy(gameObject);
    }
}
