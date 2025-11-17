using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using TMPro;

public class MusicManager : MonoBehaviour
{
    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] audioClips;
    [SerializeField] private AudioClip ambientAudioClip;
    [SerializeField] private AudioSource ambientAudioSource;
    
    [Header("Volume Settings")]
    [SerializeField] private float maxVolume = 1.0f;
    [SerializeField] private float fadeInDuration = 2.0f;
    
    [Header("Random Settings")]
    [SerializeField] private bool avoidRepeat = false;
    [SerializeField] private bool useShuffleSystem = true;
    
    [Header("Stop Effects")]
    [SerializeField] private bool useStopEffects = true;
    [SerializeField] private float fadeOutDuration = 2.0f;
    [SerializeField] private float reverbTailDuration = 1.5f;
    
    private int lastPlayedIndex = -1;
    private List<int> availableIndices = new List<int>();
    private List<int> playedIndices = new List<int>();
    private Coroutine fadeCoroutine;
    private AudioReverbFilter reverbFilter;
    private AudioEchoFilter echoFilter;
    private GameObject musicPlayingUI;
    private GameObject musicPlayingUITextTarget;
    private TextMeshProUGUI musicPlayingUIText;
    private Animator musicPlayingUITextAnimator;

    void Start()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
        
        reverbFilter = GetComponent<AudioReverbFilter>();
        echoFilter = GetComponent<AudioEchoFilter>();

        reverbFilter.enabled = false;
        echoFilter.enabled = false;

        musicPlayingUI = GameObject.Find("MusicPlayingUI");
        musicPlayingUITextTarget = GameObject.Find("MusicPlayingName");
        musicPlayingUIText = musicPlayingUITextTarget.GetComponent<TextMeshProUGUI>();
        musicPlayingUITextAnimator = musicPlayingUI.GetComponent<Animator>();
        
        InitializeShuffleSystem();


        //GOVNOKOD
        ambientAudioSource.Play();
    }
    
    private void InitializeShuffleSystem()
    {
        availableIndices.Clear();
        playedIndices.Clear();
        
        for (int i = 0; i < audioClips.Length; i++)
        {
            availableIndices.Add(i);
        }
        
        ShuffleList(availableIndices);
    }
    
    public void PlayMusic()
    {
        if (audioClips == null || audioClips.Length == 0)
        {
            Debug.LogWarning("No audio clips assigned!");
            return;
        }
        
        if (audioSource.isPlaying)
        {
            audioSource.Stop();
        }
        
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
        
        int randomIndex = GetRandomClipIndex();
        
        audioSource.clip = audioClips[randomIndex];
        audioSource.volume = 0f;
        audioSource.Play();
        
        fadeCoroutine = StartCoroutine(FadeIn());

        lastPlayedIndex = randomIndex;

        musicPlayingUITextAnimator.SetTrigger("CallMusicPlayer");
        musicPlayingUIText.text = audioClips[randomIndex].name;

        //Debug.Log($"Now playing: {audioClips[randomIndex].name} (Index: {randomIndex})");

        ambientAudioSource.Stop();
    }
    
    public void StopPlayingMusic()
    {
        if (audioSource.isPlaying)
        {
            if (useStopEffects && reverbFilter != null && echoFilter != null)
            {
                StartCoroutine(StopWithEffects());
            }
            else
            {
                audioSource.Stop();
                Debug.Log("Music stopped");
            }
        }
        ambientAudioSource.Play();
    }
    
    private IEnumerator FadeIn()
    {
        float timer = 0f;
        
        while (timer < fadeInDuration)
        {
            timer += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(0f, maxVolume, timer / fadeInDuration);
            yield return null;
        }
        
        audioSource.volume = maxVolume;
    }
    
    private IEnumerator StopWithEffects()
    {
        reverbFilter.enabled = true;
        echoFilter.enabled = true;
        
        
        float startVolume = audioSource.volume;
        float timer = 0f;
        
        while (timer < fadeOutDuration)
        {
            timer += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, 0f, timer / fadeOutDuration);
            yield return null;
        }
        
        audioSource.volume = 0f;
        
        yield return new WaitForSeconds(reverbTailDuration);
        
        audioSource.Stop();
        audioSource.volume = maxVolume;

        reverbFilter.enabled = false;
        echoFilter.enabled = false;
        
        Debug.Log("Music stopped with effects");
    }
    
    private int GetRandomClipIndex()
    {
        if (audioClips.Length == 1)
        {
            return 0;
        }
        
        if (useShuffleSystem)
        {
            return GetShuffledIndex();
        }
        else if (avoidRepeat && lastPlayedIndex != -1)
        {
            int randomIndex;
            do
            {
                randomIndex = Random.Range(0, audioClips.Length);
            } while (randomIndex == lastPlayedIndex);
            
            return randomIndex;
        }
        else
        {
            return Random.Range(0, audioClips.Length);
        }
    }
    
    private int GetShuffledIndex()
    {
        if (availableIndices.Count == 0)
        {
            ResetShuffleSystem();
        }
        
        int selectedIndex = availableIndices[0];
        availableIndices.RemoveAt(0);
        playedIndices.Add(selectedIndex);
        
        return selectedIndex;
    }
    
    private void ResetShuffleSystem()
    {
        Debug.Log("All clips played! Resetting shuffle system...");
        
        availableIndices.Clear();
        availableIndices.AddRange(playedIndices);
        playedIndices.Clear();
        
        ShuffleList(availableIndices);
    }
    
    private void ShuffleList(List<int> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            int temp = list[i];
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }
    
    public void ResetShuffle()
    {
        InitializeShuffleSystem();
        Debug.Log("Shuffle system reset");
    }
    
    public bool IsPlaying()
    {
        return audioSource.isPlaying;
    }
}