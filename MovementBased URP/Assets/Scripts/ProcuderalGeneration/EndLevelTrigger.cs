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
    public GameObject targetPerkSelectorManager;
    public GameObject targetPerkSelector;
    public PerkSelectorScript perkSelector;
    public Animator perkSelectorAnimator;

    void Start()
    {
        slideMetalDoorManager = targetDoor.GetComponent<SlideMetalDoorManager>();
        targetProcuderalGeneration = GameObject.Find("ProcuderalGeneration");
        levelGenerator = targetProcuderalGeneration.GetComponent<LevelGenerator>();

        targetMusicManager = GameObject.Find("MusicManager");
        musicManagerScript = targetMusicManager.GetComponent<MusicManager>();

        targetPerkSelectorManager = GameObject.Find("PerkSelectorManager");
        perkSelector = targetPerkSelectorManager.GetComponent<PerkSelectorScript>();
        targetPerkSelector = GameObject.Find("PerkSelector");
        perkSelectorAnimator = targetPerkSelector.GetComponent<Animator>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.gameObject.CompareTag(tagFilter)) return;

        perkSelector.OpenPerkMenu();
        perkSelectorAnimator.Play("PerkSelectorMenu");
        
        slideMetalDoorManager.CloseDoor();
        levelGenerator.RestartGeneration();
        musicManagerScript.StopPlayingMusic();

        Destroy(gameObject);
    }
}
