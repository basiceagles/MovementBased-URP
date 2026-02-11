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
        if (animator != null && animator.gameObject != null)
        {
            animator.gameObject.SetActive(true);
            animator.Play(damagedAnimClip.name);
        }
    }

    public void DisableRedEyeOverlay()
    {
        if (animator != null && animator.gameObject != null)
        {
            animator.gameObject.SetActive(false);
        }
    }
}
