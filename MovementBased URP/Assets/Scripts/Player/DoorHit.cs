using UnityEngine;
using System.Collections;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class DoorHit : MonoBehaviour
{
    [Header("Settings")]
    public float minSpeed = 11f;
    public float kickForce = 25f;
    public float kickUpForce = 5f;
    public float interactionDistance = 4f;
    public KeyCode kickKey = KeyCode.E;

    [Header("Slow-Mo & Visuals")]
    public float slowMoTimeScale = 0.5f;
    public float slowMoTransitionSpeed = 5f;
    public float slowMoFovChange = 10f;
    public float maxChromatic = 0.8f;
    public float maxLensDistortion = -0.3f;
    public float maxVignette = 0.4f;
    public float maxBloom = 2.0f;

    [Header("References")]
    public Collider triggerCollider;
    public GameObject doorObject;

    private PlayerController _player;
    private Rigidbody _doorRb;
    private Camera _cam;
    private CameraShake _shake;
    private SpeedFOVEffect _speedFov;
    private ChromaticAberration _chromatic;
    private LensDistortion _lens;
    private Vignette _vignette;
    private Bloom _bloom;

    private bool _isKicked;
    private bool _isPlayerInside;
    private float _entrySpeed;
    private float _effectLerp;
    private float _lerpVel;
    private float _timeVel;
    private float _origFov;
    private float _origFixedDelta;
    private float _audioTimer;
    private AudioSource[] _cachedAudio;

    private void Start()
    {
        _player = FindObjectOfType<PlayerController>();

        if (doorObject == null)
        {
            doorObject = gameObject;
        }

        _doorRb = doorObject.GetComponent<Rigidbody>();
        
        if (Camera.main != null)
        {
            _cam = Camera.main;
            _origFov = _cam.fieldOfView;
            _shake = _cam.GetComponent<CameraShake>();
            _speedFov = _cam.GetComponent<SpeedFOVEffect>();
        }

        _origFixedDelta = Time.fixedDeltaTime;

        GameObject volObj = GameObject.Find("Global Volume");
        if (volObj != null)
        {
            if (volObj.TryGetComponent(out Volume vol))
            {
                vol.profile.TryGet(out _chromatic);
                vol.profile.TryGet(out _lens);
                vol.profile.TryGet(out _vignette);
                vol.profile.TryGet(out _bloom);
            }
        }
    }

    private void Update()
    {
        // 1. Pause & Error Guard
        if (Settings.Instance != null)
        {
            if (Settings.Instance.IsPaused)
            {
                return;
            }
        }

        if (_player == null)
        {
            return;
        }

        // 2. State Determination
        if (triggerCollider != null)
        {
            _isPlayerInside = triggerCollider.bounds.Contains(_player.transform.position);
        }
        else
        {
            _isPlayerInside = false;
        }

        bool canSlowMo = _isPlayerInside && _player.currentSpeed >= minSpeed && !_isKicked;
        float targetTime = canSlowMo ? slowMoTimeScale : 1f;

        // 3. Kick Logic
        if (canSlowMo)
        {
            if (Input.GetKeyDown(kickKey))
            {
                if (IsLookingAtDoor())
                {
                    KickDoor();
                }
                else
                {
                    ExitTriggerManually();
                }
            }
        }

        // 4. Time & Transitions
        float recov = _isKicked ? 3f : 1f;
        _effectLerp = Mathf.SmoothDamp(_effectLerp, canSlowMo ? 1f : 0f, ref _lerpVel, 1f / (slowMoTransitionSpeed * recov), Mathf.Infinity, Time.unscaledDeltaTime);
        
        if (Mathf.Abs(Time.timeScale - targetTime) > 0.01f || _effectLerp > 0.01f)
        {
            Time.timeScale = Mathf.SmoothDamp(Time.timeScale, targetTime, ref _timeVel, 1f / (slowMoTransitionSpeed * recov), Mathf.Infinity, Time.unscaledDeltaTime);
            Time.fixedDeltaTime = _origFixedDelta * Time.timeScale;
            UpdateGlobalAudio(Time.timeScale);
            UpdateVisuals();
        }
        else
        {
            if (_isKicked)
            {
                ResetEffects();
                enabled = false;
            }
        }

        if (canSlowMo)
        {
            PreserveSpeed();
        }
    }

    private void UpdateVisuals()
    {
        if (_cam != null)
        {
            if (_speedFov != null)
            {
                _speedFov.enabled = _effectLerp < 0.1f;
            }

            float breath = Mathf.Sin(Time.unscaledTime * 3.5f) * 1.5f * _effectLerp;
            _cam.fieldOfView = Mathf.Lerp(_origFov, _origFov + slowMoFovChange + breath, _effectLerp);

            if (_shake != null)
            {
                if (_effectLerp > 0.1f)
                {
                    _shake.Shake(0.012f, 0.01f * _effectLerp);
                }
            }
        }

        if (_chromatic != null)
        {
            _chromatic.intensity.value = _effectLerp * maxChromatic;
        }

        if (_lens != null)
        {
            _lens.intensity.value = _effectLerp * maxLensDistortion;
        }

        if (_vignette != null)
        {
            _vignette.intensity.value = _effectLerp * maxVignette;
        }

        if (_bloom != null)
        {
            _bloom.intensity.value = _effectLerp * maxBloom;
        }
    }

    private void KickDoor()
    {
        _isKicked = true;

        if (_doorRb == null)
        {
            _doorRb = doorObject.AddComponent<Rigidbody>();
        }
        
        doorObject.transform.SetParent(null);
        _doorRb.isKinematic = false;
        _doorRb.useGravity = true;
        _doorRb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        if (doorObject.TryGetComponent(out Collider col))
        {
            StartCoroutine(TemporaryPhase(col, 0.1f));
        }

        Vector3 dir = (doorObject.transform.position - _player.transform.position).normalized;
        float dotF = Vector3.Dot(dir, doorObject.transform.forward);
        float dotR = Vector3.Dot(dir, doorObject.transform.right);

        if (Mathf.Abs(dotF) > Mathf.Abs(dotR))
        {
            dir = Vector3.Project(dir, doorObject.transform.forward).normalized;
        }
        else
        {
            dir = Vector3.Project(dir, doorObject.transform.right).normalized;
        }
        
        dir.y = 0;

        _doorRb.AddForce(dir * kickForce + Vector3.up * kickUpForce, ForceMode.Impulse);
        _doorRb.AddTorque(Random.insideUnitSphere * 20f, ForceMode.Impulse);

        StartCoroutine(HitStop(0.07f));

        if (_shake != null)
        {
            _shake.Shake(0.3f, 0.6f);
        }

        if (triggerCollider != null)
        {
            triggerCollider.enabled = false;
        }
    }

    private bool IsLookingAtDoor()
    {
        if (Physics.Raycast(_cam.transform.position, _cam.transform.forward, out RaycastHit h, interactionDistance))
        {
            bool isDirect = h.collider.gameObject == doorObject;
            bool isChild = h.transform.IsChildOf(doorObject.transform);

            if (isDirect || isChild)
            {
                return true;
            }
        }
        return false;
    }

    private void PreserveSpeed()
    {
        if (_player.currentSpeed < _entrySpeed * 0.95f)
        {
            _player.horizontalVelocity = _player.horizontalVelocity.normalized * _entrySpeed;
        }

        if (_player.isSliding)
        {
            _player.currentSlideSpeed = Mathf.Max(_player.currentSlideSpeed, _entrySpeed);
        }
    }

    private void UpdateGlobalAudio(float p)
    {
        _audioTimer += Time.unscaledDeltaTime;

        if (_cachedAudio == null || _audioTimer > 0.5f)
        {
            _cachedAudio = FindObjectsOfType<AudioSource>();
            _audioTimer = 0f;
        }

        foreach (var s in _cachedAudio)
        {
            if (s != null)
            {
                if (s.isPlaying)
                {
                    s.pitch = p;
                }
            }
        }
    }

    private void ResetEffects()
    {
        Time.timeScale = 1f;
        Time.fixedDeltaTime = _origFixedDelta;

        if (_cam != null)
        {
            _cam.fieldOfView = _origFov;
        }

        if (_chromatic != null)
        {
            _chromatic.intensity.value = 0;
        }

        if (_lens != null)
        {
            _lens.intensity.value = 0;
        }

        if (_vignette != null)
        {
            _vignette.intensity.value = 0;
        }

        if (_bloom != null)
        {
            _bloom.intensity.value = 0;
        }

        if (_speedFov != null)
        {
            _speedFov.enabled = true;
        }

        AudioSource[] sources = FindObjectsOfType<AudioSource>();
        foreach (var s in sources)
        {
            if (s != null)
            {
                s.pitch = 1f;
            }
        }
    }

    private void ExitTriggerManually()
    {
        _isKicked = true;

        if (triggerCollider != null)
        {
            triggerCollider.enabled = false;
        }
    }

    private IEnumerator HitStop(float d)
    {
        float s = Time.timeScale;
        Time.timeScale = 0;
        yield return new WaitForSecondsRealtime(d);
        Time.timeScale = s;
    }

    private IEnumerator TemporaryPhase(Collider c, float d)
    {
        c.isTrigger = true;
        yield return new WaitForSeconds(d);
        if (c != null)
        {
            c.isTrigger = false;
        }
    }

    private void OnTriggerEnter(Collider o)
    {
        if (o.CompareTag("Player"))
        {
            _isPlayerInside = true;
            _entrySpeed = _player.currentSpeed;
        }
    }

    private void OnDisable()
    {
        ResetEffects();
    }
}
