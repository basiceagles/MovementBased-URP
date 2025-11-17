using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Audio;
using System;

public enum AntiAliasingType
{
    None = 0,
    FXAA = 1,
    SMAA = 2,
    TAA = 3
}

public class Settings : MonoBehaviour
{
    [Header("Input")]
    public InputActionAsset inputActions;
    private InputAction m_OpenSettings;

    [Header("UI References")]
    public GameObject settingsUI;
    public GameObject mainMenuUI;
    public TMPro.TMP_Dropdown resolutionDropdown;
    public TMPro.TMP_Dropdown refreshRateDropdown;
    public Toggle fullscreenToggle;
    public Toggle vsyncToggle;
    public Toggle shadowsToggle;    
    public TMPro.TMP_Dropdown antiAliasingDropdown;
    public Slider brightnessSlider;
    public Slider sensitivitySlider;
    
    [Header("Settings")]
    public bool isMainMenu;
    public Volume postProcessVolume;
    public CameraController cameraController;
    public KeyRebindManager keyRebindManager;
    public Camera mainCamera;
    
    private Resolution[] resolutions;
    private List<string> uniqueResolutions = new List<string>();
    private List<int> availableRefreshRates = new List<int>();
    private int targetFPS = -1;
    private int lastDisplayCount = 0;
    private int lastMonitorRefreshRate = 0;
    
    [Header("Audio Mixer Settings")]
    [SerializeField] private AudioMixerGroup masterMixerGroup;
    [SerializeField] private AudioMixerGroup sfxMixerGroup;
    [SerializeField] private AudioMixerGroup musicMixerGroup;
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Slider musicSlider;
    [Header("Other Settings")]
    [SerializeField] private Toggle fpsCounterToggle;
    private GameObject fpsCounterTarget;
    private bool mainMenuIsActive = false;
    private AudioSource[] allAudioSourcesInScene;
    [Header("Other References")]
    [SerializeField] private PerkSelectorScript perkSelectorScript;

    private void Start()
    {
        if (isMainMenu)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        fpsCounterTarget = GameObject.Find("FpsCounterUI");

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

        // Initialize audio before loading settings
        InitializeAudio();
        SetupAntiAliasingDropdown();
        LoadSettings();

        FindAllAudioSourcesInScene();

        m_OpenSettings = InputSystem.actions.FindAction("OpenSettings");

        lastDisplayCount = Display.displays.Length;
        lastMonitorRefreshRate = Screen.currentResolution.refreshRate;

        //fix after switching scenes
        Time.timeScale = 1f;
        UnPauseAllAudioSource();
    }

    private void FindAllAudioSourcesInScene()
    {
        allAudioSourcesInScene = FindObjectsOfType<AudioSource>();
    }

    private void InitializeAudio()
    {
        // Add listeners for audio sliders
        masterSlider.onValueChanged.AddListener(MasterMixerVolume);
        sfxSlider.onValueChanged.AddListener(SfxMixerVolume);
        musicSlider.onValueChanged.AddListener(MusicMixerVolume);
        
        // Add listener for anti-aliasing dropdown
        antiAliasingDropdown.onValueChanged.AddListener(SetAntiAliasing);
        
        // Add listener for shadows toggle
        shadowsToggle.onValueChanged.AddListener(SetShadows);
    }

    public void MasterMixerVolume(float sliderValue)
    {
        float volumeDB = ConvertToDecibels(sliderValue);
        masterMixerGroup.audioMixer.SetFloat("Master", volumeDB);
        PlayerPrefs.SetFloat("MasterVolume", sliderValue);
    }

    public void SfxMixerVolume(float sliderValue)
    {
        float volumeDB = ConvertToDecibels(sliderValue);
        sfxMixerGroup.audioMixer.SetFloat("SFX", volumeDB);
        PlayerPrefs.SetFloat("SfxVolume", sliderValue);
    }

    public void MusicMixerVolume(float sliderValue)
    {
        float volumeDB = ConvertToDecibels(sliderValue);
        musicMixerGroup.audioMixer.SetFloat("Music", volumeDB);
        PlayerPrefs.SetFloat("MusicVolume", sliderValue);
    }

    private float ConvertToDecibels(float linearVolume)
    {
        // Improved volume curve for better perceived loudness
        if (linearVolume <= 0.001f)
            return -80f;
        
        // Use a more gradual curve that provides better volume at higher values
        // This will make 100% volume actually sound like 100%
        float adjustedVolume = Mathf.Pow(linearVolume, 0.7f); // Adjust the exponent for desired curve
        return Mathf.Log10(adjustedVolume) * 20f;
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
        
        int antiAliasingType = PlayerPrefs.GetInt("AntiAliasing", 1);
        antiAliasingDropdown.SetValueWithoutNotify(antiAliasingType);
        SetAntiAliasing(antiAliasingType);
        
        bool isFullscreen = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
        fullscreenToggle.SetIsOnWithoutNotify(isFullscreen);
        SetFullscreen(isFullscreen);

        bool shadowsEnabled = PlayerPrefs.GetInt("Shadows", 1) == 1;
        shadowsToggle.SetIsOnWithoutNotify(shadowsEnabled);
        SetShadows(shadowsEnabled);

        bool vsyncEnabled = PlayerPrefs.GetInt("VSync", 0) == 1;
        vsyncToggle.SetIsOnWithoutNotify(vsyncEnabled);
        SetVSync(vsyncEnabled);
        
        float brightness = PlayerPrefs.GetFloat("Brightness", 1f);
        brightnessSlider.SetValueWithoutNotify(brightness);
        SetBrightness(brightness);
        
        float sensitivity = PlayerPrefs.GetFloat("Sensitivity", 1f);
        sensitivitySlider.SetValueWithoutNotify(sensitivity);
        SetSensitivity(sensitivity);

        // Load audio settings with proper default values
        float masterVolume = PlayerPrefs.GetFloat("MasterVolume", 0.8f);
        masterSlider.SetValueWithoutNotify(masterVolume);
        MasterMixerVolume(masterVolume);

        float sfxVolume = PlayerPrefs.GetFloat("SfxVolume", 0.8f);
        sfxSlider.SetValueWithoutNotify(sfxVolume);
        SfxMixerVolume(sfxVolume);

        float musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.8f);
        musicSlider.SetValueWithoutNotify(musicVolume);
        MusicMixerVolume(musicVolume);

        bool fpsEnabled = PlayerPrefs.GetInt("FPSCounter", 0) == 1;
        fpsCounterToggle.SetIsOnWithoutNotify(fpsEnabled);
        SetFpsCounter(fpsEnabled);
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
        PlayerPrefs.SetFloat("Sensitivity", sensitivitySlider.value);
        PlayerPrefs.SetInt("FPSCounter", fpsCounterToggle.isOn ? 1 : 0);
        PlayerPrefs.SetInt("AntiAliasing", antiAliasingDropdown.value);
        PlayerPrefs.SetInt("Shadows", shadowsToggle.isOn ? 1 : 0);

        // Audio volumes are already saved when sliders change
        PlayerPrefs.Save();

        settingsUI.SetActive(false);

        mainMenuIsActive = false;

        if (!isMainMenu)
        {
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            UnPauseAllAudioSource();

            if (cameraController != null)
            {
                cameraController.enabled = true;
            }
        }

        //Some settings requires update after saving
        bool isFpsCounterOn = fpsCounterToggle.isOn;
        SetFpsCounter(isFpsCounterOn);
    }

    public void OpenMainMenuSettings()
    {
        mainMenuUI.SetActive(true);
        mainMenuIsActive = true;
        
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        

        if (!isMainMenu)
        {
            Time.timeScale = 0f;

            PauseAllAudioSource();

            if (cameraController != null)
            {
                cameraController.enabled = false;
            }
        }
    }

    public void OpenSettings()
    {
        if (!isMainMenu)
        {
            Time.timeScale = 0f;

            if (cameraController != null)
            {
                cameraController.enabled = false;
            }

            settingsUI.SetActive(true);
            mainMenuUI.SetActive(false);

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    private void PauseAllAudioSource()
    {
        if (allAudioSourcesInScene == null) return;

        foreach (AudioSource audio in allAudioSourcesInScene)
        {
            audio.Pause();
        }
    }
    
    private void UnPauseAllAudioSource()
    {
        if (allAudioSourcesInScene == null) return;

        foreach (AudioSource audio in allAudioSourcesInScene)
        {
            audio.UnPause();
        }
    }
    
    public void CloseMainMenuSettings()
    {
        if (!isMainMenu)
        {
            Time.timeScale = 1f;

            UnPauseAllAudioSource();

            if (cameraController != null)
            {
                cameraController.enabled = true;
            }

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            mainMenuUI.SetActive(false);
            mainMenuIsActive = false;
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
        if (m_OpenSettings != null && m_OpenSettings.WasPressedThisFrame() && !isMainMenu && !perkSelectorScript.isPerkMenuOpen)
        {
            if (!mainMenuIsActive)
            {
                FindAllAudioSourcesInScene();
                OpenMainMenuSettings();
            }
            else if (mainMenuIsActive)
            {
                CloseMainMenuSettings();
            }
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
    
    private void SetupAntiAliasingDropdown()
    {
        antiAliasingDropdown.ClearOptions();
        List<string> antiAliasingOptions = new List<string>
        {
            "None",
            "FXAA",
            "SMAA",
            "TAA"
        };
        antiAliasingDropdown.AddOptions(antiAliasingOptions);
    }
    
    public void SetAntiAliasing(int antiAliasingIndex)
    {
        AntiAliasingType aaType = (AntiAliasingType)antiAliasingIndex;
        var cameraData = mainCamera.GetUniversalAdditionalCameraData();
        
        switch (aaType)
        {
            case AntiAliasingType.None:
                cameraData.antialiasing = AntialiasingMode.None;
                break;
            case AntiAliasingType.FXAA:
                cameraData.antialiasing = AntialiasingMode.FastApproximateAntialiasing;
                break;
            case AntiAliasingType.SMAA:
                cameraData.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;
                cameraData.antialiasingQuality = AntialiasingQuality.High;
                break;
            case AntiAliasingType.TAA:
                cameraData.antialiasing = AntialiasingMode.TemporalAntiAliasing;
                break;
        }
        
        PlayerPrefs.SetInt("AntiAliasing", antiAliasingIndex);
    }
    
    public void SetShadows(bool enabled)
    {
        if (mainCamera != null)
        {
            var cameraData = mainCamera.GetUniversalAdditionalCameraData();
            if (cameraData != null)
            {
                cameraData.renderShadows = enabled;
            }
        }
        
        PlayerPrefs.SetInt("Shadows", enabled ? 1 : 0);
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
    
    public void SetFpsCounter(bool enabled)
    {
        if (enabled && fpsCounterTarget != null)
        {
            fpsCounterTarget.SetActive(true);
        }
        else if (fpsCounterTarget != null)
        {
            fpsCounterTarget.SetActive(false);
        }
    }
}