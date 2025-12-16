using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class MaterialChanger : MonoBehaviour
{
    [Header("Texture Settings")]
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

    private Material materialInstance;
    private Renderer objectRenderer;
    private Transform playerTransform;
    private bool isVisible = true;
    private float nextSwitchTime;
    private float nextVisibilityCheck;
    private bool useFirstSet = true;

    private void Awake()
    {
        objectRenderer = GetComponent<Renderer>();
        if (objectRenderer == null)
        {
            Debug.LogWarning($"[MaterialChanger] No Renderer component found on {name}");
            enabled = false;
            return;
        }

        // Create a unique material instance
        materialInstance = new Material(objectRenderer.sharedMaterial);
        objectRenderer.material = materialInstance;

        // Initialize next switch time
        nextSwitchTime = Time.time + switchInterval;
    }

    private void OnEnable()
    {
        if (materialInstance == null) return;

        // Initial material update
        UpdateMaterialTextures();

        // Reset switch timer when re-enabled
        nextSwitchTime = Time.time + switchInterval;
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
        if (materialInstance == null || (useOcclusion && !isVisible)) 
            return;

        try
        {
            // Apply base texture
            var mainTexture = useFirstSet ? baseTexture1 : baseTexture2;
            if (mainTexture != null)
            {
                materialInstance.mainTexture = mainTexture;
            }

            // Apply emission texture if available
            var emissionTexture = useFirstSet ? emissionTexture1 : emissionTexture2;
            if (emissionTexture != null)
            {
                materialInstance.SetTexture("_EmissionText", emissionTexture);
                materialInstance.EnableKeyword("_EMISSION");
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
        if (materialInstance != null)
        {
            if (Application.isPlaying)
            {
                Destroy(materialInstance);
            }
            else
            {
                DestroyImmediate(materialInstance);
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