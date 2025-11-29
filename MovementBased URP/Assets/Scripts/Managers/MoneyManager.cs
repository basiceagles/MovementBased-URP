using UnityEngine;
using UnityEngine.Events;
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
    public static MoneyManager Instance { get; private set; }
    public int TotalMoney => totalMoney;
    public int LevelsCompleted => levelsCompleted;
    public DifficultyType CurrentDifficulty => currentDifficulty;

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
            return true;
        }
        return false;
    }
}
