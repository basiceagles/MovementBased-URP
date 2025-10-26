using UnityEngine;

public class EndLevelTrigger : MonoBehaviour
{
    public GameObject targetDoor;
    [SerializeField] string tagFilter = "Player";
    private SlideMetalDoorManager slideMetalDoorManager;
    private GameObject targetProcuderalGeneration;
    private LevelGenerator levelGenerator;
    void Start()
    {
        slideMetalDoorManager = targetDoor.GetComponent<SlideMetalDoorManager>();
        targetProcuderalGeneration = GameObject.Find("ProcuderalGeneration");
        levelGenerator = targetProcuderalGeneration.GetComponent<LevelGenerator>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.gameObject.CompareTag(tagFilter)) return;
        
        slideMetalDoorManager.CloseDoor();
        levelGenerator.RestartGeneration();
        Destroy(gameObject);
    }
}
