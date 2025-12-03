using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using SmallHedge.SoundManager;

public class MetaPerkSystem : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private MoneyManager moneyManager;
    [SerializeField] private Grappling grappling;
    [SerializeField] private Swinging swinging;

    private enum PerkStatType
    {
        None,
        RunSpeed,
        JumpHeight,
        SlideSpeed,
        SlideAcceleration,
        MaxSlideSpeed,
        EnableGrapple,
        EnableSwing
    }

    [Header("Meta Perk Buttons")]
    [SerializeField] private Button[] metaPerkButtons;
    [SerializeField] private int[] metaPerkCosts;
    [SerializeField] private TextMeshProUGUI[] metaPerkCostTexts;
    [SerializeField] private PerkStatType[] metaPerkStatTypes;
    [SerializeField] private float[] metaPerkStatValues;

    [Header("Meta Perk Increments")]
    [SerializeField] private float runSpeedIncrement = 1f;
    [SerializeField] private float jumpHeightIncrement = 0.5f;
    [SerializeField] private float slideSpeedIncrement = 1f;
    [SerializeField] private float slideAccelerationIncrement = 1f;
    [SerializeField] private float maxSlideSpeedIncrement = 1f;

    private const string RunSpeedKey = "Meta_RunSpeed";
    private const string JumpHeightKey = "Meta_JumpHeight";
    private const string SlideSpeedKey = "Meta_SlideSpeed";
    private const string SlideAccelerationKey = "Meta_SlideAcceleration";
    private const string MaxSlideSpeedKey = "Meta_MaxSlideSpeed";
    private const string PerkBoughtKeyPrefix = "MetaPerk_Bought_";
    private const string GrappleEnabledKey = "Meta_GrappleEnabled";
    private const string SwingEnabledKey = "Meta_SwingEnabled";

    private void Awake()
    {
        if (playerController == null)
        {
            playerController = FindObjectOfType<PlayerController>();
        }

        if (grappling == null)
        {
            grappling = FindObjectOfType<Grappling>();
        }

        if (swinging == null)
        {
            swinging = FindObjectOfType<Swinging>();
        }
    }

    private void Start()
    {
        if (playerController == null)
        {
            Debug.LogWarning("MetaPerkSystem: PlayerController reference is missing.");
            return;
        }

        LoadMetaPerks();
        LoadPerkButtonsState();
        InitializePerkCostTexts();
    }

    private void LoadMetaPerks()
    {
        float run = playerController.runSpeed;
        float jump = playerController.jumpHeight;
        float slide = playerController.slideSpeed;
        float slideAcc = playerController.slideAcceleration;
        float maxSlide = playerController.maxSlideSpeed;

        if (PlayerPrefs.HasKey(RunSpeedKey))
        {
            run = PlayerPrefs.GetFloat(RunSpeedKey);
        }
        else
        {
            PlayerPrefs.SetFloat(RunSpeedKey, run);
        }

        if (PlayerPrefs.HasKey(JumpHeightKey))
        {
            jump = PlayerPrefs.GetFloat(JumpHeightKey);
        }
        else
        {
            PlayerPrefs.SetFloat(JumpHeightKey, jump);
        }

        if (PlayerPrefs.HasKey(SlideSpeedKey))
        {
            slide = PlayerPrefs.GetFloat(SlideSpeedKey);
        }
        else
        {
            PlayerPrefs.SetFloat(SlideSpeedKey, slide);
        }

        if (PlayerPrefs.HasKey(SlideAccelerationKey))
        {
            slideAcc = PlayerPrefs.GetFloat(SlideAccelerationKey);
        }
        else
        {
            PlayerPrefs.SetFloat(SlideAccelerationKey, slideAcc);
        }

        if (PlayerPrefs.HasKey(MaxSlideSpeedKey))
        {
            maxSlide = PlayerPrefs.GetFloat(MaxSlideSpeedKey);
        }
        else
        {
            PlayerPrefs.SetFloat(MaxSlideSpeedKey, maxSlide);
        }

        playerController.runSpeed = run;
        playerController.jumpHeight = jump;
        playerController.slideSpeed = slide;
        playerController.slideAcceleration = slideAcc;
        playerController.maxSlideSpeed = maxSlide;

        PlayerPrefs.Save();

        LoadAbilityPerks();
    }

    private void LoadAbilityPerks()
    {
        if (grappling != null && PlayerPrefs.HasKey(GrappleEnabledKey))
        {
            bool enabled = PlayerPrefs.GetInt(GrappleEnabledKey, 0) == 1;
            grappling.SetGrapplingEnabled(enabled);
        }

        if (swinging != null && PlayerPrefs.HasKey(SwingEnabledKey))
        {
            bool enabled = PlayerPrefs.GetInt(SwingEnabledKey, 0) == 1;
            swinging.SetSwingingEnabled(enabled);
        }
    }

    private void InitializePerkCostTexts()
    {
        if (metaPerkButtons == null || metaPerkCostTexts == null) return;

        int length = Mathf.Min(metaPerkButtons.Length, metaPerkCostTexts.Length);

        for (int i = 0; i < length; i++)
        {
            TextMeshProUGUI label = metaPerkCostTexts[i];
            if (label == null) continue;

            int cost = (metaPerkCosts != null && i < metaPerkCosts.Length) ? metaPerkCosts[i] : 0;
            label.text = FormatCost(cost);
        }
    }

    private string FormatCost(int value)
    {
        if (value >= 1000000000)
        {
            return (value / 1000000000) + "B";
        }
        if (value >= 1000000)
        {
            return (value / 1000000) + "M";
        }
        if (value >= 1000)
        {
            return (value / 1000) + "K";
        }

        return value.ToString();
    }

    private void LoadPerkButtonsState()
    {
        if (metaPerkButtons == null) return;

        for (int i = 0; i < metaPerkButtons.Length; i++)
        {
            Button button = metaPerkButtons[i];
            if (button == null) continue;

            string key = PerkBoughtKeyPrefix + button.name;
            bool isBought = PlayerPrefs.GetInt(key, 0) == 1;
            if (isBought)
            {
                button.interactable = false;
            }
        }
    }

    private void SaveMetaPerks()
    {
        if (playerController == null) return;

        PlayerPrefs.SetFloat(RunSpeedKey, playerController.runSpeed);
        PlayerPrefs.SetFloat(JumpHeightKey, playerController.jumpHeight);
        PlayerPrefs.SetFloat(SlideSpeedKey, playerController.slideSpeed);
        PlayerPrefs.SetFloat(SlideAccelerationKey, playerController.slideAcceleration);
        PlayerPrefs.SetFloat(MaxSlideSpeedKey, playerController.maxSlideSpeed);
        PlayerPrefs.Save();
    }

    public void UpgradeRunSpeed()
    {
        if (playerController == null) return;

        float newValue = playerController.runSpeed + runSpeedIncrement;
        playerController.runSpeed = newValue;
        SaveMetaPerks();
    }

    public void UpgradeJumpHeight()
    {
        if (playerController == null) return;

        float newValue = playerController.jumpHeight + jumpHeightIncrement;
        playerController.jumpHeight = newValue;
        SaveMetaPerks();
    }

    public void UpgradeSlideSpeed()
    {
        if (playerController == null) return;

        float newValue = playerController.slideSpeed + slideSpeedIncrement;
        playerController.slideSpeed = newValue;
        SaveMetaPerks();
    }

    public void UpgradeSlideAcceleration()
    {
        if (playerController == null) return;

        float newValue = playerController.slideAcceleration + slideAccelerationIncrement;
        playerController.slideAcceleration = newValue;
        SaveMetaPerks();
    }

    public void UpgradeMaxSlideSpeed()
    {
        if (playerController == null) return;

        float newValue = playerController.maxSlideSpeed + maxSlideSpeedIncrement;
        playerController.maxSlideSpeed = newValue;
        SaveMetaPerks();
    }

    public void SetRunSpeed(float value)
    {
        if (playerController == null) return;

        playerController.runSpeed = value;
        SaveMetaPerks();
    }

    public void SetJumpHeight(float value)
    {
        if (playerController == null) return;

        playerController.jumpHeight = value;
        SaveMetaPerks();
    }

    public void SetSlideSpeed(float value)
    {
        if (playerController == null) return;

        playerController.slideSpeed = value;
        SaveMetaPerks();
    }

    public void SetSlideAcceleration(float value)
    {
        if (playerController == null) return;

        playerController.slideAcceleration = value;
        SaveMetaPerks();
    }

    public void SetMaxSlideSpeed(float value)
    {
        if (playerController == null) return;

        playerController.maxSlideSpeed = value;
        SaveMetaPerks();
    }

    public void MarkCurrentButtonAsBought()
    {
        GameObject current = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
        if (current == null)
        {
            Debug.LogWarning("MetaPerkSystem.MarkCurrentButtonAsBought: currentSelectedGameObject is null.");
            return;
        }

        Button button = current.GetComponent<Button>();
        if (button == null)
        {
            Debug.LogWarning("MetaPerkSystem.MarkCurrentButtonAsBought: currentSelectedGameObject has no Button component.");
            return;
        }

        MarkButtonAsBought(button);
    }

    public void MarkButtonAsBought(Button button)
    {
        if (button == null) return;

        if (metaPerkButtons == null || metaPerkButtons.Length == 0)
        {
            Debug.LogWarning("MetaPerkSystem.MarkButtonAsBought: metaPerkButtons array is not configured.");
            SoundManager.PlaySound(SoundType.UIDENIED);
            if (moneyManager != null)
            {
                moneyManager.PlayNotEnoughMoneyFeedback();
            }
            else if (MoneyManager.Instance != null)
            {
                MoneyManager.Instance.PlayNotEnoughMoneyFeedback();
            }
            return;
        }

        int buttonIndex = System.Array.IndexOf(metaPerkButtons, button);
        if (buttonIndex < 0)
        {
            Debug.LogWarning("MetaPerkSystem.MarkButtonAsBought: clicked button '" + button.name + "' was not found in metaPerkButtons array.");
            SoundManager.PlaySound(SoundType.UIDENIED);
            if (moneyManager != null)
            {
                moneyManager.PlayNotEnoughMoneyFeedback();
            }
            else if (MoneyManager.Instance != null)
            {
                MoneyManager.Instance.PlayNotEnoughMoneyFeedback();
            }
            return;
        }

        // Resolve MoneyManager reference if needed
        if (moneyManager == null && MoneyManager.Instance != null)
        {
            moneyManager = MoneyManager.Instance;
        }

        // Determine cost for this button
        int cost = 0;
        if (metaPerkButtons != null && metaPerkCosts != null)
        {
            if (buttonIndex >= 0 && buttonIndex < metaPerkCosts.Length)
            {
                cost = metaPerkCosts[buttonIndex];
            }
        }

        // If there is a cost, ensure we have enough money
        if (cost > 0)
        {
            if (moneyManager == null)
            {
                Debug.LogWarning("MetaPerkSystem.MarkButtonAsBought: MoneyManager reference is missing, cannot spend money.");
                SoundManager.PlaySound(SoundType.UIDENIED);
                if (MoneyManager.Instance != null)
                {
                    MoneyManager.Instance.PlayNotEnoughMoneyFeedback();
                }
                return;
            }

            bool spent = moneyManager.SpendMoney(cost);
            if (!spent)
            {
                Debug.Log("MetaPerkSystem: Not enough money to buy perk '" + button.name + "'. Cost: " + cost);
                SoundManager.PlaySound(SoundType.UIDENIED);
                if (moneyManager != null)
                {
                    moneyManager.PlayNotEnoughMoneyFeedback();
                }
                else if (MoneyManager.Instance != null)
                {
                    MoneyManager.Instance.PlayNotEnoughMoneyFeedback();
                }
                moneyManager.PlayNotEnoughMoneyFeedback();
                return;
            }
        }

        // Apply configured stat change for this button
        ApplyPerkStatForButton(button);

        SoundManager.PlaySound(SoundType.PERKSELECTCLICK);
        button.interactable = false;

        string key = PerkBoughtKeyPrefix + button.name;
        PlayerPrefs.SetInt(key, 1);
        PlayerPrefs.Save();
    }

    private void ApplyPerkStatForButton(Button button)
    {
        if (playerController == null || metaPerkButtons == null || metaPerkStatTypes == null)
        {
            return;
        }

        int index = System.Array.IndexOf(metaPerkButtons, button);
        if (index < 0 || index >= metaPerkStatTypes.Length)
        {
            return;
        }

        PerkStatType type = metaPerkStatTypes[index];
        float value = (metaPerkStatValues != null && index < metaPerkStatValues.Length) ? metaPerkStatValues[index] : 0f;

        // For numeric stats, skip if value is zero. For toggle perks, value is ignored.
        if (type == PerkStatType.RunSpeed ||
            type == PerkStatType.JumpHeight ||
            type == PerkStatType.SlideSpeed ||
            type == PerkStatType.SlideAcceleration ||
            type == PerkStatType.MaxSlideSpeed)
        {
            if (Mathf.Approximately(value, 0f))
            {
                return;
            }
        }

        switch (type)
        {
            case PerkStatType.RunSpeed:
                playerController.runSpeed = value;
                break;
            case PerkStatType.JumpHeight:
                playerController.jumpHeight = value;
                break;
            case PerkStatType.SlideSpeed:
                playerController.slideSpeed = value;
                break;
            case PerkStatType.SlideAcceleration:
                playerController.slideAcceleration = value;
                break;
            case PerkStatType.MaxSlideSpeed:
                playerController.maxSlideSpeed = value;
                break;
            case PerkStatType.EnableGrapple:
                if (grappling != null)
                {
                    grappling.SetGrapplingEnabled(true);
                    PlayerPrefs.SetInt(GrappleEnabledKey, 1);
                    PlayerPrefs.Save();
                }
                return;
            case PerkStatType.EnableSwing:
                if (swinging != null)
                {
                    swinging.SetSwingingEnabled(true);
                    PlayerPrefs.SetInt(SwingEnabledKey, 1);
                    PlayerPrefs.Save();
                }
                return;
            case PerkStatType.None:
            default:
                return;
        }

        SaveMetaPerks();
    }
}
