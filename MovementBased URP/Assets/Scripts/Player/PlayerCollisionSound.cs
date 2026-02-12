using UnityEngine;
using System.Collections;

public class PlayerColissionSound : MonoBehaviour
{
    public AudioClip[] soundClips;
    private AudioSource audioSource;
    private bool isPlayingSound = false;
    public float soundPlayInterval = 0.2f;
    public float soundPlayIntervalGround = 1f;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (!isPlayingSound)
            {
                StartCoroutine(PlayRandomSound());
            }
        }
        else if (collision.gameObject.CompareTag("Ground"))
        {
            if (!isPlayingSound)
            {
                StartCoroutine(PlayRandomSoundGround());
            }
        }
    }

    private IEnumerator PlayRandomSound()
    {
        isPlayingSound = true;

        if (soundClips.Length > 0)
        {
            int randomIndex = Random.Range(0, soundClips.Length);
            AudioClip randomClip = soundClips[randomIndex];
            audioSource.PlayOneShot(randomClip);
        }

        yield return new WaitForSeconds(soundPlayInterval);

        isPlayingSound = false;
    }

    private IEnumerator PlayRandomSoundGround()
    {
        isPlayingSound = true;

        if (soundClips.Length > 0)
        {
            int randomIndex = Random.Range(0, soundClips.Length);
            AudioClip randomClip = soundClips[randomIndex];
            audioSource.PlayOneShot(randomClip);
        }

        yield return new WaitForSeconds(soundPlayIntervalGround);

        isPlayingSound = false;
    }
}