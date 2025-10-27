using UnityEngine;

public class EndLevelTrigger : MonoBehaviour
{
    public GameObject targetDoor;
    [SerializeField] string tagFilter = "Player";
    private SlideMetalDoorManager slideMetalDoorManager;
    private GameObject targetProcuderalGeneration;
    private LevelGenerator levelGenerator;
    private GameObject targetMusicManager;
    private MusicManager musicManagerScript;
    private GameObject targetPerkSelector;
    private PerkSelector perkSelector;
    private Animator perkSelectorAnimator;

    void Start()
    {
        slideMetalDoorManager = targetDoor.GetComponent<SlideMetalDoorManager>();
        targetProcuderalGeneration = GameObject.Find("ProcuderalGeneration");
        levelGenerator = targetProcuderalGeneration.GetComponent<LevelGenerator>();
        targetMusicManager = GameObject.Find("MusicManager");
        musicManagerScript = targetMusicManager.GetComponent<MusicManager>();
        targetPerkSelector = GameObject.Find("PerkSelector");
        perkSelector = targetPerkSelector.GetComponent<PerkSelector>();
        perkSelectorAnimator = targetPerkSelector.GetComponent<Animator>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.gameObject.CompareTag(tagFilter)) return;
        
        slideMetalDoorManager.CloseDoor();
        levelGenerator.RestartGeneration();
        musicManagerScript.StopPlayingMusic();
        perkSelector.OpenPerkMenu();
        perkSelectorAnimator.Play("PerkSelectorMenu");
        Destroy(gameObject);
    }
}
