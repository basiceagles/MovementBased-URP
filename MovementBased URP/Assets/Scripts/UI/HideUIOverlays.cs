using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HideUIOverlays : MonoBehaviour
{
    // Array of buttons
    public Button[] buttons;

    // Array of overlays (GameObjects)
    public GameObject[] overlays;
    public GameObject[] images;
    public Button firstSelectedButton;

    private void Start()
    {
        // Add listeners to each button
        for (int i = 0; i < buttons.Length; i++)
        {
            int index = i; // Capture the index for the listener
            buttons[i].onClick.AddListener(() => OnButtonClicked(index));
        }
        EventSystem.current.SetSelectedGameObject(firstSelectedButton.gameObject);
    }

    private void OnButtonClicked(int index)
    {
        // Hide all overlays
        HideAllOverlays();

        // Show the overlay corresponding to the clicked button's index
        if (index >= 0 && index < overlays.Length)
        {
            overlays[index].SetActive(true);
        }

        if (index >= 0 && index < images.Length)
        {
            images[index].SetActive(true);
        }
    }

    private void HideAllOverlays()
    {
        foreach (GameObject overlay in overlays)
        {
            overlay.SetActive(false);
        }
        foreach (GameObject image in images)
        {
            image.SetActive(false);
        }
    }
}