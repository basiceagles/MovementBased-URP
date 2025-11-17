using UnityEngine;
using UnityEngine.Audio;

public class SetUpMixers : MonoBehaviour
{
    [Header("Audio Mixer Settings")]
    [SerializeField] private AudioMixerGroup masterMixerGroup;
    [SerializeField] private AudioMixerGroup sfxMixerGroup;
    [SerializeField] private AudioMixerGroup musicMixerGroup;
    
    void Start()
    {
        // Master Volume (with default value of 1.0 if not set)
        float masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
        masterMixerGroup.audioMixer.SetFloat("Master", ConvertToDecibels(masterVolume));

        // Music Volume (with default value of 1.0 if not set)
        float musicVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);
        musicMixerGroup.audioMixer.SetFloat("Music", ConvertToDecibels(musicVolume));

        // SFX Volume (with default value of 1.0 if not set)
        float sfxVolume = PlayerPrefs.GetFloat("SfxVolume", 1f);
        sfxMixerGroup.audioMixer.SetFloat("SFX", ConvertToDecibels(sfxVolume));
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
}