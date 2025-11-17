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

    /// <summary>
    /// Starts the countdown timer
    /// </summary>
    public void StartCountdown()
    {
        // Stop any existing countdown
        StopCountdown();
        
        // Reset flags and time
        isThresholdCalled = false;
        isBelowThresholdCalled = false;
        CalculateTotalTime();
        currentTime = totalTimeInSeconds;
        
        // Start countdown coroutine
        countdownCoroutine = StartCoroutine(CountdownCoroutine());
        
        // Trigger initial update
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
            // Check threshold conditions
            CheckThresholdConditions();
            
            // Update countdown
            currentTime -= Time.deltaTime;
            currentTime = Mathf.Max(0, currentTime); // Prevent negative values
            
            // Update display
            UpdateCountdownDisplay();
            
            yield return null;
        }
        
        // Countdown complete
        OnCountdownComplete();
        countdownCoroutine = null;
    }

    private void CheckThresholdConditions()
    {
        // Check if countdown is under threshold (called every frame while under threshold)
        if (currentTime <= thresholdSeconds && currentTime > 0)
        {
            OnCountdownUnderThreshold();
        }
        // Check if countdown is above threshold (called every frame while above threshold)
        else if (currentTime > thresholdSeconds)
        {
            OnCountdownAboveThreshold();
        }
        
        // Check if countdown just went below threshold (called once)
        if (currentTime <= thresholdSeconds && !isBelowThresholdCalled)
        {
            OnCountdownBelowThreshold();
            isBelowThresholdCalled = true;
        }
    }

    private void UpdateCountdownDisplay()
    {
        string formattedTime = FormatTime(currentTime);
        
        // Update TextMeshPro text if auto-update is enabled
        if (autoUpdateText)
        {
            UpdateTimerText(formattedTime);
        }
    }
    
    private void OnCountdownUnderThreshold()
    {
        // Update text color when under threshold
        if (timerText != null)
        {
            timerText.color = thresholdColor;
        }
        
        // Increase music pitch
        if (musicManagerAS != null)
        {
            musicManagerAS.pitch = 1.1f;
        }
    }

    private void OnCountdownAboveThreshold()
    {
        // Update text color when above threshold
        if (timerText != null)
        {
            timerText.color = normalColor;
        }
        
        // Reset music pitch
        if (musicManagerAS != null)
        {
            musicManagerAS.pitch = 1.0f;
        }
    }

    private void OnCountdownBelowThreshold()
    {
        Debug.Log("Warning: Time below threshold!");
    }

    private void OnCountdownComplete()
    {
        // Add countdown completion logic here
        // For example: trigger game over, play sound, etc.
        Debug.Log("Countdown complete!");

        // Reset music pitch when timer completes
        if (musicManagerAS != null)
        {
            musicManagerAS.pitch = 1.0f;
        }

        SceneManager.LoadScene(deadSceneName);
    }

    private void UpdateTimerText(string timeString)
    {
        if (timerText != null)
        {
            timerText.text = timeString;
        }
    }

    /// <summary>
    /// Formats time in 00:00:00 format (minutes:seconds:milliseconds)
    /// </summary>
    private string FormatTime(float timeInSeconds)
    {
        int minutes = Mathf.FloorToInt(timeInSeconds / 60f);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60f);
        int milliseconds = Mathf.FloorToInt((timeInSeconds * 100f) % 100f);
        
        return $"{minutes:00}:{seconds:00}:{milliseconds:00}";
    }

    /// <summary>
    /// Stops and resets the countdown timer
    /// </summary>
    public void StopAndReset()
    {
        // Stop coroutine
        if (countdownCoroutine != null)
        {
            StopCoroutine(countdownCoroutine);
            countdownCoroutine = null;
        }
        
        // Reset time and flags
        CalculateTotalTime();
        currentTime = totalTimeInSeconds;
        isThresholdCalled = false;
        isBelowThresholdCalled = false;
        
        // Reset text color and music pitch
        if (timerText != null)
        {
            timerText.color = normalColor;
        }

        if (musicManagerAS != null)
        {
            musicManagerAS.pitch = 1.0f;
        }


        // Update display to initial time
        //UpdateCountdownDisplay();

        timerText.text = "SAFE";
    }

    /// <summary>
    /// Stops the countdown without resetting
    /// </summary>
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