using UnityEngine;
using System.Collections;

public class PlayerColissionSound : MonoBehaviour
{
    // Array to hold the audio clips
    public AudioClip[] soundClips;
    
    // Reference to the AudioSource component
    private AudioSource audioSource;

    // Flag to check if a sound is currently playing
    private bool isPlayingSound = false;

    // Time interval between sounds
    public float soundPlayInterval = 0.2f;
    public float soundPlayIntervalGround = 1f;

    void Awake()
    {
        // Get the AudioSource component attached to this GameObject
        audioSource = GetComponent<AudioSource>();
    }

    void OnCollisionEnter(Collision collision)
    {
        // Check if the collided object has the tag "Player"
        if (collision.gameObject.CompareTag("Player"))
        {
            // If a sound is not currently playing, play a random sound
            if (!isPlayingSound)
            {
                StartCoroutine(PlayRandomSound());
            }
        }
        else if (collision.gameObject.CompareTag("Ground"))
        {
            // If a sound is not currently playing, play a random sound
            if (!isPlayingSound)
            {
                StartCoroutine(PlayRandomSoundGround());
            }
        }
    }

    private IEnumerator PlayRandomSound()
    {
        isPlayingSound = true;

        // Check if there are any sound clips available
        if (soundClips.Length > 0)
        {
            // Select a random sound clip from the array
            int randomIndex = Random.Range(0, soundClips.Length);
            AudioClip randomClip = soundClips[randomIndex];

            // Play the selected sound clip
            audioSource.PlayOneShot(randomClip);
        }

        // Wait for the specified interval before allowing another sound to play
        yield return new WaitForSeconds(soundPlayInterval);

        isPlayingSound = false;
    }
    private IEnumerator PlayRandomSoundGround()
    {
        isPlayingSound = true;

        // Check if there are any sound clips available
        if (soundClips.Length > 0)
        {
            // Select a random sound clip from the array
            int randomIndex = Random.Range(0, soundClips.Length);
            AudioClip randomClip = soundClips[randomIndex];

            // Play the selected sound clip
            audioSource.PlayOneShot(randomClip);
        }

        // Wait for the specified interval before allowing another sound to play
        yield return new WaitForSeconds(soundPlayIntervalGround);

        isPlayingSound = false;
    }
}