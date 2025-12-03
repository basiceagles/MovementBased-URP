using UnityEngine;
using UnityEngine.Events;
using TMPro;
using static LevelGenerator;
public class MoneyManager : MonoBehaviour
{
    private const string TOTAL_MONEY_KEY = "TotalPlayerMoney";
    private const string LEVELS_COMPLETED_KEY = "LevelsCompleted";
    private const string CURRENT_DIFFICULTY_KEY = "CurrentDifficulty";

    public int easyMoneyValue = 100;
    public int mediumMoneyValue = 150;
    public int hardMoneyValue = 200;
    public int closedMoneyValue = 100;
    public int closedMediumMoneyValue = 150;
    public int closedHardMoneyValue = 200;

    [Header("Multiplier Settings")]
    [Range(0.1f, 10f)]
    public float moneyMultiplier = 1.0f;

    [Header("Current State")]
    [SerializeField] private int totalMoney = 0;
    [SerializeField] private int levelsCompleted = 0;
    [SerializeField] private DifficultyType currentDifficulty = DifficultyType.Easy;

    [Header("Events")]
    public UnityEvent<int> OnTotalMoneyChanged;
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI totalMoneyText;
    [SerializeField] private Color notEnoughMoneyColor = Color.red;
    [SerializeField] private float notEnoughMoneyDuration = 0.2f;
    [SerializeField] private float notEnoughMoneyShakeAmount = 10f;
    public static MoneyManager Instance { get; private set; }
    public int TotalMoney => totalMoney;
    public int LevelsCompleted => levelsCompleted;
    public DifficultyType CurrentDifficulty => currentDifficulty;

    private Vector3 originalMoneyTextPosition;
    private Color originalMoneyTextColor;
    private bool moneyTextInitialized;
    private Coroutine notEnoughMoneyRoutine;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void OnEnable()
    {
        LevelGenerator.OnLevelCompleted += OnLevelCompleted;
    }

    private void OnDisable()
    {
        LevelGenerator.OnLevelCompleted -= OnLevelCompleted;
    }

    private void Start()
    {
        LoadTotalMoney();
        CalculateAndAddMoneyFromPreviousRun();
    }
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Delete))
        {
            ResetAllMoney();
        }
    }
    
    private void ResetAllMoney()
    {
        PlayerPrefs.DeleteKey(TOTAL_MONEY_KEY);
        PlayerPrefs.DeleteKey(LEVELS_COMPLETED_KEY);
        PlayerPrefs.DeleteKey(CURRENT_DIFFICULTY_KEY);
        PlayerPrefs.Save();
        
        totalMoney = 0;
        levelsCompleted = 0;
        currentDifficulty = DifficultyType.Easy;
        
        OnTotalMoneyChanged?.Invoke(totalMoney);
        UpdateMoneyText();
    }

    private void OnLevelCompleted(DifficultyType difficultyType)
    {
        currentDifficulty = difficultyType;
        levelsCompleted++;
    }

    public void OnPlayerDeath()
    {
        PlayerPrefs.SetInt(LEVELS_COMPLETED_KEY, levelsCompleted);
        PlayerPrefs.SetInt(CURRENT_DIFFICULTY_KEY, (int)currentDifficulty);
        PlayerPrefs.Save();
    }
    public static void SaveMoneyOnDeath()
    {
        if (Instance != null)
        {
            Instance.OnPlayerDeath();
        }
    }

    private void LoadTotalMoney()
    {
        totalMoney = PlayerPrefs.GetInt(TOTAL_MONEY_KEY, 0);
        OnTotalMoneyChanged?.Invoke(totalMoney);
        UpdateMoneyText();
    }
    
    private void CalculateAndAddMoneyFromPreviousRun()
    {
        int prevLevels = PlayerPrefs.GetInt(LEVELS_COMPLETED_KEY, 0);
        int prevDifficultyInt = PlayerPrefs.GetInt(CURRENT_DIFFICULTY_KEY, 0);
        if (prevLevels > 0)
        {
            DifficultyType prevDifficulty = (DifficultyType)prevDifficultyInt;
            int moneyPerLevel = GetMoneyValueForDifficulty(prevDifficulty);
            int earnedMoney = Mathf.RoundToInt(moneyPerLevel * prevLevels * moneyMultiplier);
            totalMoney += earnedMoney;
            PlayerPrefs.SetInt(TOTAL_MONEY_KEY, totalMoney);

            PlayerPrefs.DeleteKey(LEVELS_COMPLETED_KEY);
            PlayerPrefs.DeleteKey(CURRENT_DIFFICULTY_KEY);
            PlayerPrefs.Save();
            
            OnTotalMoneyChanged?.Invoke(totalMoney);
            UpdateMoneyText();
        }
        levelsCompleted = 0;
        currentDifficulty = DifficultyType.Easy;
    }
    
    private int GetMoneyValueForDifficulty(DifficultyType difficulty)
    {
        switch (difficulty)
        {
            case DifficultyType.Easy: return easyMoneyValue;
            case DifficultyType.Medium: return mediumMoneyValue;
            case DifficultyType.Hard: return hardMoneyValue;
            case DifficultyType.Closed: return closedMoneyValue;
            case DifficultyType.ClosedMedium: return closedMediumMoneyValue;
            case DifficultyType.ClosedHard: return closedHardMoneyValue;
            default: return easyMoneyValue;
        }
    }

    public bool SpendMoney(int amount)
    {
        if (totalMoney >= amount)
        {
            totalMoney -= amount;
            PlayerPrefs.SetInt(TOTAL_MONEY_KEY, totalMoney);
            PlayerPrefs.Save();
            OnTotalMoneyChanged?.Invoke(totalMoney);
            UpdateMoneyText();
            return true;
        }
        return false;
    }

    private void UpdateMoneyText()
    {
        if (totalMoneyText == null) return;
        InitializeMoneyTextState();
        totalMoneyText.text = "Stable Coins: " + FormatMoney(totalMoney);
    }

    private void InitializeMoneyTextState()
    {
        if (moneyTextInitialized || totalMoneyText == null) return;

        originalMoneyTextPosition = totalMoneyText.rectTransform.anchoredPosition;
        originalMoneyTextColor = totalMoneyText.color;
        moneyTextInitialized = true;
    }

    private string FormatMoney(int value)
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

    public void PlayNotEnoughMoneyFeedback()
    {
        if (totalMoneyText == null) return;

        InitializeMoneyTextState();

        if (notEnoughMoneyRoutine != null)
        {
            StopCoroutine(notEnoughMoneyRoutine);
        }

        notEnoughMoneyRoutine = StartCoroutine(NotEnoughMoneyCoroutine());
    }

    private System.Collections.IEnumerator NotEnoughMoneyCoroutine()
    {
        float elapsed = 0f;
        RectTransform rect = totalMoneyText.rectTransform;

        // Set color to warning color
        totalMoneyText.color = notEnoughMoneyColor;

        while (elapsed < notEnoughMoneyDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / notEnoughMoneyDuration;

            // Simple left-right shake using sine
            float offset = Mathf.Sin(t * Mathf.PI * 10f) * notEnoughMoneyShakeAmount;
            rect.anchoredPosition = originalMoneyTextPosition + new Vector3(offset, 0f, 0f);

            yield return null;
        }

        // Restore original state
        rect.anchoredPosition = originalMoneyTextPosition;
        totalMoneyText.color = originalMoneyTextColor;
        notEnoughMoneyRoutine = null;
    }
}
