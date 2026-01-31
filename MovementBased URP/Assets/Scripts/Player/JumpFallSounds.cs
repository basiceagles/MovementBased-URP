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

        
        if (wasInAir && isGrounded && !isJumping && !hasPlayedFallSound)
        {
            SoundManager.PlaySound(SoundType.FALL);
            hasPlayedFallSound = true;
        }
        if (!isGrounded)
        {
            wasInAir = true;
            hasPlayedFallSound = false;
        }
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
