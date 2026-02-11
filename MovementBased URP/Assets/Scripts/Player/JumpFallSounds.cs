using SmallHedge.SoundManager;
using UnityEngine;

public class JumpFallSounds : MonoBehaviour
{
    [SerializeField] private PlayerController playerController;
    private bool isJumping;
    private bool isGrounded;
    private bool wasInAir;
    private bool hasPlayedFallSound;
    private float lastJumpTime;
    private float airTimeCounter;

    void Update()
    {
        isJumping = playerController.isJumping;
        isGrounded = playerController.isGrounded;

        HandlePlayJumpAndFallSounds();
    }

    private void HandlePlayJumpAndFallSounds()
    {
        if (wasInAir && isGrounded && !isJumping && !hasPlayedFallSound)
        {
            if (Time.time - lastJumpTime >= 0.5f && airTimeCounter >= 0.15f)
            {
                SoundManager.PlaySound(SoundType.FALL);
                hasPlayedFallSound = true;
            }
        }

        if (!isGrounded)
        {
            wasInAir = true;
            hasPlayedFallSound = false;
            airTimeCounter += Time.deltaTime;
        }
        else if (!isJumping)
        {
            wasInAir = false;
            airTimeCounter = 0f;
        }
    }

    public void CallJumpSoundMethod()
    {
        SoundManager.PlaySound(SoundType.JUMP);
        lastJumpTime = Time.time;
        airTimeCounter = 0f;
    }
}
