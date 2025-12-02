using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialChanger : MonoBehaviour
{    
    public Texture2D baseTexture1;
    public Texture2D baseTexture2;
    public Texture2D emissionTexture1;
    public Texture2D emissionTexture2;
    
    private Material cachedMaterial;
    private Renderer objectRenderer;

    private static readonly List<MaterialChanger> instances = new List<MaterialChanger>();
    private static Coroutine globalCoroutine;
    private static MaterialChanger hostInstance;
    private static bool useFirstSet = true;
    private const float SwitchInterval = 1f;

    void Awake()
    {
        objectRenderer = GetComponent<Renderer>();
        // Cache material instance (creates a copy)
        cachedMaterial = objectRenderer.material;

        instances.Add(this);

        if (hostInstance == null)
        {
            hostInstance = this;
        }

        StartGlobalRoutineIfNeeded();
    }

    private void StartGlobalRoutineIfNeeded()
    {
        if (hostInstance == this && globalCoroutine == null)
        {
            globalCoroutine = StartCoroutine(GlobalSwitchRoutine());
        }
    }

    private static IEnumerator GlobalSwitchRoutine()
    {
        while (true)
        {
            // Compact the list and apply to valid instances
            for (int i = instances.Count - 1; i >= 0; i--)
            {
                var changer = instances[i];
                if (changer != null)
                {
                    changer.ApplyCurrentTextures();
                }
                else
                {
                    instances.RemoveAt(i);
                }
            }

            if (instances.Count == 0)
            {
                break;
            }

            useFirstSet = !useFirstSet;

            yield return new WaitForSeconds(SwitchInterval);
        }

        globalCoroutine = null;
        hostInstance = null;
    }

    private void ApplyCurrentTextures()
    {
        if (cachedMaterial == null)
            return;

        var main = useFirstSet ? baseTexture1 : baseTexture2;
        cachedMaterial.mainTexture = main;

        var emission = useFirstSet ? emissionTexture1 : emissionTexture2;
        if (emission != null)
        {
            cachedMaterial.SetTexture("_EmissionText", emission);
        }
    }
    
    void OnDestroy()
    {
        instances.Remove(this);
        
        if (hostInstance == this)
        {
            hostInstance = null;

            if (instances.Count > 0)
            {
                hostInstance = instances[0];
                if (hostInstance != null)
                {
                    hostInstance.StartGlobalRoutineIfNeeded();
                }
            }
        }

        // Clean up the material instance
        if (cachedMaterial != null && Application.isPlaying)
        {
            Destroy(cachedMaterial);
        }
    }
}