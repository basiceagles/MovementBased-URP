using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class RedEyeDetector : MonoBehaviour
{
    [Header("Detection Settings")]
    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private string targetTag = "RedEye";
    [SerializeField] private LayerMask obstacleLayers = -1;
    [SerializeField] private float bufferTime = 0.1f;
    
    [Header("Fill Settings")]
    [SerializeField] private float fillDuration = 1f;
    [SerializeField] private float maxFillValue = 1f;
    
    [Header("Debug")]
    [SerializeField] private float currentFillValue = 0f;
    [SerializeField] private bool isRedEyeVisible = false;

    [SerializeField] private AudioSource audioSource;
    [SerializeField] private string deadSceneName;
    [SerializeField] private GameObject playerTarget;
    
    private Camera mainCamera;
    public List<GameObject> redEyeObjects;
    private GameObject currentRedEyeObject;
    private Coroutine fillCoroutine;
    private CameraShake cameraShake;
    private PlayerController playerController;
    private float lastVisibleTime = 0f;
    private GameObject enemyOverlayManagerTarget;
    private EnemyOverlayManager enemyOverlayManager;

    private GameObject globalVolumeTarget;
    private Volume globalVolume;

    private LensDistortion lensDistortion;
    private ChromaticAberration chromaticAberration;
    
    public System.Action<float> OnFillValueChanged;
    public System.Action OnFillComplete;
    public System.Action OnRedEyeDetected;
    public System.Action OnRedEyeLost;

    void Start()
    {
        playerController = playerTarget.GetComponent<PlayerController>();
        enemyOverlayManagerTarget = GameObject.Find("EnemyOverlaysManager");
        if (enemyOverlayManagerTarget != null) enemyOverlayManager = enemyOverlayManagerTarget.GetComponent<EnemyOverlayManager>();
        else Debug.LogWarning("EnemyOverlayManager is not set!");

        globalVolumeTarget = GameObject.Find("Global Volume");
        globalVolume = globalVolumeTarget.GetComponent<Volume>();

        globalVolume.profile.TryGet<LensDistortion>(out lensDistortion);
        globalVolume.profile.TryGet<ChromaticAberration>(out chromaticAberration);
        
        mainCamera = Camera.main;
        cameraShake = mainCamera.GetComponent<CameraShake>();
        currentFillValue = 0f;

        if (mainCamera == null)
        {
            Debug.LogError("No main camera found in scene!");
        }

        redEyeObjects = new List<GameObject>();
        Invoke("FindRedEyeGameobjects", 0.1f);
    }
    
    public void FindRedEyeGameobjects()
    {
        GameObject[] foundObjects = GameObject.FindGameObjectsWithTag(targetTag);
        
        if (foundObjects == null || foundObjects.Length == 0)
        {
            Debug.LogWarning($"No objects with tag '{targetTag}' found in scene.");
        }
        else
        {
            redEyeObjects.Clear();
            redEyeObjects.AddRange(foundObjects);
            Debug.Log($"Found {redEyeObjects.Count} RedEye objects in scene");
        }
    }
    
    void Update()
    {
        redEyeObjects.RemoveAll(obj => obj == null);

        // Store previous state
        bool wasVisible = isRedEyeVisible;
        GameObject previousRedEye = currentRedEyeObject;

        // Check visibility FIRST before finding new targets
        bool currentObjectIsVisible = currentRedEyeObject != null && IsObjectVisible(currentRedEyeObject);
        
        if (currentObjectIsVisible)
        {
            lastVisibleTime = Time.time;
        }

        // Find current target
        FindCurrentRedEye();

        if (currentRedEyeObject == null || mainCamera == null) 
        {
            if (wasVisible)
            {
                StopFillCoroutine();
            }
            return;
        }

        // Apply buffer and update visibility state
        bool isVisibleWithBuffer = currentObjectIsVisible || (Time.time - lastVisibleTime <= bufferTime);
        isRedEyeVisible = isVisibleWithBuffer;

        // Handle visibility changes immediately
        if (!wasVisible && isRedEyeVisible)
        {
            StartFillCoroutine();
            OnRedEyeDetected?.Invoke();
        }
        else if (wasVisible && !isRedEyeVisible)
        {
            StopFillCoroutine();
            OnRedEyeLost?.Invoke();
        }

        audioSource.volume = currentFillValue;

        if (currentFillValue == maxFillValue && currentRedEyeObject != null)
        {
            StopFillCoroutine();
            Destroy(currentRedEyeObject);
            redEyeObjects.Remove(currentRedEyeObject);
            currentRedEyeObject = null;

            if (playerController.isDamaged)
            {
                SceneManager.LoadScene(deadSceneName);
            }
            else
            {
                playerController.isDamaged = true;
                enemyOverlayManager.CallRedEyeOverlay();
            }
        }

        if (lensDistortion != null)
            lensDistortion.intensity.value = -currentFillValue;
        
        if (chromaticAberration != null)
            chromaticAberration.intensity.value = currentFillValue;
    }

    private void FindCurrentRedEye()
    {
        GameObject closestVisibleRedEye = null;
        float closestDistance = Mathf.Infinity;
        bool foundCurrentObject = false;

        foreach (GameObject redEye in redEyeObjects)
        {
            if (redEye == null) continue;

            if (IsObjectVisible(redEye))
            {
                float distance = Vector3.Distance(mainCamera.transform.position, redEye.transform.position);
                
                // Check if this is our current object
                if (redEye == currentRedEyeObject)
                {
                    foundCurrentObject = true;
                }
                
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestVisibleRedEye = redEye;
                }
            }
        }

        // If current object is still visible, keep it
        if (foundCurrentObject && currentRedEyeObject != null)
        {
            // Current object is still visible, no need to change
            return;
        }

        // If we found a different visible RedEye
        if (closestVisibleRedEye != null && closestVisibleRedEye != currentRedEyeObject)
        {
            // If we were tracking a different RedEye, stop the current fill
            if (currentRedEyeObject != null && fillCoroutine != null)
            {
                StopFillCoroutine();
            }
            
            currentRedEyeObject = closestVisibleRedEye;
            lastVisibleTime = Time.time;
            
            // Don't start fill here - let Update handle it immediately
        }
        // If no RedEye is visible at all
        else if (closestVisibleRedEye == null && currentRedEyeObject != null)
        {
            // Current object is no longer visible - this will be handled in Update
        }
    }
    
    private bool IsObjectVisible(GameObject targetObject)
    {
        if (targetObject == null) return false;
        
        float distance = Vector3.Distance(mainCamera.transform.position, targetObject.transform.position);
        if (distance > detectionRange) return false;
        
        Vector3 screenPoint = mainCamera.WorldToViewportPoint(targetObject.transform.position);

        // Use slightly larger bounds for better edge detection
        bool onScreen = screenPoint.z > 0 &&
                       screenPoint.x >= -0.05f && screenPoint.x <= 1.05f &&
                       screenPoint.y >= -0.05f && screenPoint.y <= 1.05f;
        
        if (!onScreen) return false;
        
        return HasLineOfSight(targetObject);
    }
    
    private bool HasLineOfSight(GameObject targetObject)
    {
        Vector3 cameraPosition = mainCamera.transform.position;
        Vector3 targetPosition = targetObject.transform.position;
        
        Vector3 billboardForward = targetObject.transform.forward;
        Vector3 adjustedTargetPosition = targetPosition + billboardForward * 0.1f;
        
        Vector3 direction = (adjustedTargetPosition - cameraPosition).normalized;
        float distance = Vector3.Distance(cameraPosition, adjustedTargetPosition);
        
        RaycastHit hit;
        if (Physics.Raycast(cameraPosition, direction, out hit, Mathf.Infinity, obstacleLayers, QueryTriggerInteraction.Ignore))
        {
            if (hit.collider.gameObject == targetObject || 
                hit.collider.transform.IsChildOf(targetObject.transform))
            {
                return true;
            }
            else if (hit.distance < distance)
            {
                return false; // Препятствие между камерой и RedEye
            }
        }
        
        return true;
    }
    
    private void StartFillCoroutine()
    {
        if (fillCoroutine != null)
        {
            StopCoroutine(fillCoroutine);
        }
        
        fillCoroutine = StartCoroutine(FillCoroutine());

        if (!audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }
    
    private void StopFillCoroutine()
    {
        if (fillCoroutine != null)
        {
            StopCoroutine(fillCoroutine);
            fillCoroutine = null;
        }
        
        currentFillValue = 0f;
        OnFillValueChanged?.Invoke(currentFillValue);

        audioSource.Stop();
    }
    
    private IEnumerator FillCoroutine()
    {
        float startValue = currentFillValue;
        float timer = 0f;
        float targetValue = maxFillValue;
        
        while (timer < fillDuration && isRedEyeVisible && currentRedEyeObject != null)
        {
            timer += Time.deltaTime;
            float progress = Mathf.Clamp01(timer / fillDuration);
            
            float newFillValue = Mathf.Lerp(startValue, targetValue, progress);
            
            if (newFillValue != currentFillValue)
            {
                currentFillValue = newFillValue;
                OnFillValueChanged?.Invoke(currentFillValue);
            }
            
            yield return null;
        }
        
        if (isRedEyeVisible && currentRedEyeObject != null && currentFillValue < targetValue)
        {
            currentFillValue = targetValue;
            OnFillValueChanged?.Invoke(currentFillValue);
            OnFillComplete?.Invoke();
        }
        
        fillCoroutine = null;
    }
}