using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;  // Required for UnityEvent

public class DialoguesSystem : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private TextMeshProUGUI npcNameText;

    [Header("Dialogue Data")] 
    [SerializeField] private DialogueDataNPC[] dialogues;

    [Header("Settings")]
    [SerializeField] private int startDialogueIndex = 0;
    [SerializeField] private bool dontHideOnClick = false;

    private int currentDialogueIndex = -1;
    private int currentLineIndex = 0;
    private bool isDialogueActive = false;

    [Header("Text Effects")]
    [SerializeField] private bool enableTextShake = false;
    [SerializeField] [Range(0f, 10f)] private float shakeIntensity = 1f;
    [SerializeField] [Range(0.01f, 0.5f)] private float shakeSpeed = 0.1f;

    [Header("Audio")]
    [SerializeField] private AudioSource dialogueAudioSource;
    private Coroutine stopAudioCoroutine;

    private Coroutine typewriterCoroutine;
    private bool isTyping = false;

    // New feature: List of UnityEvents to trigger after the last dialogue is skipped
    [Header("Post-Dialogue Events")]
    [SerializeField] private List<UnityEvent> postDialogueEvents = new List<UnityEvent>(); // List of UnityEvents to trigger after the last line

    public event Action DialogueStarted;
    public event Action DialogueEnded;

    void Start()
    {
        HideDialogue();
    }

    void Update()
    {
        if (!isDialogueActive)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            if (isTyping)
            {
                CompleteCurrentLineInstantly();
            }
            else
            {
                DialogueDataNPC data = dialogues[currentDialogueIndex];
                bool isLastLine = (currentLineIndex >= data.lines.Length - 1);

                // If it's the last line and 'dontHideOnClick' is true, do nothing
                if (!isLastLine || !dontHideOnClick)
                {
                    ShowNextLine();
                }
                
                // If it's the last line, trigger post-dialogue events
                if (isLastLine)
                {
                    TriggerPostDialogueEvents();
                }
            }
        }
    }

    public void StartDialogue(int index)
    {
        if (dialogues == null || dialogues.Length == 0)
            return;

        if (index < 0 || index >= dialogues.Length)
            return;

        currentDialogueIndex = index;
        currentLineIndex = 0;
        isDialogueActive = true;

        ShowDialogue();
        UpdateDialogueText();
        UpdateNpcName();

        DialogueStarted?.Invoke();

        PlayCurrentLineAudio();
    }

    public void StartDefaultDialogue()
    {
        StartDialogue(startDialogueIndex);
    }

    public void ShowDialogue()
    {
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(true);
        }
    }

    public void HideDialogue()
    {
        bool wasActive = isDialogueActive;

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }

        isDialogueActive = false;
        currentDialogueIndex = -1;
        currentLineIndex = 0;

        // Stop any pending audio and timers when dialogue is closed
        if (stopAudioCoroutine != null)
        {
            StopCoroutine(stopAudioCoroutine);
            stopAudioCoroutine = null;
        }

        if (dialogueAudioSource != null)
        {
            dialogueAudioSource.Stop();
        }

        if (typewriterCoroutine != null)
        {
            StopCoroutine(typewriterCoroutine);
            typewriterCoroutine = null;
        }

        isTyping = false;

        if (wasActive)
        {
            DialogueEnded?.Invoke();
        }
    }

    private void ShowNextLine()
    {
        if (!isDialogueActive || currentDialogueIndex < 0)
            return;

        DialogueDataNPC data = dialogues[currentDialogueIndex];
        if (data == null || data.lines == null || data.lines.Length == 0)
        {
            HideDialogue();
            return;
        }

        currentLineIndex++;

        if (currentLineIndex >= data.lines.Length)
        {
            HideDialogue();
        }
        else
        {
            UpdateDialogueText();
            PlayCurrentLineAudio();
        }
    }

    private void UpdateDialogueText()
    {
        if (!isDialogueActive || currentDialogueIndex < 0)
            return;

        DialogueDataNPC data = dialogues[currentDialogueIndex];
        if (data == null || data.lines == null || data.lines.Length == 0)
            return;

        currentLineIndex = Mathf.Clamp(currentLineIndex, 0, data.lines.Length - 1);

        if (dialogueText != null)
        {
            // Stop any existing shake effect
            if (enableTextShake)
            {
                StopAllCoroutines();
            }

            if (typewriterCoroutine != null)
            {
                StopCoroutine(typewriterCoroutine);
                typewriterCoroutine = null;
            }

            var localizedLine = data.lines[currentLineIndex];
            string lineText = localizedLine.GetLocalizedString();

            float duration = 0f;
            if (data.audioClipPlayDurations != null && currentLineIndex >= 0 && currentLineIndex < data.audioClipPlayDurations.Length)
            {
                duration = data.audioClipPlayDurations[currentLineIndex];
            }

            if (duration <= 0f)
            {
                dialogueText.text = lineText;
                isTyping = false;
            }
            else
            {
                typewriterCoroutine = StartCoroutine(TypeLine(lineText, duration));
            }

            // Start shake effect if enabled
            if (enableTextShake && isActiveAndEnabled)
            {
                StartCoroutine(ShakeText());
            }
        }
    }

        private void CompleteCurrentLineInstantly()
    {
        if (!isDialogueActive || currentDialogueIndex < 0)
            return;

        DialogueDataNPC data = dialogues[currentDialogueIndex];
        if (data == null || data.lines == null || data.lines.Length == 0)
            return;

        // Stop typewriter
        if (typewriterCoroutine != null)
        {
            StopCoroutine(typewriterCoroutine);
            typewriterCoroutine = null;
        }

        if (stopAudioCoroutine != null)
        {
            StopCoroutine(stopAudioCoroutine);
            stopAudioCoroutine = null;
        }

        if (dialogueAudioSource != null && dialogueAudioSource.isPlaying)
        {
            dialogueAudioSource.Stop();
        }

        if (dialogueText != null)
        {
            currentLineIndex = Mathf.Clamp(currentLineIndex, 0, data.lines.Length - 1);
            dialogueText.text = data.lines[currentLineIndex].GetLocalizedString();
        }

        isTyping = false;
    }


    private void UpdateNpcName()
    {
        if (!isDialogueActive || currentDialogueIndex < 0)
            return;

        DialogueDataNPC data = dialogues[currentDialogueIndex];
        if (data == null)
            return;

        if (npcNameText != null)
        {
            npcNameText.text = data.npcName.GetLocalizedString();
        }
    }

    public void SetDialogueAudioSource(AudioSource source)
    {
        dialogueAudioSource = source;
    }

    private void PlayCurrentLineAudio()
    {
        if (dialogueAudioSource == null || !isDialogueActive || currentDialogueIndex < 0)
            return;

        DialogueDataNPC data = dialogues[currentDialogueIndex];
        if (data == null)
            return;

        if (stopAudioCoroutine != null)
        {
            StopCoroutine(stopAudioCoroutine);
            stopAudioCoroutine = null;
        }

        AudioClip clip = null;
        if (data.voiceClips != null && currentLineIndex >= 0 && currentLineIndex < data.voiceClips.Length)
        {
            clip = data.voiceClips[currentLineIndex];
        }

        dialogueAudioSource.Stop();

        if (clip != null)
        {
            dialogueAudioSource.clip = clip;
            dialogueAudioSource.Play();
        }

        float duration = 0f;
        if (data.audioClipPlayDurations != null && currentLineIndex >= 0 && currentLineIndex < data.audioClipPlayDurations.Length)
        {
            duration = data.audioClipPlayDurations[currentLineIndex];
        }

        if (duration > 0f)
        {
            stopAudioCoroutine = StartCoroutine(StopAudioAfterDuration(duration));
        }
    }

    private IEnumerator TypeLine(string line, float duration)
    {
        isTyping = true;
        dialogueText.text = "";
        float timePerChar = duration / line.Length;

        foreach (char c in line)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(timePerChar);
        }

        isTyping = false;
        typewriterCoroutine = null;
    }

    private IEnumerator ShakeText()
    {
        if (dialogueText == null) yield break;

        RectTransform textRect = dialogueText.rectTransform;
        if (textRect == null) yield break;

        Vector3 originalPos = textRect.localPosition;
        float timer = 0f;

        while (isDialogueActive && dialogueText != null && textRect != null)
        {
            timer += Time.deltaTime * shakeSpeed * 50f;
            float offsetX = (Mathf.PerlinNoise(timer, 0) * 2 - 1) * shakeIntensity;
            float offsetY = (Mathf.PerlinNoise(0, timer) * 2 - 1) * shakeIntensity;

            textRect.localPosition = originalPos + new Vector3(offsetX, offsetY, 0);
            yield return null;
        }

        if (textRect != null)
        {
            textRect.localPosition = originalPos;
        }
    }

    private IEnumerator StopAudioAfterDuration(float duration)
    {
        int lineIndexAtStart = currentLineIndex;
        yield return new WaitForSeconds(duration);

        if (isDialogueActive && currentDialogueIndex >= 0 && lineIndexAtStart == currentLineIndex && dialogueAudioSource != null)
        {
            dialogueAudioSource.Stop();
        }
    }

    // Method to trigger the post-dialogue events
    private void TriggerPostDialogueEvents()
    {
        foreach (var unityEvent in postDialogueEvents)
        {
            unityEvent?.Invoke();
        }
    }
}
