using System.Collections;
using SmallHedge.SoundManager;
using UnityEngine;

public class PlayerAudioSystem : MonoBehaviour
{
    [Header("Footstep Settings")]
    [SerializeField] private float walkStepDelay = 0.5f;
    [SerializeField] private float runStepDelay = 0.3f;
    [SerializeField] private float wallRunStepDelay = 0.4f;
    public AudioSource slideSFX;

    [Header("Collision Audio Settings")]
    public AudioClip[] collisionSoundClips;
    public float soundPlayInterval = 0.2f;
    public float soundPlayIntervalGround = 1f;

    private PlayerController playerController;
    private WallRunning wallRunning;
    private AudioSource audioSource;

    private float nextStepTime = 0f;
    private float nextWallStepTime = 0f;
    private bool currentlySliding;
    private bool wasInAir;
    private bool hasPlayedFallSound;
    private float lastJumpTime;
    private float airTimeCounter;
    private bool isPlayingCollisionSound;

    private static readonly Vector3[] DirectionsBuffer = new Vector3[4];

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        playerController = GetComponentInParent<PlayerController>();
        if (playerController == null)
            playerController = FindObjectOfType<PlayerController>();

        wallRunning = GetComponentInParent<WallRunning>();
        if (wallRunning == null)
            wallRunning = FindObjectOfType<WallRunning>();
    }

    private void Update()
    {
        if (playerController == null) return;

        HandleFootsteps();
        HandleAirAndFallSounds();
    }

    private void HandleFootsteps()
    {
        float playerSpeed = playerController.horizontalVelocity.magnitude;
        bool isGrounded = playerController.isGrounded;
        bool isSliding = playerController.isSliding;

        bool isWallRunningActive = wallRunning != null && wallRunning.IsWallRunning;
        bool isActivelyWallRunning = wallRunning != null && wallRunning.IsActivelyWallRunning;

        if (playerSpeed > 0.01f && isGrounded && !isSliding && !isWallRunningActive)
        {
            float stepDelay = playerSpeed < 5f ? walkStepDelay : runStepDelay;

            if (Time.time >= nextStepTime)
            {
                PlayFootstepByTag();
                nextStepTime = Time.time + stepDelay;
            }
        }

        if (playerSpeed > 0.01f && isActivelyWallRunning)
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
            if (slideSFX != null) slideSFX.Play();
            currentlySliding = true;
        }
        else if (!isSliding && currentlySliding)
        {
            if (slideSFX != null) slideSFX.Stop();
            currentlySliding = false;
        }
    }

    private static float lastGlobalFallSoundTime = -10f;
    private const float FALL_SOUND_COOLDOWN = 0.5f;

    private void HandleAirAndFallSounds()
    {
        bool isJumping = playerController.isJumping;
        bool isGrounded = playerController.isGrounded;

        if (!isGrounded)
        {
            airTimeCounter += Time.deltaTime;
            if (airTimeCounter > 0.15f)
            {
                wasInAir = true;
                hasPlayedFallSound = false;
            }
        }
        else
        {
            if (wasInAir && !isJumping && !hasPlayedFallSound)
            {
                if (Time.time - lastJumpTime >= 0.4f && airTimeCounter >= 0.2f && (Time.time - lastGlobalFallSoundTime >= FALL_SOUND_COOLDOWN))
                {
                    lastGlobalFallSoundTime = Time.time;
                    hasPlayedFallSound = true;
                    SoundManager.PlaySound(SoundType.FALL);
                }
            }

            if (!isJumping)
            {
                wasInAir = false;
                airTimeCounter = 0f;
            }
        }
    }

    public void CallJumpSoundMethod()
    {
        SoundManager.PlaySound(SoundType.JUMP);
        lastJumpTime = Time.time;
        airTimeCounter = 0f;
    }

    private void PlayFootstepByTag()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 2f))
        {
            if (hit.collider.CompareTag("Metal"))
            {
                SoundManager.PlaySound(SoundType.FOOTSTEPMETAL);
            }
            else
            {
                SoundManager.PlaySound(SoundType.FOOTSTEP);
            }
        }
    }

    private void PlayWallRunSound()
    {
        DirectionsBuffer[0] = transform.right;
        DirectionsBuffer[1] = -transform.right;
        DirectionsBuffer[2] = transform.forward;
        DirectionsBuffer[3] = -transform.forward;

        for (int i = 0; i < DirectionsBuffer.Length; i++)
        {
            if (Physics.Raycast(transform.position, DirectionsBuffer[i], out RaycastHit hit, 2f))
            {
                if (hit.collider.CompareTag("Metal"))
                {
                    SoundManager.PlaySound(SoundType.FOOTSTEPMETAL);
                    break;
                }
                else
                {
                    SoundManager.PlaySound(SoundType.FOOTSTEP);
                    break;
                }
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (isPlayingCollisionSound) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            StartCoroutine(PlayRandomCollisionSound(soundPlayInterval));
        }
        else if (collision.gameObject.CompareTag("Ground"))
        {
            StartCoroutine(PlayRandomCollisionSound(soundPlayIntervalGround));
        }
    }

    private IEnumerator PlayRandomCollisionSound(float interval)
    {
        isPlayingCollisionSound = true;

        if (collisionSoundClips != null && collisionSoundClips.Length > 0 && audioSource != null)
        {
            int randomIndex = Random.Range(0, collisionSoundClips.Length);
            audioSource.PlayOneShot(collisionSoundClips[randomIndex]);
        }

        yield return new WaitForSeconds(interval);
        isPlayingCollisionSound = false;
    }
}
