using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

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
            ShowNextLine();
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
            dialogueText.text = data.lines[currentLineIndex];
        }
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
}
