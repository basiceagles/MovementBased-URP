using UnityEngine;
using TMPro;

public class DeathScreenUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI levelsCompletedText;
    [SerializeField] private TextMeshProUGUI playTimeText;
    
    [Header("Money UI")]
    [SerializeField] private TextMeshProUGUI moneyEarnedText;
    [SerializeField] private TextMeshProUGUI moneyTotalText;
    [SerializeField] private TextMeshProUGUI multiplierText;

    private void Start()
    {
        DisplayStats();
    }

    private void DisplayStats()
    {
        if (levelsCompletedText != null)
        {
            levelsCompletedText.text = RunStats.LevelsCompleted.ToString();
        }
        if (playTimeText != null)
        {
            playTimeText.text = FormatTime(RunStats.TotalPlayTime);
        }
        if (moneyEarnedText != null)
        {
            moneyEarnedText.text = "+" + RunStats.MoneyEarned.ToString() + " SC";
        }
        if (moneyTotalText != null)
        {
            moneyTotalText.text = RunStats.MoneyTotal.ToString();
        }
        if (multiplierText != null)
        {
            int percent = Mathf.RoundToInt(RunStats.MoneyMultiplier * 100f);
            multiplierText.text = percent.ToString() + "%";
        }
    }
    
    private string FormatTime(float timeInSeconds)
    {
        int hours = Mathf.FloorToInt(timeInSeconds / 3600f);
        int minutes = Mathf.FloorToInt((timeInSeconds % 3600f) / 60f);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60f);
        return string.Format("{0:00}:{1:00}:{2:00}", hours, minutes, seconds);
    }
}
