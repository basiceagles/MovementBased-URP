using UnityEngine;

public class MoneyPickup : MonoBehaviour
{
    [SerializeField] private int moneyValue = 100;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            MoneyManager.Instance.totalMoney += moneyValue;
            Destroy(gameObject);
        }
    }
}
