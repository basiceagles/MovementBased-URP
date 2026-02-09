using UnityEngine;

public class LeverRush : MonoBehaviour
{
    [Header("Settings")]
    public Animator leverAnimator;
    public string triggerName = "ActivateLever";
    public float interactionRange = 10f; 
    public bool collected = false;

    [Header("UI References")]
    [SerializeField] private GameObject uiToolTip;
    [SerializeField] private GameObject uiKeybind;
    private CrosshairSwitcher crosshairSwitcher;

    [Header("Reward Settings")]
    [SerializeField] private GameObject medkitPrefab;
    [SerializeField] private GameObject money100Prefab;
    [SerializeField] private GameObject money200Prefab;
    [SerializeField] private GameObject money300Prefab;
    [SerializeField] private Transform spawnPoint;

    private Camera playerCamera;
    private bool isLookingAtLever = false;

    private void Start()
    {
        playerCamera = Camera.main;
        
        GameObject uiObj = GameObject.Find("CrossHairUI");
        if (uiObj != null)
        {
            crosshairSwitcher = uiObj.GetComponent<CrosshairSwitcher>();
        }
    }

    private void Update()
    {
        if (collected) return;

        if (playerCamera == null)
        {
            playerCamera = Camera.main;
            return;
        }

        float distance = Vector3.Distance(transform.position, playerCamera.transform.position);

        bool inRange = distance <= interactionRange;
        if (uiToolTip != null && uiToolTip.activeSelf != inRange)
        {
            uiToolTip.SetActive(inRange);
        }

        if (inRange)
        {
            HandleRaycast();
        }
        else if (isLookingAtLever)
        {
            ToggleInteractionUI(false);
        }
    }

    private void HandleRaycast()
    {
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactionRange))
        {
            if (hit.collider.gameObject == gameObject)
            {
                if (!isLookingAtLever)
                {
                    ToggleInteractionUI(true);
                }
                
                if (Input.GetKeyDown(KeyCode.E))
                {
                    Collect();
                }
                return;
            }
        }

        if (isLookingAtLever)
        {
            ToggleInteractionUI(false);
        }
    }

    private void ToggleInteractionUI(bool state)
    {
        isLookingAtLever = state;
        if (uiKeybind != null) uiKeybind.SetActive(state);
        
        if (crosshairSwitcher != null)
        {
            if (state)
            {
                crosshairSwitcher.ShowSwitchCrosshair();
            }
            else
            {
                crosshairSwitcher.HideSwitchCrosshair();
            }
        }
    }

    public void Collect()
    {
        if (collected)
        {
            return;
        }
        
        collected = true;

        if (leverAnimator != null)
        {
            leverAnimator.SetTrigger(triggerName);
        }

        if (uiToolTip != null)
        {
            uiToolTip.SetActive(false);
        }

        ToggleInteractionUI(false);

        SpawnReward();
    }

    private void SpawnReward()
    {
        if (spawnPoint == null) return;

        float mainRoll = Random.Range(0f, 100f);

        if (mainRoll <= 20f)
        {
            if (medkitPrefab != null)
            {
                Instantiate(medkitPrefab, spawnPoint.position, spawnPoint.rotation);
            }
        }
        else
        {
            float moneyRoll = Random.Range(0f, 100f);
            GameObject prefabToSpawn = null;

            if (moneyRoll <= 60f)
            {
                prefabToSpawn = money100Prefab;
            }
            else if (moneyRoll <= 90f)
            {
                prefabToSpawn = money200Prefab;
            }
            else
            {
                prefabToSpawn = money300Prefab;
            }

            if (prefabToSpawn != null)
            {
                Instantiate(prefabToSpawn, spawnPoint.position, spawnPoint.rotation);
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Rush"))
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Rush"))
        {
            Destroy(gameObject);
        }
    }
}