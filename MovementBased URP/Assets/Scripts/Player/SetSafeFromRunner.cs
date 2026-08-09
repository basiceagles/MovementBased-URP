using UnityEngine;

public class SetSafeFromRunner : MonoBehaviour
{
    private PlayerController playerController;

    private void Start()
    {
        playerController = GetComponentInParent<PlayerController>();
        if (playerController == null)
            playerController = FindObjectOfType<PlayerController>();
    }

    public void SetSafe()
    {
        if (playerController != null) playerController.isSafeFromRunner = true;
    }

    public void SetUnSafe()
    {
        if (playerController != null) playerController.isSafeFromRunner = false;
    }
}

