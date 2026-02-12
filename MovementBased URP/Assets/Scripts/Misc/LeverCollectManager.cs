using UnityEngine;

public class LeverCollectManager : MonoBehaviour
{
    [Header("Settings")]
    public string targetTag = "LeverManager";
    public string componentType = "Lever"; 
    [Header("References")]
    [SerializeField] private GameObject leverManager;

    private BoxCollider boxCollider;
    private Component leverComponent;

    void Start()
    {
        boxCollider = GetComponent<BoxCollider>();
        Invoke("SearchForLeverManager", 0.5f);
    }

    void SearchForLeverManager()
    {
        if (boxCollider == null) return;

        Collider[] collidersInRange = Physics.OverlapBox(
            transform.TransformPoint(boxCollider.center),
            boxCollider.size * 0.5f,
            transform.rotation
        );

        foreach (Collider collider in collidersInRange)
        {
            if (collider.CompareTag(targetTag))
            {
                leverManager = collider.gameObject;
                leverComponent = leverManager.GetComponent(componentType);
                return;
            }
        }
    }
}
