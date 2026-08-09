using UnityEngine;
using System;
using System.Collections;
using TMPro;
using UnityEngine.SceneManagement;

public class Timer : MonoBehaviour
{
    [Header("Countdown Settings")]
    public int totalSeconds = 60;
    [SerializeField] private int thresholdSeconds = 10;
    [SerializeField] private string deadSceneName = "TimerDead";
    
    [Header("TextMeshPro Display")]
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color thresholdColor = Color.red;
    [SerializeField] private bool autoUpdateText = true;
    
    private float totalTimeInSeconds;
    public float currentTime;
    private Coroutine countdownCoroutine;
    private bool isThresholdCalled = false;
    private bool isBelowThresholdCalled = false;

    private GameObject targetMusicManager;
    private AudioSource musicManagerAS;
    
    void Awake()
    {
        CalculateTotalTime();

        targetMusicManager = GameObject.Find("MusicManager");
        if (targetMusicManager != null)
        {
            musicManagerAS = targetMusicManager.GetComponent<AudioSource>();
        }
        
        timerText.text = "SAFE";
    }

    public void StartCountdown()
    {
        StopCountdown();
        
        isThresholdCalled = false;
        isBelowThresholdCalled = false;
        CalculateTotalTime();
        currentTime = totalTimeInSeconds;
        
        countdownCoroutine = StartCoroutine(CountdownCoroutine());
        
        UpdateCountdownDisplay();
    }

    public void StartCountdown(int customSeconds)
    {
        totalSeconds = customSeconds;
        StartCountdown();
    }

    private IEnumerator CountdownCoroutine()
    {
        while (currentTime > 0)
        {
            CheckThresholdConditions();
            
            currentTime -= Time.deltaTime;
            currentTime = Mathf.Max(0, currentTime); 
            
            UpdateCountdownDisplay();
            
            yield return null;
        }
        
        OnCountdownComplete();
        countdownCoroutine = null;
    }

    private void CheckThresholdConditions()
    {
        if (currentTime <= thresholdSeconds && currentTime > 0)
        {
            OnCountdownUnderThreshold();
        }
        else if (currentTime > thresholdSeconds)
        {
            OnCountdownAboveThreshold();
        }
        
        if (currentTime <= thresholdSeconds && !isBelowThresholdCalled)
        {
            OnCountdownBelowThreshold();
            isBelowThresholdCalled = true;
        }
    }

    private void UpdateCountdownDisplay()
    {
        string formattedTime = FormatTime(currentTime);
        
        if (autoUpdateText)
        {
            UpdateTimerText(formattedTime);
        }
    }
    
    private void OnCountdownUnderThreshold()
    {
        if (timerText != null)
        {
            timerText.color = thresholdColor;
        }
        
        if (musicManagerAS != null)
        {
            musicManagerAS.pitch = 1.1f;
        }
    }

    private void OnCountdownAboveThreshold()
    {
        if (timerText != null)
        {
            timerText.color = normalColor;
        }
        
        if (musicManagerAS != null)
        {
            musicManagerAS.pitch = 1.0f;
        }
    }

    private void OnCountdownBelowThreshold()
    {
    }

    private void OnCountdownComplete()
    {
        if (musicManagerAS != null)
        {
            musicManagerAS.pitch = 1.0f;
        }

        if (SceneManager.GetActiveScene().name == "Intro3")
        {
            SceneManager.LoadScene("Intro4");
        }
        else
        {
            SceneManager.LoadScene(deadSceneName);
        }
    }

    private void UpdateTimerText(string timeString)
    {
        if (timerText != null)
        {
            timerText.text = timeString;
        }
    }

    private string FormatTime(float timeInSeconds)
    {
        int minutes = Mathf.FloorToInt(timeInSeconds / 60f);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60f);
        int milliseconds = Mathf.FloorToInt((timeInSeconds * 100f) % 100f);
        
        return $"{minutes:00}:{seconds:00}:{milliseconds:00}";
    }

    public void StopAndReset()
    {
        if (countdownCoroutine != null)
        {
            StopCoroutine(countdownCoroutine);
            countdownCoroutine = null;
        }
        
        CalculateTotalTime();
        currentTime = totalTimeInSeconds;
        isThresholdCalled = false;
        isBelowThresholdCalled = false;
        
        if (timerText != null)
        {
            timerText.color = normalColor;
        }

        if (musicManagerAS != null)
        {
            musicManagerAS.pitch = 1.0f;
        }

        timerText.text = "SAFE";
    }

    public void StopCountdown()
    {
        if (countdownCoroutine != null)
        {
            StopCoroutine(countdownCoroutine);
            countdownCoroutine = null;
        }
    }

    private void CalculateTotalTime()
    {
        totalTimeInSeconds = totalSeconds;
    }
}