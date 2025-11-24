using UnityEngine;
using TMPro;

public class DialogueUIReferences : MonoBehaviour
{
    public GameObject dialogueUI;
    public TextMeshProUGUI dialogueText;

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
