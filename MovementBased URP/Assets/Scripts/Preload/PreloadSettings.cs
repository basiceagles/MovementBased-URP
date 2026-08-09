using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Localization.Settings;

public class PreloadSettings : MonoBehaviour
{
    [SerializeField] private AudioMixer masterMixer;
    [SerializeField] private AudioMixerGroup masterMixerGroup;
    [SerializeField] private AudioMixerGroup sfxMixerGroup;
    [SerializeField] private AudioMixerGroup musicMixerGroup;

    private void Awake()
    {
        PreloadLanguageSettings();
        PreloadVolumeSettings();
    }

    private void PreloadLanguageSettings()
    {
        int languageIndex = PlayerPrefs.GetInt("Language", 1);

        if (LocalizationSettings.AvailableLocales != null &&
            LocalizationSettings.AvailableLocales.Locales != null &&
            LocalizationSettings.AvailableLocales.Locales.Count > languageIndex)
        {
            LocalizationSettings.SelectedLocale =
                LocalizationSettings.AvailableLocales.Locales[languageIndex];
        }
    }

    private void PreloadVolumeSettings()
    {
        if (masterMixer == null) return;

        float masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
        float sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
        float musicVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);

        masterMixer.SetFloat("MasterVolume", Mathf.Log10(Mathf.Max(masterVolume, 0.0001f)) * 20);
        masterMixer.SetFloat("SFXVolume", Mathf.Log10(Mathf.Max(sfxVolume, 0.0001f)) * 20);
        masterMixer.SetFloat("MusicVolume", Mathf.Log10(Mathf.Max(musicVolume, 0.0001f)) * 20);
    }
}
