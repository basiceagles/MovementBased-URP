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
    public Toggle fullscreenToggle;
    public Toggle vsyncToggle;
    public Slider brightnessSlider;
    public Slider fovSlider;
    
    [Header("Settings")]
    public bool isMainMenu;
    public Camera playerCamera;
    public Volume postProcessVolume;
    
    private Resolution[] resolutions;
    private float defaultFOV = 60f;

    void Start()
    {
        //багфикс чтобы не пропадала мышка при заходе в главное меню
        if (isMainMenu)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        
        // Автоматический поиск камеры если не назначена
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }
        
        //Для работы дропдаун меню с разрешениями экрана
        SetupResolutionDropdown();
        
        // Загрузка сохраненных настроек
        LoadSettings();
        
        m_OpenSettings = InputSystem.actions.FindAction("OpenSettings");
    }

    void SetupResolutionDropdown()
    {
        resolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();
        List<string> options = new List<string>();
        int currentResolutionindex = 0;
        
        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + "x" + resolutions[i].height + " @ " + resolutions[i].refreshRate + "Hz";
            options.Add(option);

            if (resolutions[i].width == Screen.currentResolution.width && 
                resolutions[i].height == Screen.currentResolution.height &&
                resolutions[i].refreshRate == Screen.currentResolution.refreshRate)
            {
                currentResolutionindex = i;
            }
        }
        
        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResolutionindex;
        resolutionDropdown.RefreshShownValue();
    }

    void LoadSettings()
    {
        // Разрешение
        int savedResolution = PlayerPrefs.GetInt("ResolutionIndex", resolutionDropdown.value);
        if (savedResolution < resolutions.Length)
        {
            resolutionDropdown.value = savedResolution;
            SetResolution(savedResolution);
        }
        
        // Полноэкранный режим
        bool isFullscreen = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
        fullscreenToggle.isOn = isFullscreen;
        Screen.fullScreen = isFullscreen;
        
        // V-Sync
        int vsyncValue = PlayerPrefs.GetInt("VSync", 1);
        vsyncToggle.isOn = vsyncValue == 1;
        QualitySettings.vSyncCount = vsyncValue;
        
        // Яркость
        float brightness = PlayerPrefs.GetFloat("Brightness", 1f);
        brightnessSlider.value = brightness;
        SetBrightness(brightness);
        
        // FOV
        float fov = PlayerPrefs.GetFloat("FOV", defaultFOV);
        fovSlider.value = fov;
        SetFOV(fov);
    }

    public void SaveAndClose()
    {
        // Сохранение всех настроек
        PlayerPrefs.SetInt("ResolutionIndex", resolutionDropdown.value);
        PlayerPrefs.SetInt("Fullscreen", Screen.fullScreen ? 1 : 0);
        PlayerPrefs.SetInt("VSync", QualitySettings.vSyncCount);
        PlayerPrefs.SetFloat("Brightness", brightnessSlider.value);
        PlayerPrefs.SetFloat("FOV", fovSlider.value);
        PlayerPrefs.Save();
        
        settingsUI.SetActive(false);

        if (!isMainMenu)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public void OpenSettings()
    {
        settingsUI.SetActive(true);
    }

    void Update()
    {
        if (m_OpenSettings != null && m_OpenSettings.WasPressedThisFrame() && !isMainMenu)
        {
            OpenSettings();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public void SetResolution(int resolutionIndex)
    {
        Resolution resolution = resolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
    }

    public void SetFullscreen(bool isFullScreen)
    {
        Screen.fullScreen = isFullScreen;
    }
    
    public void SetVSync(bool enabled)
    {
        QualitySettings.vSyncCount = enabled ? 1 : 0;
    }
    
    public void SetBrightness(float brightness)
    {
        if (postProcessVolume != null)
        {
            postProcessVolume.weight = brightness;
        }
        else
        {
            // Альтернативный метод через RenderSettings
            RenderSettings.ambientIntensity = brightness;
        }
    }
    
    public void SetFOV(float fov)
    {
        if (playerCamera != null)
        {
            playerCamera.fieldOfView = fov;
        }
    }
}
