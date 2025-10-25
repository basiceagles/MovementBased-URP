using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class Settings : MonoBehaviour
{
    public InputActionAsset inputActions;
    private InputAction m_OpenSettings;
    Resolution[] resolutions;

    public TMPro.TMP_Dropdown resolutionDropdown;
    public GameObject settingsUI;
    public bool isMainMenu;

    void Start()
    {
        //багфикс чтобы не пропадала мышка при заходе в главное меню
        if (isMainMenu)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        
        //Для работы дропдаун меню с разрешениями экрана
        resolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();
        List<string> options = new List<string>();
        int currentResolutionindex = 0;
        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + "x" + resolutions[i].height;
            options.Add(option);

            if (resolutions[i].width == Screen.currentResolution.width && resolutions[i].height == Screen.currentResolution.height)
            {
                currentResolutionindex = i;
            }
        }
        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResolutionindex;
        resolutionDropdown.RefreshShownValue();

        //

        m_OpenSettings = InputSystem.actions.FindAction("OpenSettings");
    }

    public void SaveAndClose()
    {
        settingsUI.SetActive(false);

        if (!isMainMenu)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        //нужно добавить сюда все переменные + плейр перфс, чтобы оно сохраняло все переменное после нажатия этой кнопки
    }

    public void OpenSettings()
    {
        settingsUI.SetActive(true);
    }

    void Update()
    {
        if (m_OpenSettings.WasPressedThisFrame() && !isMainMenu)
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

    public void SetFullscreen (bool isFullScreen)
    {
        Screen.fullScreen = isFullScreen;
    }
}
