using UnityEngine;

public class LeverChance : MonoBehaviour
{
    [Header("Settings")]
    public GameObject targetLever; 
    public float activationRange = 15f; 
    public float rollChance = 50f;

    private Camera playerCamera;
    private bool hasRolled = false;

    private void Start()
    {
        playerCamera = Camera.main;
        targetLever.SetActive(false);
    }

    private void Update()
    {
        if (hasRolled)
        {
            return;
        }

        if (playerCamera == null)
        {
            playerCamera = Camera.main;
            return;
        }

        if (targetLever == null)
        {
            return;
        }

        float rangeSqr = activationRange * activationRange;
        if ((transform.position - playerCamera.transform.position).sqrMagnitude <= rangeSqr)
        {
            hasRolled = true;
            float roll = Random.Range(0f, 100f);
            targetLever.SetActive(roll <= rollChance);
        }
    }
}
