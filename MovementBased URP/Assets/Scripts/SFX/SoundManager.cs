using System;
using UnityEngine;
using UnityEngine.Audio;

namespace SmallHedge.SoundManager
{
    [RequireComponent(typeof(AudioSource))]
    public class SoundManager : MonoBehaviour
    {
        [SerializeField] private SoundsSO SO;
        private static SoundManager instance = null;
        private AudioSource audioSource;

        private void Awake()
        {
            if(!instance)
            {
                instance = this;
                audioSource = GetComponent<AudioSource>();
            }
        }

        public static void PlaySound(SoundType sound, AudioSource source = null, float volume = 1)
        {
            if (instance == null || instance.SO == null) return;
            int index = (int)sound;
            if (index < 0 || index >= instance.SO.sounds.Length) return;

            SoundList soundList = instance.SO.sounds[index];
            AudioClip[] clips = soundList.sounds;
            if (clips == null || clips.Length == 0) return;

            AudioClip randomClip = clips[UnityEngine.Random.Range(0, clips.Length)];

            if (source)
            {
                source.outputAudioMixerGroup = soundList.mixer;
                source.clip = randomClip;
                source.volume = volume * soundList.volume;
                source.Play();
            }
            else if (instance.audioSource != null)
            {
                instance.audioSource.outputAudioMixerGroup = soundList.mixer;
                instance.audioSource.PlayOneShot(randomClip, volume * soundList.volume);
            }
        }

        public static void StopSound(SoundType sound, AudioSource source)
        {
            if (source == null || instance == null || instance.SO == null) return;

            int index = (int)sound;
            if (index < 0 || index >= instance.SO.sounds.Length) return;

            SoundList soundList = instance.SO.sounds[index];
            AudioClip[] clips = soundList.sounds;
            if (clips == null) return;

            for (int i = 0; i < clips.Length; i++)
            {
                if (source.clip == clips[i])
                {
                    source.Stop();
                    return;
                }
            }
        }
    }

    [Serializable]
    public struct SoundList
    {
        [HideInInspector] public string name;
        [Range(0, 1)] public float volume;
        public AudioMixerGroup mixer;
        public AudioClip[] sounds;
    }
}