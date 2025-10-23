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
    [SerializeField] private BoxCollider boxCollider;

    void Start()
    {
        // If no animator is assigned, try to get it from this GameObject
        if (leverAnimator == null)
        {
            leverAnimator = GetComponent<Animator>();
        }

    }

    // Public method to be called from other scripts
    public void Collect()
    {
        if (collected == true) return;
        leverAnimator.SetTrigger(triggerName);
        collected = !collected;

        HideUITip();
        HideUIKeybind();
    }
    public void HideUITip()
    {
        uiToolTip.SetActive(false);
    }
    public void ShowUITip()
    {
        uiToolTip.SetActive(true);
    }
    public void HideUIKeybind()
    {
        uiKeybind.SetActive(false);
    }
    public void ShowUIKeybind()
    {
        uiKeybind.SetActive(true);
    }
}
