using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HideUIOverlays : MonoBehaviour
{
    public Button[] buttons;

    public GameObject[] overlays;
    public GameObject[] images;
    public Button firstSelectedButton;

    private void Start()
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            int index = i; 
            buttons[i].onClick.AddListener(() => OnButtonClicked(index));
        }
        EventSystem.current.SetSelectedGameObject(firstSelectedButton.gameObject);
    }

    private void OnButtonClicked(int index)
    {
        HideAllOverlays();

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