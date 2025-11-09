using UnityEngine;

public class EnemyOverlayManager : MonoBehaviour
{
    [SerializeField] private GameObject redEyeOverlay;
    void Start()
    {
        
    }

    public void CallRedEyeOverlay()
    {
        redEyeOverlay.SetActive(true);
    }
}
