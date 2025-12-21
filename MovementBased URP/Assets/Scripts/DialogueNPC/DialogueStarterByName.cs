using UnityEngine;
using UnityEngine.Events;

public class DialogueStarterByName : MonoBehaviour
{
    [SerializeField] private DialoguesSystem dialogueSystem;
    [SerializeField] private GameObject UITip;
    [SerializeField] private CameraController cameraController;
    [SerializeField] private AudioSource dialogueAudioSource;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private ItemPickup itemPickup;
    [SerializeField] private string npcName;
    [SerializeField] private bool EnableCameraAtEnd = true;
    [SerializeField] private bool useEndEvents = false;
    [SerializeField] private UnityEvent[] onDialogueEndEvents;
    [SerializeField] private UnityEvent[] onDialogueStartEvents;
    [SerializeField] private string npcId; // Changed from npcName to npcId for clarity


    [SerializeField] private bool isDialoguePlaying = false; // Inspector view of whether any dialogue is active

    private static bool anyDialoguePlaying = false; // Global guard to prevent overlapping dialogues

    private bool isActiveDialogueStarter = false;

    private void OnEnable()
    {
        if (dialogueSystem != null)
        {
            dialogueSystem.DialogueEnded += OnDialogueEnded;
        }
    }

    public void SetDialoguePlaying(bool value)
    {
        isDialoguePlaying = value;
        anyDialoguePlaying = value;
    }

    private void OnDisable()
    {
        if (dialogueSystem != null)
        {
            dialogueSystem.DialogueEnded -= OnDialogueEnded;
        }
    }

    public void StartDialogue()
    {
        if (useEndEvents && onDialogueStartEvents != null)
        {
            for (int i = 0; i < onDialogueStartEvents.Length; i++)
            {
                var unityEvent = onDialogueStartEvents[i];
                if (unityEvent != null)
                {
                    unityEvent.Invoke();
                }
            }
        }

        if (dialogueSystem == null || string.IsNullOrEmpty(npcName))
            return;

        // Prevent starting a new dialogue if one is already playing anywhere
        if (anyDialoguePlaying)
            return;

        int index = FindDialogueIndexByName();
        if (index >= 0)
        {
            // Mark this instance as the one that started the current dialogue
            isActiveDialogueStarter = true;

            anyDialoguePlaying = true;
            isDialoguePlaying = true;

            if (dialogueAudioSource != null)
            {
                dialogueSystem.SetDialogueAudioSource(dialogueAudioSource);
            }

            if (playerController != null)
            {
                playerController.SetMovementEnabled(false);
            }

            if (itemPickup != null)
            {
                itemPickup.SetInteractionEnabled(false);
            }

            if (cameraController != null)
            {
                cameraController.SetCameraMovementEnabled(false);
            }
            dialogueSystem.StartDialogue(index);
        }
    }

    private int FindDialogueIndexByName()
    {
        return FindIndexInSystem(dialogueSystem, npcId);
    }

    private int FindIndexInSystem(DialoguesSystem system, string targetNpcId)
    {
        // Simple linear search through DialogueDataNPC array
        var systemType = typeof(DialoguesSystem);
        var field = systemType.GetField("dialogues", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field == null)
            return -1;
        var value = field.GetValue(system) as DialogueDataNPC[];
        if (value == null)
            return -1;
        for (int i = 0; i < value.Length; i++)
        {
            var data = value[i];
            if (data != null && data.npcId == targetNpcId)
            {
                return i;
            }
        }
        return -1;
    }

    private void OnDialogueEnded()
    {
        // Only react if this instance actually started the active dialogue
        if (!isActiveDialogueStarter)
        {
            return;
        }

        isActiveDialogueStarter = false;

        anyDialoguePlaying = false;
        isDialoguePlaying = false;

        if (EnableCameraAtEnd && cameraController != null)
        {
            cameraController.SetCameraMovementEnabled(true);
        }

        if (EnableCameraAtEnd && playerController != null)
        {
            playerController.SetMovementEnabled(true);
        }

        if (EnableCameraAtEnd && itemPickup != null)
        {
            itemPickup.SetInteractionEnabled(true);
        }

        if (useEndEvents && onDialogueEndEvents != null)
        {
            for (int i = 0; i < onDialogueEndEvents.Length; i++)
            {
                var unityEvent = onDialogueEndEvents[i];
                if (unityEvent != null)
                {
                    unityEvent.Invoke();
                }
            }
        }
    }

    public void ShowUITip()
    {
        UITip.SetActive(true);
    }

    public void HideUITip()
    {
        UITip.SetActive(false);
    }
}
