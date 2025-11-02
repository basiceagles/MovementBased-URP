using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public class Settings : MonoBehaviour
{
    [Header("Input")]
    public InputActionAsset inputActions;
    private InputAction m_OpenSettings;
    
    [Header("UI References")]
    public GameObject settingsUI;
    public TMPro.TMP_Dropdown resolutionDropdown;
    public TMPro.TMP_Dropdown refreshRateDropdown;
    public Toggle fullscreenToggle;
    public Toggle vsyncToggle;
    public Slider brightnessSlider;
    public Slider sensitivitySlider;
    
    [Header("Settings")]
    public bool isMainMenu;
    public Volume postProcessVolume;
    public CameraController cameraController;
    public KeyRebindManager keyRebindManager;
    
    private Resolution[] resolutions;
    private List<string> uniqueResolutions = new List<string>();
    private List<int> availableRefreshRates = new List<int>();
    private int targetFPS = -1;
    private int lastDisplayCount = 0;
    private int lastMonitorRefreshRate = 0;

    private void Start()
    {
        if (isMainMenu)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        
        SetupResolutionDropdown();
        SetupRefreshRateDropdown();
        
        if (!PlayerPrefs.HasKey("FirstLaunch"))
        {
            Resolution currentRes = Screen.currentResolution;
            int maxRefreshRate = currentRes.refreshRate;
            
            foreach (Resolution res in resolutions)
            {
                if (res.width == currentRes.width && res.height == currentRes.height)
                {
                    if (res.refreshRate > maxRefreshRate)
                    {
                        maxRefreshRate = res.refreshRate;
                    }
                }
            }
            
            Screen.SetResolution(currentRes.width, currentRes.height, FullScreenMode.FullScreenWindow, maxRefreshRate);
            
            int optimalIndex = 0;
            for (int i = 0; i < resolutions.Length; i++)
            {
                if (resolutions[i].width == currentRes.width && 
                    resolutions[i].height == currentRes.height &&
                    resolutions[i].refreshRate == maxRefreshRate)
                {
                    optimalIndex = i;
                    break;
                }
            }
            
            PlayerPrefs.SetInt("FirstLaunch", 1);
            PlayerPrefs.SetInt("Fullscreen", 1);
            PlayerPrefs.SetInt("ResolutionIndex", optimalIndex);
            PlayerPrefs.Save();
        }
        
        LoadSettings();
        
        m_OpenSettings = InputSystem.actions.FindAction("OpenSettings");
        
        lastDisplayCount = Display.displays.Length;
        lastMonitorRefreshRate = Screen.currentResolution.refreshRate;
    }

    private void SetupResolutionDropdown()
    {
        resolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();
        uniqueResolutions.Clear();
        
        int currentResolutionIndex = 0;
        
        for (int i = 0; i < resolutions.Length; i++)
        {
            string resolutionString = resolutions[i].width + "x" + resolutions[i].height;
            
            if (!uniqueResolutions.Contains(resolutionString))
            {
                uniqueResolutions.Add(resolutionString);
            }

            if (resolutions[i].width == Screen.currentResolution.width && 
                resolutions[i].height == Screen.currentResolution.height)
            {
                currentResolutionIndex = uniqueResolutions.IndexOf(resolutionString);
            }
        }
        
        resolutionDropdown.AddOptions(uniqueResolutions);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();
    }

    private void SetupRefreshRateDropdown()
    {
        UpdateAvailableRefreshRates();
        
        int currentRefreshRateIndex = 1;
        for (int i = 0; i < availableRefreshRates.Count; i++)
        {
            if (availableRefreshRates[i] == Screen.currentResolution.refreshRate)
            {
                currentRefreshRateIndex = i;
                break;
            }
        }
        
        refreshRateDropdown.value = currentRefreshRateIndex;
        refreshRateDropdown.RefreshShownValue();
    }

    private void UpdateAvailableRefreshRates()
    {
        availableRefreshRates.Clear();
        refreshRateDropdown.ClearOptions();
        
        string currentResolutionString = uniqueResolutions[resolutionDropdown.value];
        string[] parts = currentResolutionString.Split('x');
        int width = int.Parse(parts[0]);
        int height = int.Parse(parts[1]);
        
        List<string> refreshRateOptions = new List<string>();
        
        availableRefreshRates.Add(0);
        refreshRateOptions.Add("No limits");
        
        for (int i = 0; i < resolutions.Length; i++)
        {
            if (resolutions[i].width == width && resolutions[i].height == height)
            {
                if (!availableRefreshRates.Contains(resolutions[i].refreshRate))
                {
                    availableRefreshRates.Add(resolutions[i].refreshRate);
                    refreshRateOptions.Add(resolutions[i].refreshRate + "Hz");
                }
            }
        }
        
        refreshRateDropdown.AddOptions(refreshRateOptions);
    }

    private void LoadSettings()
    {
        LoadDropdownValue("ResolutionIndex", resolutionDropdown, uniqueResolutions.Count);
        LoadDropdownValue("RefreshRateIndex", refreshRateDropdown, availableRefreshRates.Count);
        
        bool isFullscreen = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
        fullscreenToggle.SetIsOnWithoutNotify(isFullscreen);
        SetFullscreen(isFullscreen);
        
        bool vsyncEnabled = PlayerPrefs.GetInt("VSync", 0) == 1;
        vsyncToggle.SetIsOnWithoutNotify(vsyncEnabled);
        SetVSync(vsyncEnabled);
        
        float brightness = PlayerPrefs.GetFloat("Brightness", 1f);
        brightnessSlider.SetValueWithoutNotify(brightness);
        SetBrightness(brightness);
        
        float sensitivity = PlayerPrefs.GetFloat("Sensitivity", 1f);
        sensitivitySlider.SetValueWithoutNotify(sensitivity);
        SetSensitivity(sensitivity);
    }
    
    private void LoadDropdownValue(string key, TMPro.TMP_Dropdown dropdown, int maxCount)
    {
        int savedValue = PlayerPrefs.GetInt(key, dropdown.value);
        if (savedValue < maxCount)
        {
            dropdown.value = savedValue;
        }
    }

    public void SaveAndClose()
    {
        PlayerPrefs.SetInt("ResolutionIndex", resolutionDropdown.value);
        PlayerPrefs.SetInt("RefreshRateIndex", refreshRateDropdown.value);
        PlayerPrefs.SetInt("Fullscreen", Screen.fullScreen ? 1 : 0);
        PlayerPrefs.SetInt("VSync", QualitySettings.vSyncCount);
        PlayerPrefs.SetFloat("Brightness", brightnessSlider.value);
        
        SetSensitivity(sensitivitySlider.value);
        
        PlayerPrefs.Save();
        
        settingsUI.SetActive(false);

        if (!isMainMenu)
        {
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            
            if (cameraController != null)
            {
                cameraController.enabled = true;
            }
        }
    }

    public void OpenSettings()
    {
        settingsUI.SetActive(true);
        
        if (!isMainMenu)
        {
            Time.timeScale = 0f;
            
            if (cameraController != null)
            {
                cameraController.enabled = false;
            }
        }
    }
    
    public void ResetKeybinds()
    {
        if (keyRebindManager != null)
        {
            keyRebindManager.ResetToDefaults();
        }
    }

    private void Update()
    {
        if (m_OpenSettings != null && m_OpenSettings.WasPressedThisFrame() && !isMainMenu)
        {
            OpenSettings();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        
        CheckForDisplayChanges();
        
        if (Input.GetKeyDown(KeyCode.Delete))
        {
            PlayerPrefs.DeleteAll();
        }
    }

    private void CheckForDisplayChanges()
    {
        int currentMonitorRefreshRate = Screen.currentResolution.refreshRate;
        
        if (Display.displays.Length != lastDisplayCount || currentMonitorRefreshRate != lastMonitorRefreshRate)
        {
            lastDisplayCount = Display.displays.Length;
            lastMonitorRefreshRate = currentMonitorRefreshRate;
            
            SetupResolutionDropdown();
            SetupRefreshRateDropdown();
            
            if (QualitySettings.vSyncCount == 0)
            {
                ApplyCurrentSettings();
            }
        }
    }

    private void LateUpdate()
    {
        if (targetFPS > 0 && QualitySettings.vSyncCount == 0)
        {
            float targetFrameTime = 1f / targetFPS;
            float currentFrameTime = Time.unscaledDeltaTime;
            
            if (currentFrameTime < targetFrameTime)
            {
                int sleepTime = (int)((targetFrameTime - currentFrameTime) * 1000f);
                if (sleepTime > 0)
                {
                    System.Threading.Thread.Sleep(sleepTime);
                }
            }
        }
    }

    public void SetResolution(int resolutionIndex)
    {
        UpdateAvailableRefreshRates();
        ApplyCurrentSettings();
    }

    public void SetRefreshRate(int refreshRateIndex)
    {
        ApplyCurrentSettings();
    }

    private void ApplyCurrentSettings()
    {
        string resolutionString = uniqueResolutions[resolutionDropdown.value];
        string[] parts = resolutionString.Split('x');
        int width = int.Parse(parts[0]);
        int height = int.Parse(parts[1]);
        int refreshRate = availableRefreshRates[refreshRateDropdown.value];

        int actualRefreshRate = refreshRate > 0 ? refreshRate : Screen.currentResolution.refreshRate;
        
        if (!IsResolutionSupported(width, height, actualRefreshRate))
        {
            actualRefreshRate = GetNearestSupportedRefreshRate(width, height, actualRefreshRate);
        }
        
        Screen.SetResolution(width, height, Screen.fullScreen, actualRefreshRate);
        
        if (QualitySettings.vSyncCount == 0)
        {
            if (refreshRate == 0)
            {
                targetFPS = -1;
                Application.targetFrameRate = -1;
            }
            else
            {
                targetFPS = refreshRate;
                Application.targetFrameRate = targetFPS;
            }
        }
    }

    private bool IsResolutionSupported(int width, int height, int refreshRate)
    {
        foreach (Resolution res in Screen.resolutions)
        {
            if (res.width == width && res.height == height && res.refreshRate == refreshRate)
            {
                return true;
            }
        }
        return false;
    }

    private int GetNearestSupportedRefreshRate(int width, int height, int targetRefreshRate)
    {
        int nearestRate = 60;
        int minDifference = int.MaxValue;
        
        foreach (Resolution res in Screen.resolutions)
        {
            if (res.width == width && res.height == height)
            {
                int difference = Mathf.Abs(res.refreshRate - targetRefreshRate);
                if (difference < minDifference)
                {
                    minDifference = difference;
                    nearestRate = res.refreshRate;
                }
            }
        }
        
        return nearestRate;
    }

    public void SetFullscreen(bool isFullScreen)
    {
        if (isFullScreen)
        {
            Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
        }
        else
        {
            Screen.fullScreenMode = FullScreenMode.Windowed;
        }
        Screen.fullScreen = isFullScreen;
    }
    
    public void SetVSync(bool enabled)
    {
        if (enabled)
        {
            QualitySettings.vSyncCount = 1;
            Application.targetFrameRate = -1;
            targetFPS = -1;
        }
        else
        {
            QualitySettings.vSyncCount = 0;
            int refreshRate = availableRefreshRates.Count > 0 ? availableRefreshRates[refreshRateDropdown.value] : Screen.currentResolution.refreshRate;
            
            if (refreshRate == 0)
            {
                targetFPS = -1;
                Application.targetFrameRate = -1;
            }
            else
            {
                targetFPS = refreshRate;
                Application.targetFrameRate = targetFPS;
            }
        }
    }
    public void SetBrightness(float brightness)
    {
        if (postProcessVolume != null)
        {
            postProcessVolume.weight = brightness;
        }
        else
        {
            RenderSettings.ambientIntensity = brightness;
        }
    }
    
    public void SetSensitivity(float sensitivity)
    {
        if (cameraController != null)
        {
            cameraController.SetSensitivity(sensitivity);
        }
        
        PlayerPrefs.SetFloat("Sensitivity", sensitivity);
    }
}
