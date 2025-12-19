using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Renderer))]
public class MaterialChanger : MonoBehaviour
{
    [Header("Texture Settings")]
    public Material materialTemplate;
    public Texture2D baseTexture1;
    public Texture2D baseTexture2;
    public Texture2D emissionTexture1;
    public Texture2D emissionTexture2;
    [Range(0.1f, 5f)] public float switchInterval = 1f;

    [Header("Occlusion Settings")]
    public bool useOcclusion = true;
    public string playerTag = "Player";
    [Range(5f, 100f)] public float maxVisibleDistance = 30f;
    [Range(0.1f, 2f)] public float visibilityCheckInterval = 0.5f;

    private Material[] materialInstances;
    private Renderer objectRenderer;
    private Transform playerTransform;
    private bool isVisible = true;
    private float nextSwitchTime;
    private float nextVisibilityCheck;
    private bool useFirstSet = true;
    private Coroutine initCoroutine;

    private static readonly int BaseMapId = Shader.PropertyToID("_BaseMap");
    private static readonly int MainTexId = Shader.PropertyToID("_MainTex");
    private static readonly int EmissionMapId = Shader.PropertyToID("_EmissionMap");

    private bool EnsureMaterialInstances()
    {
        if (objectRenderer == null)
            objectRenderer = GetComponent<Renderer>();

        if (objectRenderer == null)
            return false;

        var currentMaterials = objectRenderer.sharedMaterials;
        if (materialInstances != null && materialInstances.Length > 0)
        {
            if (currentMaterials != null && currentMaterials.Length == materialInstances.Length)
                return true;
        }

        var sourceMaterials = currentMaterials;
        if (sourceMaterials == null || sourceMaterials.Length == 0)
        {
            var single = objectRenderer.sharedMaterial;
            if (single == null)
                single = objectRenderer.material;

            if (single == null)
                return false;

            sourceMaterials = new[] { single };
        }

        if (materialTemplate != null)
        {
            var slotCount = sourceMaterials.Length;
            if (slotCount <= 0)
                slotCount = 1;

            materialInstances = new Material[slotCount];
            for (int i = 0; i < slotCount; i++)
            {
                materialInstances[i] = new Material(materialTemplate);
            }

            objectRenderer.materials = materialInstances;
            return true;
        }

        materialInstances = new Material[sourceMaterials.Length];
        for (int i = 0; i < sourceMaterials.Length; i++)
        {
            var src = sourceMaterials[i];
            materialInstances[i] = src != null ? new Material(src) : null;
        }

        objectRenderer.materials = materialInstances;
        return true;
    }

    private void Awake()
    {
        objectRenderer = GetComponent<Renderer>();
        if (objectRenderer == null)
        {
            Debug.LogWarning($"[MaterialChanger] No Renderer component found on {name}");
            enabled = false;
            return;
        }

        // Initialize next switch time
        nextSwitchTime = Time.time + switchInterval;
    }

    private void OnEnable()
    {
        if (initCoroutine != null)
        {
            StopCoroutine(initCoroutine);
            initCoroutine = null;
        }

        initCoroutine = StartCoroutine(DelayedInitialize());

        // Reset switch timer when re-enabled
        nextSwitchTime = Time.time + switchInterval;
    }

    private IEnumerator DelayedInitialize()
    {
        yield return null;

        if (!EnsureMaterialInstances())
        {
            Debug.LogWarning($"[MaterialChanger] No Material found on Renderer for {name}");
            enabled = false;
            yield break;
        }

        UpdateMaterialTextures();
        initCoroutine = null;
    }

    private void Start()
    {
        // Find player if occlusion is enabled
        if (useOcclusion && string.IsNullOrEmpty(playerTag) == false)
        {
            var playerObj = GameObject.FindGameObjectWithTag(playerTag);
            if (playerObj != null)
            {
                playerTransform = playerObj.transform;
            }
        }
    }

    private void Update()
    {
        if (objectRenderer != null)
        {
            var mats = objectRenderer.sharedMaterials;
            if (mats == null || mats.Length == 0)
            {
                if (EnsureMaterialInstances())
                    UpdateMaterialTextures();
            }
            else if (materialInstances == null || materialInstances.Length != mats.Length)
            {
                if (EnsureMaterialInstances())
                    UpdateMaterialTextures();
            }
            else
            {
                for (int i = 0; i < mats.Length; i++)
                {
                    if (mats[i] == null)
                    {
                        if (EnsureMaterialInstances())
                            UpdateMaterialTextures();
                        break;
                    }
                }
            }
        }

        // Handle occlusion if enabled
        if (useOcclusion)
        {
            UpdateVisibility();
        }

        // Check if it's time to switch textures
        if (Time.time >= nextSwitchTime)
        {
            useFirstSet = !useFirstSet;
            UpdateMaterialTextures();
            nextSwitchTime = Time.time + switchInterval;
        }
    }

    private void UpdateVisibility()
    {
        // Throttle visibility checks
        if (Time.time < nextVisibilityCheck) return;
        nextVisibilityCheck = Time.time + visibilityCheckInterval;

        if (playerTransform == null)
        {
            // Try to find player again if we lost reference
            var playerObj = GameObject.FindGameObjectWithTag(playerTag);
            if (playerObj != null)
            {
                playerTransform = playerObj.transform;
            }
            return;
        }

        // Check distance to player
        bool wasVisible = isVisible;
        isVisible = Vector3.Distance(transform.position, playerTransform.position) <= maxVisibleDistance;

        // If just became visible, update the material
        if (isVisible && !wasVisible)
        {
            UpdateMaterialTextures();
        }
    }

    private void UpdateMaterialTextures()
    {
        if (materialInstances == null || materialInstances.Length == 0 || (useOcclusion && !isVisible)) 
            return;

        try
        {
            // Apply base texture
            var mainTexture = useFirstSet ? baseTexture1 : baseTexture2;
            var emissionTexture = useFirstSet ? emissionTexture1 : emissionTexture2;

            for (int i = 0; i < materialInstances.Length; i++)
            {
                var mat = materialInstances[i];
                if (mat == null)
                    continue;

                if (mainTexture != null)
                {
                    mat.mainTexture = mainTexture;
                    if (mat.HasProperty(BaseMapId))
                        mat.SetTexture(BaseMapId, mainTexture);
                    if (mat.HasProperty(MainTexId))
                        mat.SetTexture(MainTexId, mainTexture);
                }

                // Apply emission texture if available
                if (emissionTexture != null)
                {
                    if (mat.HasProperty(EmissionMapId))
                        mat.SetTexture(EmissionMapId, emissionTexture);
                    mat.EnableKeyword("_EMISSION");
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[MaterialChanger] Error updating textures on {name}: {e.Message}");
        }
    }

    private void OnDestroy()
    {
        // Clean up material instance
        if (materialInstances != null)
        {
            for (int i = 0; i < materialInstances.Length; i++)
            {
                var mat = materialInstances[i];
                if (mat == null)
                    continue;

                if (Application.isPlaying)
                {
                    Destroy(mat);
                }
                else
                {
                    DestroyImmediate(mat);
                }
            }
        }
    }

    // Helper method to manually trigger a texture update
    public void ForceUpdateMaterial()
    {
        UpdateMaterialTextures();
    }

    // Helper method to manually toggle the texture set
    public void ToggleTextureSet(bool useFirst)
    {
        useFirstSet = useFirst;
        UpdateMaterialTextures();
    }
}