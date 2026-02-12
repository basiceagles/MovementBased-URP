using UnityEngine;

public class Lever : MonoBehaviour
{
    [Header("UI Settings")]
    public string triggerObjectName = "LeverUITrigger";

    [Header("Animator Reference")]
    public Animator leverAnimator;

    [Header("Settings")]
    public string triggerName = "ActivateLever";
    public bool collected = false;

    [Header("Objects Reference")]
    [SerializeField] private GameObject uiToolTip;
    [SerializeField] private GameObject uiKeybind;
    private CrosshairSwitcher crosshairSwitcher;
    private GameObject crosshairSwitcherTarget;

    [Header("Lever Manager Search")]
    public string targetTag = "LeverManager";
    public string componentType = "LeverSearcher";

    private BoxCollider boxCollider;
    private GameObject leverManager;
    private LeverSearcher leverSearcherComponent;

    void Start()
    {
        boxCollider = GetComponent<BoxCollider>();
        Invoke("SearchForLeverManager", 0.1f);

        crosshairSwitcherTarget = GameObject.Find("CrossHairUI");
        crosshairSwitcher = crosshairSwitcherTarget.GetComponent<CrosshairSwitcher>();
    }

    void SearchForLeverManager()
    {
        if (boxCollider == null) return;

        Collider[] collidersInRange = Physics.OverlapBox(
            transform.TransformPoint(boxCollider.center),
            boxCollider.size * 0.5f,
            transform.rotation
        );

        foreach (Collider collider in collidersInRange)
        {
            if (collider.CompareTag(targetTag))
            {
                leverManager = collider.gameObject;
                leverSearcherComponent = leverManager.GetComponent<LeverSearcher>();

                if (leverSearcherComponent != null)
                {
                    break;
                }
            }
        }
    }

    public void Collect()
    {
        if (collected == true) return;

        if (leverAnimator != null)
        {
            leverAnimator.SetTrigger(triggerName);
        }
        collected = true;

        HideUITip();
        HideUIKeybind();

        if (leverSearcherComponent != null)
        {
            leverSearcherComponent.CheckLeverStatus();
        }
        else
        {
            SearchForLeverManager();
            leverSearcherComponent.CheckLeverStatus();
        }
    }

    public void HideUITip()
    {
        if (uiToolTip != null)
        {
            uiToolTip.SetActive(false);
        }
    }

    public void ShowUITip()
    {
        if (uiToolTip != null && !collected)
        {
            uiToolTip.SetActive(true);
        }
    }

    public void HideUIKeybind()
    {
        if (uiKeybind != null)
        {
            uiKeybind.SetActive(false);
        }
        if (crosshairSwitcher != null)
        {
            crosshairSwitcher.HideSwitchCrosshair();
        }
    }

    public void ShowUIKeybind()
    {
        if (uiKeybind != null && !collected)
        {
            uiKeybind.SetActive(true);
        }
    }
}