using UnityEngine;

public class DialogueStarterByName : MonoBehaviour
{
    [SerializeField] private DialoguesSystem dialogueSystem;
    [SerializeField] private GameObject UITip;
    [SerializeField] private CameraController cameraController;
    [SerializeField] private string npcName;

    private void OnEnable()
    {
        if (dialogueSystem != null)
        {
            dialogueSystem.DialogueEnded += OnDialogueEnded;
        }
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
        if (dialogueSystem == null || string.IsNullOrEmpty(npcName))
            return;

        int index = FindDialogueIndexByName();
        if (index >= 0)
        {
            if (cameraController != null)
            {
                cameraController.SetCameraMovementEnabled(false);
            }
            dialogueSystem.StartDialogue(index);
        }
    }

    private int FindDialogueIndexByName()
    {
        // Access the dialogues array via reflection-like pattern is not ideal,
        // so add a helper to DialoguesSystem if you later want cleaner access.
        // For now, we just search using a small helper method you can extend.
        return FindIndexInSystem(dialogueSystem, npcName);
    }

    private int FindIndexInSystem(DialoguesSystem system, string targetName)
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
            if (data != null && data.npcName == targetName)
            {
                return i;
            }
        }

        return -1;
    }

    private void OnDialogueEnded()
    {
        if (cameraController != null)
        {
            cameraController.SetCameraMovementEnabled(true);
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
