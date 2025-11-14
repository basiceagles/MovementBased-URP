using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ButtonPerkController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Button and GameObject Arrays")]
    public Button[] buttons;
    public GameObject[] gameObjects;

    [Header("Animation Parameters")]
    public AnimationClip animationClip;
    [Header("Audio Source")]
    public AudioSource audioSource;


    void Start()
    {
        // Validate arrays
        if (buttons.Length != gameObjects.Length)
        {
            Debug.LogError("Buttons and GameObjects arrays must have the same length!");
            return;
        }

        // Add listeners for each button
        for (int i = 0; i < buttons.Length; i++)
        {
            int index = i; // Important: capture the index for the closure
            
            if (buttons[i] != null)
            {
                // Add event triggers for highlight/unhighlight
                var trigger = buttons[i].gameObject.GetComponent<EventTrigger>();
                if (trigger == null)
                    trigger = buttons[i].gameObject.AddComponent<EventTrigger>();

                // Create entry for pointer enter (highlight)
                var entryEnter = new EventTrigger.Entry();
                entryEnter.eventID = EventTriggerType.PointerEnter;
                entryEnter.callback.AddListener((data) => { OnButtonHighlighted(index); });
                trigger.triggers.Add(entryEnter);

                // Create entry for pointer exit (unhighlight)
                var entryExit = new EventTrigger.Entry();
                entryExit.eventID = EventTriggerType.PointerExit;
                entryExit.callback.AddListener((data) => { OnButtonUnhighlighted(index); });
                trigger.triggers.Add(entryExit);
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // This is for the component itself, not needed if using EventTrigger
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // This is for the component itself, not needed if using EventTrigger
    }

    private void OnButtonHighlighted(int index)
    {
        if (IsValidIndex(index))
        {
            ShowPerkUI(index);
            PlayAnimation(index, animationClip);
            audioSource.Play();
            if (index == 0)
            {
                audioSource.loop = true;
            }
        }
    }

    private void OnButtonUnhighlighted(int index)
    {
        if (IsValidIndex(index))
        {
            HidePerkUI(index);
            audioSource.Stop();
            audioSource.loop = false;
        }
    }

    private void ShowPerkUI(int index)
    {
        GameObject perkUI;
        if (gameObjects[index] != null)
        {
            perkUI = gameObjects[index];
            perkUI.SetActive(true);
        }
    }

    private void HidePerkUI(int index)
    {
        GameObject perkUI;
        if (gameObjects[index] != null)
        {
            perkUI = gameObjects[index];
            perkUI.SetActive(false);
        }
    }

    private void PlayAnimation(int index, AnimationClip animationName)
    {
        if (gameObjects[index] != null)
        {
            Animator animator = gameObjects[index].GetComponent<Animator>();
            if (animator != null)
            {
                animator.Play(animationName.name);
            }
        }
    }

    private bool IsValidIndex(int index)
    {
        if (index < 0 || index >= buttons.Length || index >= gameObjects.Length)
        {
            return false;
        }

        if (buttons[index] == null)
        {
            return false;
        }

        if (gameObjects[index] == null)
        {
            return false;
        }

        return true;
    }
}