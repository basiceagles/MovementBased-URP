using UnityEngine;
using System;
using UnityEngine.Localization;

[CreateAssetMenu(fileName = "NewDialogueData", menuName = "Dialogue System/Dialogue Data")]
public class DialogueData : ScriptableObject
{
    [Header("Dialogue Information")]
    public string dialogueName;
    [TextArea(1, 3)]
    public string description;
    
    [Header("Dialogue Lines")]
    public DialogueLine[] dialogueLines;

    [Header("Global Settings")]
    public bool canBeInterrupted = true;
    public bool repeatable = true;
    
    // Runtime tracking
    [NonSerialized] public bool hasBeenPlayed = false;
}

[System.Serializable]
public class DialogueLine
{
    [Header("Localization")]
    [Tooltip("Localized string reference for this line")]
    public LocalizedString localizedText;

    [Header("Timing")]
    [Tooltip("Delay before moving to next line (in seconds)")]
    public float delayBeforeNextLine = 2f;

    [Header("Speaker")]
    [Tooltip("Optional speaker icon")]
    public Sprite speakerIcon;
    
    [Tooltip("Optional speaker name for reference")]
    public string speakerName;

    [Header("Events")]
    [Tooltip("Optional event to trigger when this line starts")]
    public UnityEngine.Events.UnityEvent onLineStart;
}