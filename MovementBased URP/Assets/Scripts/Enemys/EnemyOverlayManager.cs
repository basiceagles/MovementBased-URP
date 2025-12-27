using UnityEngine;

public class EnemyOverlayManager : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private AnimationClip damagedAnimClip;
    void Start()
    {
        
    }

    public void CallRedEyeOverlay()
    {
        animator.Play(damagedAnimClip.name);
    }
}
