using UnityEngine;

public class ScannerScript : MonoBehaviour
{
    private PlayerController playerController;

    private void Start()
    {
        playerController = GetComponentInParent<PlayerController>();
        if (playerController == null)
            playerController = FindObjectOfType<PlayerController>();
    }
}

