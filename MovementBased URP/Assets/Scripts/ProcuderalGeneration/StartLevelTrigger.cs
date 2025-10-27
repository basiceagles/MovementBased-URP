using UnityEngine;

public class StartLevelTrigger : MonoBehaviour
{
    [SerializeField] string tagFilter = "Player";
    private GameObject targetMusicManager;
    private MusicManager musicManagerScript;
    void Start()
    {
        targetMusicManager = GameObject.Find("MusicManager");
        musicManagerScript = targetMusicManager.GetComponent<MusicManager>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.gameObject.CompareTag(tagFilter)) return;

        musicManagerScript.PlayMusic();
        Destroy(gameObject);
    }
}
