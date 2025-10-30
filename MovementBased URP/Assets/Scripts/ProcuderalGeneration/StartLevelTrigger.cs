using UnityEngine;

public class StartLevelTrigger : MonoBehaviour
{
    [SerializeField] string tagFilter = "Player";
    private GameObject targetMusicManager;
    private MusicManager musicManagerScript;
    private GameObject targetTimerManager;
    private Timer targetTimer;
    void Start()
    {
        targetMusicManager = GameObject.Find("MusicManager");
        musicManagerScript = targetMusicManager.GetComponent<MusicManager>();
        targetTimerManager = GameObject.Find("TimerManager");
        targetTimer = targetTimerManager.GetComponent<Timer>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.gameObject.CompareTag(tagFilter)) return;

        musicManagerScript.PlayMusic();
        targetTimer.StartCountdown();
        Destroy(gameObject);
    }
}
