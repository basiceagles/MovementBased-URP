using UnityEngine;

public class Hyperlink : MonoBehaviour
{
    [Header("Hyperlink Settings")]
    [Tooltip("The URL to open when OpenHyperlink() is called")]
    [SerializeField] private string hyperlink = "https://www.example.com";

    public void OpenHyperlink()
    {
        Application.OpenURL(hyperlink);
    }
}
