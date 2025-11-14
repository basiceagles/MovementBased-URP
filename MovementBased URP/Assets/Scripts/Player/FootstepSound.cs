using SmallHedge.SoundManager;
using UnityEngine;

public class FootstepSound : MonoBehaviour
{
    [Header("Footstep Settings")]
    [SerializeField] private float walkStepDelay = 0.5f;
    [SerializeField] private float runStepDelay = 0.3f;

    PlayerController playerController;
    public float playerSpeed;
    private bool isGrounded;
    public bool currentlySliding;
    public bool isSliding;
    private float stepDelay;
    private float nextStepTime = 0f;
    public AudioSource slideSFX;

    private void Start()
    {
        playerController = FindObjectOfType<PlayerController>();
    }

    private void Update()
    {
        playerSpeed = playerController.currentSpeed;
        isGrounded = playerController.isGrounded;
        isSliding = playerController.isSliding;
        if (playerSpeed != 0 && isGrounded && !isSliding)
        {
            stepDelay = playerSpeed < 5f ? walkStepDelay : runStepDelay;

            if (Time.time >= nextStepTime)
            {
                PlayFootstepByTag();
                nextStepTime = Time.time + stepDelay;
            }
        }

        if (isSliding && !currentlySliding)
        {
            slideSFX.Play();
            currentlySliding = true;
        }
        else if (!isSliding && currentlySliding)
        {
            slideSFX.Stop();
            currentlySliding = false;
        }
    }
    
    private void PlayFootstepByTag()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 2f))
        {
            string surfaceTag = hit.collider.tag;
            
            switch (surfaceTag)
            {
                case "Metal":
                    SoundManager.PlaySound(SoundType.FOOTSTEPMETAL);
                    break;
                default:
                    SoundManager.PlaySound(SoundType.FOOTSTEP);
                    break;
            }
        }
    }
    
}