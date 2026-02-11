using UnityEngine;
using SmallHedge.SoundManager;

public class DoorHit : MonoBehaviour
{
    [Header("Settings")]
    public float minSpeed = 11f;
    public float kickForce = 25f;
    public float kickUpForce = 5f;
    public float interactionDistance = 4f;
    public KeyCode kickKey = KeyCode.E;
    public string playerTag = "Player";

    [Header("References")]
    public Collider triggerCollider;
    public GameObject doorObject;

    private PlayerController _player;
    private bool _isPlayerInside;
    private Rigidbody _doorRb;
    private bool _isKicked;

    private void Start()
    {
        _player = FindObjectOfType<PlayerController>();
        if (doorObject == null) 
        {
            doorObject = gameObject;
        }
        
        _doorRb = doorObject.GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (_player == null || _isKicked) 
        {
            return;
        }

        UpdatePlayerInsideStatus();

        if (_isPlayerInside && _player.currentSpeed >= minSpeed)
        {
            if (Input.GetKeyDown(kickKey))
            {
                if (IsLookingAtDoor())
                {
                    KickDoor();
                }
            }
        }
    }

    private void UpdatePlayerInsideStatus()
    {
        if (triggerCollider == null) 
        {
            return;
        }

        if (triggerCollider.bounds.Contains(_player.transform.position))
        {
            _isPlayerInside = true;
        }
        else
        {
            _isPlayerInside = false;
        }
    }

    private bool IsLookingAtDoor()
    {
        Camera cam = Camera.main;
        if (cam == null) 
        {
            return false;
        }

        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, interactionDistance))
        {
            GameObject hitObj = hit.collider.gameObject;
            return hitObj == doorObject || hitObj.transform.IsChildOf(doorObject.transform);
        }
        return false;
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
        _doorRb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        
        Collider doorCollider = doorObject.GetComponent<Collider>();
        if (doorCollider != null)
        {
            StartCoroutine(PhaseThroughFrame(doorCollider));
        }

        Vector3 kickDir = _player.transform.forward;
        Vector3 finalForce = kickDir * kickForce + Vector3.up * kickUpForce;

        _doorRb.linearVelocity = Vector3.zero;
        _doorRb.AddForce(finalForce, ForceMode.Impulse);
        
        Vector3 randomTorque = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f)) * 10f;
        _doorRb.AddTorque(randomTorque, ForceMode.Impulse);

        // SoundManager.PlaySound(SoundType.BARRELHIT); - захочешь саунд добавить - раскоментишь
        
        CameraShake shaker = FindObjectOfType<CameraShake>();
        if (shaker != null)
        {
            shaker.HeavyShake();
        }

        if (triggerCollider != null)
        {
            triggerCollider.enabled = false;
        }
        this.enabled = false;
    }

    private System.Collections.IEnumerator PhaseThroughFrame(Collider col)
    {
        col.isTrigger = true;
        yield return new WaitForSeconds(0.1f);
        if (col != null)
        {
            col.isTrigger = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            _isPlayerInside = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            _isPlayerInside = false;
        }
    }
}
