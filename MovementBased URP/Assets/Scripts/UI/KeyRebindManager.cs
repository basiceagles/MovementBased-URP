using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class KeyRebindManager : MonoBehaviour
{
    [System.Serializable]
    public class KeyBinding
    {
        public string keyName;
        public TMP_Text keyText;
        public KeyCode defaultKey;
        [HideInInspector] public KeyCode currentKey;
        [HideInInspector] public Color originalColor;
    }

    [Header("Key Bindings")]
    public KeyBinding crouchSlide = new KeyBinding { keyName = "CrouchSlide", defaultKey = KeyCode.LeftControl };
    public KeyBinding run = new KeyBinding { keyName = "Run", defaultKey = KeyCode.LeftShift };
    public KeyBinding interact = new KeyBinding { keyName = "Interact", defaultKey = KeyCode.E };
    public KeyBinding jump = new KeyBinding { keyName = "Jump", defaultKey = KeyCode.Space };
    public KeyBinding swing = new KeyBinding { keyName = "Swing", defaultKey = KeyCode.Q };
    public KeyBinding rope = new KeyBinding { keyName = "Rope", defaultKey = KeyCode.F };

    [Header("Scripts to Update")]
    public PlayerController playerController;
    public Swinging swingingScript;
    public Grappling grapplingScript;
    public WallRunning wallRunningScript;

    private KeyBinding currentRebinding;
    private bool isWaitingForKey = false;

    private void Start()
    {
        InitializeBindings();  
        LoadAllBindings();
        UpdateAllKeyTexts();
        ApplyKeysToScripts();
    }

    private void InitializeBindings()
    {
        SetupClickableText(crouchSlide);
        SetupClickableText(run);
        SetupClickableText(interact);
        SetupClickableText(jump);
        SetupClickableText(swing);
        SetupClickableText(rope);
    }

    private void SetupClickableText(KeyBinding binding)
    {
        if (binding.keyText == null) return;

        binding.originalColor = binding.keyText.color;

        Button button = binding.keyText.gameObject.GetComponent<Button>();
        if (button == null)
        {
            button = binding.keyText.gameObject.AddComponent<Button>();
        }

        button.transition = Selectable.Transition.None;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => StartRebinding(binding));
    }

    private void Update()
    {
        if (!isWaitingForKey) return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CancelRebinding();
            return;
        }

        foreach (KeyCode keyCode in System.Enum.GetValues(typeof(KeyCode)))
        {
            if (Input.GetKeyDown(keyCode))
            {
                CompleteRebinding(keyCode);
                return;
            }
        }
    }

    private void StartRebinding(KeyBinding binding)
    {
        if (binding == null) return;

        currentRebinding = binding;
        isWaitingForKey = true;
        
        binding.keyText.text = "...";
        binding.keyText.color = Color.white;
    }

    private void CompleteRebinding(KeyCode newKey)
    {
        if (currentRebinding == null) return;

        currentRebinding.currentKey = newKey;
        
        SaveBinding(currentRebinding);
        UpdateKeyText(currentRebinding);
        currentRebinding.keyText.color = currentRebinding.originalColor;

        ApplyKeysToScripts();

        isWaitingForKey = false;
        currentRebinding = null;
    }

    private void CancelRebinding()
    {
        if (currentRebinding == null) return;

        UpdateKeyText(currentRebinding);
        currentRebinding.keyText.color = currentRebinding.originalColor;

        isWaitingForKey = false;
        currentRebinding = null;
    }

    private void UpdateKeyText(KeyBinding binding)
    {
        if (binding == null || binding.keyText == null) return;

        binding.keyText.text = binding.currentKey.ToString().ToUpper();
    }

    private void UpdateAllKeyTexts()
    {
        UpdateKeyText(crouchSlide);
        UpdateKeyText(run);
        UpdateKeyText(interact);
        UpdateKeyText(jump);
        UpdateKeyText(swing);
        UpdateKeyText(rope);
    }

    private void SaveBinding(KeyBinding binding)
    {
        if (binding == null) return;

        PlayerPrefs.SetInt("Key_" + binding.keyName, (int)binding.currentKey);
        PlayerPrefs.Save();
    }

    private void LoadAllBindings()
    {
        LoadBinding(crouchSlide);
        LoadBinding(run);
        LoadBinding(interact);
        LoadBinding(jump);
        LoadBinding(swing);
        LoadBinding(rope);
    }

    private void LoadBinding(KeyBinding binding)
    {
        if (binding == null) return;

        int savedKey = PlayerPrefs.GetInt("Key_" + binding.keyName, (int)binding.defaultKey);
        binding.currentKey = (KeyCode)savedKey;
    }

    private void ApplyKeysToScripts()
    {
        if (playerController != null)
        {
            playerController.SetKeybinds(crouchSlide.currentKey, run.currentKey, jump.currentKey);
        }

        if (swingingScript != null)
        {
            swingingScript.SetSwingKey(swing.currentKey);
        }

        if (grapplingScript != null)
        {
            grapplingScript.SetGrappleKey(rope.currentKey);
        }
        if(wallRunningScript != null)
        {
            wallRunningScript.SetWallRunningKey(jump.currentKey);
        }
    }

    public void ResetToDefaults()
    {
        ResetBinding(crouchSlide);
        ResetBinding(run);
        ResetBinding(interact);
        ResetBinding(jump);
        ResetBinding(swing);
        ResetBinding(rope);

        UpdateAllKeyTexts();
        ApplyKeysToScripts();
    }

    private void ResetBinding(KeyBinding binding)
    {
        if (binding == null) return;

        binding.currentKey = binding.defaultKey;
        PlayerPrefs.DeleteKey("Key_" + binding.keyName);
        PlayerPrefs.Save();
    }
}
