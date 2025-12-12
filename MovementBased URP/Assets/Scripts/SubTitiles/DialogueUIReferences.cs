using UnityEngine;
using TMPro;

public class DialogueUIReferences : MonoBehaviour
{
    public GameObject dialogueUI;
    public TextMeshProUGUI dialogueText;
    public bool subtitlesEnabled = true;

    public void HideDialogueUI()
    {
        if (dialogueUI != null)
        {
            dialogueUI.SetActive(false);
        }

        if (dialogueText != null)
        {
            dialogueText.gameObject.SetActive(false);
        }
    }

    public void ShowDialogueUI()
    {
        if (!subtitlesEnabled)
            return;

        if (dialogueUI != null)
        {
            dialogueUI.SetActive(true);
        }

        if (dialogueText != null)
        {
            dialogueText.gameObject.SetActive(true);
        }
    }
}
