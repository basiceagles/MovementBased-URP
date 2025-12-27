using UnityEngine;
using UnityEngine.Localization.Settings;

public class PreloadLanguage : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        int languageIndex = PlayerPrefs.GetInt("Language", 1);

        if (LocalizationSettings.AvailableLocales.Locales.Count > languageIndex)
        {
            LocalizationSettings.SelectedLocale =
                LocalizationSettings.AvailableLocales.Locales[languageIndex];
        }
    }
}
