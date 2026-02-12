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
    [SerializeField] private string blueEyeTag = "BlueEye";
    [SerializeField] private float blueEyeRadius = 10f;
    [SerializeField] private float blueEyeFillDuration = 0.8f;
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
    
    [Header("Camera Pull Settings")]
    [SerializeField] private bool enableCameraPull = true;
    [SerializeField] private float pullStrength = 3f;
    [SerializeField] private LayerMask playerLayer;
    private CameraController cameraController;
    private HashSet<GameObject> eyesSighted = new HashSet<GameObject>();
    
    private Camera mainCamera;
    public List<GameObject> redEyeObjects;
    public List<GameObject> blueEyeObjects;
    private GameObject currentRedEyeObject;
    private GameObject currentBlueEyeObject;
    private Coroutine fillCoroutine;
    private bool isInsideBlueEyeRadius = false;
    private bool hasLookedAtCurrentBlueEye = false;
    private float currentBlueEyeFillValue = 0f;
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

        globalVolumeTarget = GameObject.Find("Global Volume");
        globalVolume = globalVolumeTarget.GetComponent<Volume>();

        globalVolume.profile.TryGet<LensDistortion>(out lensDistortion);
        globalVolume.profile.TryGet<ChromaticAberration>(out chromaticAberration);
        
        mainCamera = Camera.main;
        cameraShake = mainCamera.GetComponent<CameraShake>();
        cameraController = mainCamera.GetComponent<CameraController>();
        currentFillValue = 0f;

        redEyeObjects = new List<GameObject>();
        Invoke("FindRedEyeGameobjects", 0.1f);

        blueEyeObjects = new List<GameObject>();
    }
    
    public void FindRedEyeGameobjects()
    {
        GameObject[] foundObjects = GameObject.FindGameObjectsWithTag(targetTag);
        
        if (foundObjects != null && foundObjects.Length > 0)
        {
            redEyeObjects.Clear();
            redEyeObjects.AddRange(foundObjects);
        }
    }
    
    public void OnBlueEyeEnter(GameObject blueEye)
    {
        if (blueEye == null)
            return;

        if (blueEyeObjects == null)
        {
            blueEyeObjects = new List<GameObject>();
        }

        if (!blueEyeObjects.Contains(blueEye))
        {
            blueEyeObjects.Add(blueEye);
        }

        isInsideBlueEyeRadius = true;

        if (currentBlueEyeObject == null)
        {
            currentBlueEyeObject = blueEye;
            hasLookedAtCurrentBlueEye = false;
            currentBlueEyeFillValue = 0f;
        }
    }

    public void OnBlueEyeExit(GameObject blueEye)
    {
        if (blueEye == null)
            return;

        if (blueEyeObjects != null && blueEyeObjects.Contains(blueEye))
        {
            blueEyeObjects.Remove(blueEye);
        }

        if (currentBlueEyeObject == blueEye)
        {
            bool everLooked = hasLookedAtCurrentBlueEye;

            currentBlueEyeObject = null;
            currentBlueEyeFillValue = 0f;
            hasLookedAtCurrentBlueEye = false;
            isInsideBlueEyeRadius = false;

            if (!everLooked)
            {
                Destroy(blueEye);

                if (playerController.isDamaged)
                {
                    MoneyManager.SaveMoneyOnDeath();
                    SceneManager.LoadScene(deadSceneName);
                }
                else
                {
                    playerController.isDamaged = true;
                    enemyOverlayManager.CallRedEyeOverlay();
                }
            }
        }
    }

    private void HandleBlueEyeLogic()
    {
        if (playerTarget == null || mainCamera == null)
        {
            return;
        }
        if (!isInsideBlueEyeRadius || currentBlueEyeObject == null)
        {
            return;
        }

        bool isLookingAtBlueEye = IsObjectVisible(currentBlueEyeObject);

        if (isLookingAtBlueEye)
        {
            hasLookedAtCurrentBlueEye = true;
            currentBlueEyeFillValue += Time.deltaTime / blueEyeFillDuration;
            currentBlueEyeFillValue = Mathf.Clamp01(currentBlueEyeFillValue);

            if (currentBlueEyeFillValue >= 1f)
            {
                GameObject toDestroy = currentBlueEyeObject;
                blueEyeObjects.Remove(toDestroy);
                currentBlueEyeObject = null;
                currentBlueEyeFillValue = 0f;
                hasLookedAtCurrentBlueEye = false;

                Destroy(toDestroy);
            }
        }
    }

    void Update()
    {
        HandleCameraPull();
        HandleBlueEyeLogic();

        redEyeObjects.RemoveAll(obj => obj == null);

        bool wasVisible = isRedEyeVisible;
        GameObject previousRedEye = currentRedEyeObject;

        bool currentObjectIsVisible = currentRedEyeObject != null && IsObjectVisible(currentRedEyeObject);
        
        if (currentObjectIsVisible)
        {
            lastVisibleTime = Time.time;
        }

        FindCurrentRedEye();

        if (currentRedEyeObject == null || mainCamera == null) 
        {
            if (wasVisible)
            {
                StopFillCoroutine();
            }
            return;
        }

        bool isVisibleWithBuffer = currentObjectIsVisible || (Time.time - lastVisibleTime <= bufferTime);
        isRedEyeVisible = isVisibleWithBuffer;

        bool hasTargetChanged = currentRedEyeObject != previousRedEye;

        if ((!wasVisible && isRedEyeVisible) || (isRedEyeVisible && hasTargetChanged))
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
                MoneyManager.SaveMoneyOnDeath();
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

        if (foundCurrentObject && currentRedEyeObject != null)
        {
            return;
        }

        if (closestVisibleRedEye != null && closestVisibleRedEye != currentRedEyeObject)
        {
            if (currentRedEyeObject != null && fillCoroutine != null)
            {
                StopFillCoroutine();
            }
            
            currentRedEyeObject = closestVisibleRedEye;
            lastVisibleTime = Time.time;
            
        }
    }
    
    private bool IsObjectVisible(GameObject targetObject)
    {
        if (targetObject == null) return false;
        
        float distance = Vector3.Distance(mainCamera.transform.position, targetObject.transform.position);
        if (distance > detectionRange) return false;
        
        Vector3 screenPoint = mainCamera.WorldToViewportPoint(targetObject.transform.position);

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
                return false;
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

    private void HandleCameraPull()
    {
        if (!enableCameraPull || mainCamera == null || cameraController == null) return;

        GameObject activeObserver = null;

        foreach (GameObject redEye in redEyeObjects)
        {
            if (redEye == null) continue;

            if (CanRedEyeSeePlayer(redEye))
            {
                if (!eyesSighted.Contains(redEye))
                {
                    eyesSighted.Add(redEye);
                }

                activeObserver = redEye;
                break;
            }
            else
            {
                eyesSighted.Remove(redEye);
            }
        }
        if (activeObserver != null)
        {
            ApplyPullToTarget(activeObserver);
        }
    }

    private bool CanRedEyeSeePlayer(GameObject redEye)
    {
        Vector3 eyePos = redEye.transform.position;
        Vector3 playerPos = mainCamera.transform.position;
        Vector3 dir = (playerPos - eyePos).normalized;
        float dist = Vector3.Distance(eyePos, playerPos);

        if (dist > detectionRange) return false;

        RaycastHit hit;
        LayerMask combinedMask = obstacleLayers | playerLayer;

        if (Physics.Raycast(eyePos, dir, out hit, dist + 1f, combinedMask, QueryTriggerInteraction.Ignore))
        {
            if (((1 << hit.collider.gameObject.layer) & playerLayer.value) != 0 || hit.collider.CompareTag("Player"))
            {
                return true;
            }
        }
        
        return false;
    }

    private void ApplyPullToTarget(GameObject target)
    {
        Vector3 dirToTarget = (target.transform.position - mainCamera.transform.position).normalized;
        
        Vector3 currentForward = mainCamera.transform.forward;
        currentForward.y = 0;
        Vector3 targetForward = dirToTarget;
        targetForward.y = 0;
        float angleYaw = Vector3.SignedAngle(currentForward, targetForward, Vector3.up);
        
        float currentPitch = mainCamera.transform.localEulerAngles.x;
        if (currentPitch > 180) currentPitch -= 360;
        float targetPitch = -Mathf.Asin(dirToTarget.y) * Mathf.Rad2Deg;
        float anglePitch = targetPitch - currentPitch;
        cameraController.AddExternalRotation(angleYaw * pullStrength * Time.deltaTime, -anglePitch * pullStrength * Time.deltaTime);
    }
}