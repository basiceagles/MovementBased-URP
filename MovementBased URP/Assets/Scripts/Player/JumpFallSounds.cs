using SmallHedge.SoundManager;
using UnityEngine;

public class JumpFallSounds : MonoBehaviour
{
    [SerializeField] private PlayerController playerController;
    private bool isJumping;
    private bool isGrounded;
    private bool wasInAir;
    private bool hasPlayedJumpSound;
    private bool hasPlayedFallSound;

    void Update()
    {
        isJumping = playerController.isJumping;
        isGrounded = playerController.isGrounded;

        HandlePlayJumpAndFallSounds();
    }

    private void HandlePlayJumpAndFallSounds()
    {
        if (isJumping && isGrounded && !hasPlayedJumpSound)
        {
            SoundManager.PlaySound(SoundType.JUMP);
            hasPlayedJumpSound = true;
        }
        // Reset jump sound flag when player lands or stops jumping
        if (!isJumping || !isGrounded)
        {
            hasPlayedJumpSound = false;
        }
        
        if (wasInAir && isGrounded && !isJumping && !hasPlayedFallSound)
        {
            SoundManager.PlaySound(SoundType.FALL);
            hasPlayedFallSound = true;
        }
         // Reset flags when player leaves ground
        if (!isGrounded)
        {
            wasInAir = true;
            hasPlayedFallSound = false;
        }
        // Reset wasInAir when grounded for a while (optional)
        if (isGrounded && !isJumping)
        {
            wasInAir = false;
        }
    }

    public void CallJumpSoundMethod()
    {
        SoundManager.PlaySound(SoundType.JUMP);
    }
}
