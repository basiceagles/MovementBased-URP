using UnityEngine;
using UnityEngine.Rendering.Universal;

public class GlitchController : MonoBehaviour
{
    [SerializeField] private Material material;
    [SerializeField] private float noiseAmount;
    [SerializeField] private float glitchStrenght;
    [SerializeField] private float scanLinesStrenght;
    [SerializeField] private Color color;
    [SerializeField] private bool useGlitchEffect;
    [SerializeField] private UniversalRendererData rendererData;
    [SerializeField] ScriptableRendererFeature feature;

    void Start()
    {
        feature.SetActive(useGlitchEffect);
    }
    // Update is called once per frame
    void Update()
    {
        material.SetFloat("_NoiseAmount", noiseAmount);
        material.SetFloat("_GlitchStrenght", glitchStrenght);
        material.SetFloat("_ScanLinesStrenght", scanLinesStrenght); 
        material.SetColor("_Color", color);
    }
}
