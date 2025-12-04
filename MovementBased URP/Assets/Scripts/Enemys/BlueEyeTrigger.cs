using UnityEngine;

public class BlueEyeTrigger : MonoBehaviour
{
    private RedEyeDetector redEyeDetector;

    private void Start()
    {
        redEyeDetector = FindObjectOfType<RedEyeDetector>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && redEyeDetector != null)
        {
            if (transform.parent != null)
            {
                redEyeDetector.OnBlueEyeEnter(transform.parent.gameObject);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && redEyeDetector != null)
        {
            if (transform.parent != null)
            {
                redEyeDetector.OnBlueEyeExit(transform.parent.gameObject);
            }
        }
    }
}
