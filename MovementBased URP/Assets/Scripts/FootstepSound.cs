using SmallHedge.SoundManager;
using UnityEngine;

public class FootstepSound : MonoBehaviour
{
    [Header("Footstep Settings")]
    [SerializeField] private float walkStepDelay = 0.5f; // Delay for walking footsteps
    [SerializeField] private float runStepDelay = 0.3f; // Delay for running footsteps

    PlayerController playerController;
    private float playerSpeed;
    private float stepDelay; // Current step delay based on the movement mode
    private float nextStepTime = 0f; // Time when the next footstep should play

    private void Start()
    {
        playerController = FindObjectOfType<PlayerController>();
    }

    private void Update()
    {
        playerSpeed = playerController.currentSpeed;
        if (playerSpeed != 0)
        {
            if (playerSpeed < 5f)
            {
                stepDelay = walkStepDelay;
            }
            else
            {
                stepDelay = runStepDelay;
            }

            if (Time.time >= nextStepTime)
            {
                SoundManager.PlaySound(SoundType.FOOTSTEP);

                nextStepTime = Time.time + stepDelay;
            }
        }
    }
}