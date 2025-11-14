using SmallHedge.SoundManager;
using UnityEngine;

public class FootstepSound : MonoBehaviour
{
    [Header("Footstep Settings")]
    [SerializeField] private float walkStepDelay = 0.5f;
    [SerializeField] private float runStepDelay = 0.3f;
    [SerializeField] private float wallRunStepDelay = 0.4f;

    PlayerController playerController;
    WallRunning wallRunning;
    public float playerSpeed;
    private bool isGrounded;
    public bool currentlySliding;
    public bool isSliding;
    private float nextStepTime = 0f;
    private float nextWallStepTime = 0f;
    public AudioSource slideSFX;

    private void Start()
    {
        playerController = FindObjectOfType<PlayerController>();
        wallRunning = FindObjectOfType<WallRunning>();
    }

    private void Update()
    {
        playerSpeed = playerController.currentSpeed;
        isGrounded = playerController.isGrounded;
        isSliding = playerController.isSliding;
        
        if (playerSpeed != 0 && isGrounded && !isSliding && !wallRunning.IsWallRunning)
        {
            float stepDelay = playerSpeed < 5f ? walkStepDelay : runStepDelay;

            if (Time.time >= nextStepTime)
            {
                PlayFootstepByTag();
                nextStepTime = Time.time + stepDelay;
            }
        }
        
        if (playerSpeed != 0 && wallRunning.IsActivelyWallRunning)
        {
            float wallStepDelay = playerSpeed < 5f ? wallRunStepDelay : wallRunStepDelay * 0.6f;
            
            if (Time.time >= nextWallStepTime)
            {
                PlayWallRunSound();
                nextWallStepTime = Time.time + wallStepDelay;
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
    
    private void PlayWallRunSound()
    {
        RaycastHit hit;
        Vector3[] directions = {
            transform.right,     
            -transform.right,    
            transform.forward,    
            -transform.forward    
        };
        
        foreach (Vector3 direction in directions)
        {
            if (Physics.Raycast(transform.position, direction, out hit, 2f))
            {
                switch (hit.collider.tag)
                {
                    case "Wall":
                        SoundManager.PlaySound(SoundType.FOOTSTEPMETAL);
                        return;
                    default:
                        SoundManager.PlaySound(SoundType.FOOTSTEP);
                        return;
                }
            }
        }
    }
    
}