using UnityEngine;

public class SetSafeFromRunner : MonoBehaviour
{
    private GameObject playerTarget;
    private PlayerController playerController;

    void Start()
    {
        playerTarget = GameObject.Find("Player");
        playerController = playerTarget.GetComponent<PlayerController>();
    }

    public void SetSafe()
    {
        playerController.isSafeFromRunner = true;
    }

    public void SetUnSafe()
    {
        playerController.isSafeFromRunner = false;
    }
}
