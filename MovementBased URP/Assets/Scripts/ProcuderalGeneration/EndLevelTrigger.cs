using UnityEngine;
using TMPro;

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
    private GameObject targetTimerManager;
    private Timer targetTimer;
    private GameObject cameraTarget;
    private RedEyeDetector redEyeDetector;
    private GameObject cycleIntTextTarget;
    private TMP_Text cycleIntText;
    private int currentCycle = 0;

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

        targetTimerManager = GameObject.Find("TimerManager");
        targetTimer = targetTimerManager.GetComponent<Timer>();

        cameraTarget = GameObject.Find("Main Camera");
        redEyeDetector = cameraTarget.GetComponent<RedEyeDetector>();

        cycleIntTextTarget = GameObject.Find("CycleIntTextUI");
        cycleIntText = cycleIntTextTarget.GetComponent<TMP_Text>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.gameObject.CompareTag(tagFilter)) return;

        perkSelector.OpenPerkMenu();
        perkSelectorAnimator.Play("PerkSelectorMenu");
        
        slideMetalDoorManager.CloseDoor();

        //ПЕРЕДЕЛАТЬ
        cycleIntText.text = currentCycle.ToString();
        
        musicManagerScript.StopPlayingMusic();

        targetTimer.StopAndReset();

        Destroy(gameObject);
    }
}
