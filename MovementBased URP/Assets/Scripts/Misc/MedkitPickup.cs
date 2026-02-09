using UnityEngine;

public class MedkitPickup : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerController playerController = collision.gameObject.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.isDamaged = false;
            }

            GameObject enemyOverlayManagerObj = GameObject.Find("EnemyOverlaysManager");
            if (enemyOverlayManagerObj != null)
            {
                EnemyOverlayManager enemyOverlayManager = enemyOverlayManagerObj.GetComponent<EnemyOverlayManager>();
                if (enemyOverlayManager != null)
                {
                    enemyOverlayManager.DisableRedEyeOverlay();
                }
            }

            Destroy(gameObject);
        }
    }
}
