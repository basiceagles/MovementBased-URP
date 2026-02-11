using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Audio;
using System;
using UnityEngine.Localization.Settings;
using UnityEngine.SceneManagement;

public enum AntiAliasingType
{
    None = 0,
    FXAA = 1,
    SMAA = 2,
    TAA = 3
}

public class Settings : MonoBehaviour
{
    private static Settings s_Instance;
    private bool isSettingsOpenEnabled = true;

    public static Settings Instance => s_Instance;

    public static bool IsSettingsOpenEnabled => Instance != null && Instance.isSettingsOpenEnabled;

    public static void SetSettingsOpenEnabled(bool enabled)
    {
        if (Instance == null)
        {
            Debug.LogWarning("Settings instance not ready yet");
            return;
        }

        Instance.isSettingsOpenEnabled = enabled;
    }

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
    public Toggle toggleShakeCamera;
    public Toggle toggleCameraSpeedZoom;
    public Toggle toggleCameraTilt;
    public Toggle toggleSubtitles;
    public TMPro.TMP_Dropdown languageDropdown;
    public TMPro.TMP_Dropdown antiAliasingDropdown;
    public Slider brightnessSlider;
    public Slider sensitivitySlider;

    [Header("Settings")]
    public bool isMainMenu;
    public Volume postProcessVolume;
    public CameraController cameraController;
    public CameraShake camShake;
    public SpeedFOVEffect speedFovEffect;
    public DialogueUIReferences dialogueUIReferences;
    public KeyRebindManager keyRebindManager;
    public Camera mainCamera;
    public PlayerController playerController;

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
    private bool settingsIsActive = false;
    
    private float timeScaleBeforePause = 1f;

    public bool IsPaused => mainMenuIsActive || settingsIsActive;

    private AudioSource[] allAudioSourcesInScene;
    [Header("Other References")]
    [SerializeField] private PerkSelectorScript perkSelectorScript;

    private void Awake()
    {
        if (s_Instance != null && s_Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        s_Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        if (isMainMenu)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            if (playerController == null)
            {
                playerController = FindObjectOfType<PlayerController>();
            }
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

        RealTimeLoad();
        SetupAntiAliasingDropdown();
        SetupLanguageDropdown();
        LoadSettings();

        FindAllAudioSourcesInScene();

        m_OpenSettings = InputSystem.actions.FindAction("OpenSettings");

        lastDisplayCount = Display.displays.Length;
        lastMonitorRefreshRate = Screen.currentResolution.refreshRate;

        Time.timeScale = 1f;
        UnPauseAllAudioSource();
    }

    private void FindAllAudioSourcesInScene()
    {
        allAudioSourcesInScene = FindObjectsOfType<AudioSource>();
    }

    private void RealTimeLoad()
    {
        masterSlider.onValueChanged.AddListener(MasterMixerVolume);
        sfxSlider.onValueChanged.AddListener(SfxMixerVolume);
        musicSlider.onValueChanged.AddListener(MusicMixerVolume);

        antiAliasingDropdown.onValueChanged.AddListener(SetAntiAliasing);

        shadowsToggle.onValueChanged.AddListener(SetShadows);
        toggleShakeCamera.onValueChanged.AddListener(SetCameraShake);
        toggleCameraSpeedZoom.onValueChanged.AddListener(SetCameraSpeedZoom);
        toggleCameraTilt.onValueChanged.AddListener(SetCameraTilt);
        toggleSubtitles.onValueChanged.AddListener(SetSubtitles);
        languageDropdown.onValueChanged.AddListener(SetLanguage);
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
        if (linearVolume <= 0.001f)
            return -80f;

        float adjustedVolume = Mathf.Pow(linearVolume, 0.7f);
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

        bool shakeEnabled = PlayerPrefs.GetInt("ShakeCamera", 1) == 1;
        toggleShakeCamera.SetIsOnWithoutNotify(shakeEnabled);
        SetCameraShake(shakeEnabled);

        bool speedZoomEnabled = PlayerPrefs.GetInt("CameraSpeedZoom", 1) == 1;
        toggleCameraSpeedZoom.SetIsOnWithoutNotify(speedZoomEnabled);
        SetCameraSpeedZoom(speedZoomEnabled);

        bool cameraTiltEnabled = PlayerPrefs.GetInt("CameraTilt", 1) == 1;
        toggleCameraTilt.SetIsOnWithoutNotify(cameraTiltEnabled);
        SetCameraTilt(cameraTiltEnabled);

        bool subtitlesEnabled = PlayerPrefs.GetInt("Subtitles", 1) == 1;
        toggleSubtitles.SetIsOnWithoutNotify(subtitlesEnabled);
        SetSubtitles(subtitlesEnabled);

        int languageIndex = PlayerPrefs.GetInt("Language", 1);
        languageDropdown.SetValueWithoutNotify(languageIndex);
        SetLanguage(languageIndex);
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
        PlayerPrefs.SetInt("ShakeCamera", toggleShakeCamera.isOn ? 1 : 0);
        PlayerPrefs.SetInt("CameraSpeedZoom", toggleCameraSpeedZoom.isOn ? 1 : 0);
        PlayerPrefs.SetInt("CameraTilt", toggleCameraTilt.isOn ? 1 : 0);
        PlayerPrefs.SetInt("Subtitles", toggleSubtitles.isOn ? 1 : 0);
        PlayerPrefs.SetInt("Language", languageDropdown.value);

        PlayerPrefs.Save();

        settingsUI.SetActive(false);

        mainMenuIsActive = false;
        settingsIsActive = false;

        if (!isMainMenu)
        {
            Time.timeScale = timeScaleBeforePause;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            UnPauseAllAudioSource();

            if (cameraController != null)
            {
                cameraController.enabled = true;
            }

            if (playerController != null)
            {
                playerController.SetMovementEnabled(true);
            }
        }

        bool isFpsCounterOn = fpsCounterToggle.isOn;
        SetFpsCounter(isFpsCounterOn);
    }

    public void OpenMainMenuSettings()
    {
        if (!isSettingsOpenEnabled)
            return;
            
        mainMenuUI.SetActive(true);
        mainMenuIsActive = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (!isMainMenu)
        {
            timeScaleBeforePause = Time.timeScale;
            Time.timeScale = 0f;

            PauseAllAudioSource();

            if (cameraController != null)
            {
                cameraController.enabled = false;
            }

            if (playerController != null)
            {
                playerController.SetMovementEnabled(false);
            }
        }
    }

    public void OpenSettings()
    {
        if (!isMainMenu)
        {
            //timeScaleBeforePause = Time.timeScale; УДОЛИ НАХУЙ!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            Time.timeScale = 0f;

            if (cameraController != null)
            {
                cameraController.enabled = false;
            }

            if (playerController != null)
            {
                playerController.SetMovementEnabled(false);
            }

            settingsUI.SetActive(true);
            mainMenuUI.SetActive(false);

            settingsIsActive = true;

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
            Time.timeScale = timeScaleBeforePause;

            UnPauseAllAudioSource();

            if (cameraController != null)
            {
                cameraController.enabled = true;
            }

            if (playerController != null)
            {
                playerController.SetMovementEnabled(true);
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
        if (!IsSettingsOpenEnabled) return;

        if (m_OpenSettings != null && m_OpenSettings.WasPressedThisFrame() && !isMainMenu && !perkSelectorScript.isPerkMenuOpen)
        {
            if (!mainMenuIsActive && !settingsIsActive)
            {
                FindAllAudioSourcesInScene();
                OpenMainMenuSettings();
            }
            else if (!settingsIsActive)
            {
                CloseMainMenuSettings();
            }
        }
        
        CheckForDisplayChanges();
        
        if (Input.GetKeyDown(KeyCode.Delete))
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        // ВРЕМЕННО: reset уровня на кнопку R
        if (!isMainMenu && !mainMenuIsActive && Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
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

    private void SetupLanguageDropdown()
    {
        languageDropdown.ClearOptions();
        List<string> languageOptions = new List<string>
        {
            "English",
            "Русский"
        };
        languageDropdown.AddOptions(languageOptions);
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
    
    public void SetCameraShake(bool enabled)
    {
        if (camShake != null)
        {
            camShake.enabled = enabled;
        }
    }

    public void SetCameraSpeedZoom(bool enabled)
    {
        if (speedFovEffect != null)
        {
            speedFovEffect.shrinkMultiplier = enabled ? 3.5f : 0f;
        }
        
        PlayerPrefs.SetInt("CameraSpeedZoom", enabled ? 1 : 0);
    }

    public void SetCameraTilt(bool enabled)
    {
        if (cameraController != null)
        {
            cameraController.tiltAmount = enabled ? 10f : 0f;
        }
        
        PlayerPrefs.SetInt("CameraTilt", enabled ? 1 : 0);
    }

    public void SetSubtitles(bool enabled)
    {
        if (dialogueUIReferences != null)
        {
            dialogueUIReferences.subtitlesEnabled = enabled;
            
            if (!enabled)
            {
                dialogueUIReferences.HideDialogueUI();
            }
        }
        
        PlayerPrefs.SetInt("Subtitles", enabled ? 1 : 0);
    }

    public void SetLanguage(int languageIndex)
    {
        PlayerPrefs.SetInt("Language", languageIndex);
        
        if (LocalizationSettings.AvailableLocales.Locales.Count > languageIndex)
        {
            LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[languageIndex];
        }
    }
}