using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using System.Collections;

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

    private int currentDialogueIndex = -1;
    private int currentLineIndex = 0;
    private bool isDialogueActive = false;

    [Header("Audio")]
    [SerializeField] private AudioSource dialogueAudioSource;
    private Coroutine stopAudioCoroutine;

    private Coroutine typewriterCoroutine;
    private bool isTyping = false;

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
                ShowNextLine();
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
            if (typewriterCoroutine != null)
            {
                StopCoroutine(typewriterCoroutine);
                typewriterCoroutine = null;
            }

            string lineText = data.lines[currentLineIndex];

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
        }
    }

    private void CompleteCurrentLineInstantly()
    {
        if (!isDialogueActive || currentDialogueIndex < 0)
            return;

        DialogueDataNPC data = dialogues[currentDialogueIndex];
        if (data == null || data.lines == null || data.lines.Length == 0)
            return;

        if (typewriterCoroutine != null)
        {
            StopCoroutine(typewriterCoroutine);
            typewriterCoroutine = null;
        }

        if (dialogueText != null)
        {
            currentLineIndex = Mathf.Clamp(currentLineIndex, 0, data.lines.Length - 1);
            dialogueText.text = data.lines[currentLineIndex];
        }

        isTyping = false;
    }

    private IEnumerator TypeLine(string text, float duration)
    {
        isTyping = true;

        if (dialogueText != null)
        {
            dialogueText.text = string.Empty;
        }

        if (string.IsNullOrEmpty(text) || duration <= 0f)
        {
            if (dialogueText != null)
            {
                dialogueText.text = text;
            }

            isTyping = false;
            typewriterCoroutine = null;
            yield break;
        }

        int length = text.Length;
        float delay = duration / Mathf.Max(1, length);

        for (int i = 0; i < length; i++)
        {
            if (dialogueText != null)
            {
                dialogueText.text = text.Substring(0, i + 1);
            }

            yield return new WaitForSeconds(delay);
        }

        isTyping = false;
        typewriterCoroutine = null;
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
            npcNameText.text = data.npcName;
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

    private IEnumerator StopAudioAfterDuration(float duration)
    {
        int lineIndexAtStart = currentLineIndex;
        yield return new WaitForSeconds(duration);

        if (isDialogueActive && currentDialogueIndex >= 0 && lineIndexAtStart == currentLineIndex && dialogueAudioSource != null)
        {
            dialogueAudioSource.Stop();
        }
    }
}
