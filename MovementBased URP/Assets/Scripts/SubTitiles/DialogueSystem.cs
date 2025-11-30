using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;

public class RandomDialoguePlayer : MonoBehaviour
{
    [System.Serializable]
    public class DialogueEntry
    {
        public DialogueData dialogueData;
        public AudioClip dialogueAudioClip; // Assigned per entry in inspector
        [HideInInspector] public bool hasBeenPlayedOnThisObject; // Tracks play-once per RandomDialoguePlayer instance
        [HideInInspector] public bool hasBeenPlayedInCycle; // Tracks if played in the current shuffle cycle
    }

    [Header("Dialogue Settings")]
    [SerializeField] private DialogueEntry[] dialogueEntries;

    [SerializeField] private bool playOnStart = false;
    [SerializeField] private bool playOnce = false;
    [SerializeField] private bool useGlobalDialogueTracking = false;

    [Header("UI References")]
    [SerializeField] private DialogueUIReferences dialogueSystemReferences;
    [SerializeField] private UnityEngine.UI.Image speakerImageUI;
    [SerializeField] private GameObject dialoguePanel;

    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private bool stopAudioOnDialogueEnd = true;

    private bool isPlayingDialogue = false;
    private Coroutine currentDialogueCoroutine;

    private static HashSet<DialogueData> globallyPlayedDialogues = new HashSet<DialogueData>();
    private static List<RandomDialoguePlayer> instances = new List<RandomDialoguePlayer>();

    private bool hasCompletedCycle = false;

    private void Awake()
    {
        if (!instances.Contains(this))
        {
            instances.Add(this);
        }
    }

    private void OnDestroy()
    {
        if (instances.Contains(this))
        {
            instances.Remove(this);
        }
    }

    void Start()
    {
        UpdateReferences();

        // Get AudioSource reference if not assigned
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                Debug.LogWarning("No AudioSource found! Please assign one in the inspector.");
            }
        }
        
        if (playOnStart)
        {
            StartRandomDialogue();
        }
    }

    private void UpdateReferences()
    {
        if (dialogueSystemReferences == null)
        {
            GameObject subtitleObj = GameObject.Find("SubtitleTextUI");
            if (subtitleObj != null)
            {
                dialogueSystemReferences = subtitleObj.GetComponent<DialogueUIReferences>();
            }
        }
    }

    public void StartRandomDialogue()
    {
        if (dialogueEntries == null || dialogueEntries.Length == 0)
        {
            Debug.LogWarning("No dialogue data assigned!");
            return;
        }

        // If global tracking is enabled on this object, check whether there is at least one
        // dialogue entry whose DialogueData has not been played globally yet. If all of this
        // object's entries have already been played globally, do not start a new dialogue.
        if (useGlobalDialogueTracking)
        {
            bool hasUnplayedGlobal = false;
            foreach (var entry in dialogueEntries)
            {
                if (entry != null && entry.dialogueData != null && !globallyPlayedDialogues.Contains(entry.dialogueData))
                {
                    hasUnplayedGlobal = true;
                    break;
                }
            }

            if (!hasUnplayedGlobal)
            {
                return;
            }
        }

        if (isPlayingDialogue)
        {
            StopCoroutine(currentDialogueCoroutine);
            isPlayingDialogue = false;
        }

        // Select random dialogue entry
        DialogueEntry randomEntry = null;

        if (playOnce)
        {
            // Build a list of entries that have not been played yet on THIS component instance
            System.Collections.Generic.List<DialogueEntry> availableEntries = new System.Collections.Generic.List<DialogueEntry>();
            foreach (var entry in dialogueEntries)
            {
                if (entry != null && entry.dialogueData != null && !entry.hasBeenPlayedOnThisObject)
                {
                    availableEntries.Add(entry);
                }
            }

            if (availableEntries.Count == 0)
            {
                // Nothing left to play once for this object
                return;
            }

            int randomIndex = Random.Range(0, availableEntries.Count);
            randomEntry = availableEntries[randomIndex];
        }
        else
        {
            // Shuffle-like behavior without repeats until all entries in this object have been played once
            System.Collections.Generic.List<DialogueEntry> availableEntries = new System.Collections.Generic.List<DialogueEntry>();
            foreach (var entry in dialogueEntries)
            {
                if (entry != null && entry.dialogueData != null && !entry.hasBeenPlayedInCycle)
                {
                    // If global tracking is enabled, also skip entries that have already been played globally
                    if (!useGlobalDialogueTracking || !globallyPlayedDialogues.Contains(entry.dialogueData))
                    {
                        availableEntries.Add(entry);
                    }
                }
            }

            // If everything was already played in this cycle, reset and rebuild the list
            if (availableEntries.Count == 0)
            {
                // Mark this instance as having completed its local cycle
                hasCompletedCycle = true;

                // If global tracking is enabled, only clear the global record and reset cycles
                // once ALL instances that use global tracking have completed their local cycles.
                if (useGlobalDialogueTracking)
                {
                    bool allCompleted = true;
                    foreach (var player in instances)
                    {
                        if (player != null && player.useGlobalDialogueTracking &&
                            player.dialogueEntries != null && player.dialogueEntries.Length > 0)
                        {
                            if (!player.hasCompletedCycle)
                            {
                                allCompleted = false;
                                break;
                            }
                        }
                    }

                    if (allCompleted)
                    {
                        // Clear global record and reset cycle state for all tracked players
                        globallyPlayedDialogues.Clear();

                        foreach (var player in instances)
                        {
                            if (player != null && player.useGlobalDialogueTracking && player.dialogueEntries != null)
                            {
                                player.hasCompletedCycle = false;
                                foreach (var e in player.dialogueEntries)
                                {
                                    if (e != null)
                                    {
                                        e.hasBeenPlayedInCycle = false;
                                    }
                                }
                            }
                        }
                    }
                }

                // Always reset this instance's local cycle so it can start a new shuffle
                foreach (var entry in dialogueEntries)
                {
                    if (entry != null)
                    {
                        entry.hasBeenPlayedInCycle = false;
                    }
                }

                foreach (var entry in dialogueEntries)
                {
                    if (entry != null && entry.dialogueData != null && !entry.hasBeenPlayedInCycle)
                    {
                        availableEntries.Add(entry);
                    }
                }
            }

            if (availableEntries.Count == 0)
            {
                // Still nothing available (e.g. all entries null), bail out safely
                isPlayingDialogue = false;
                return;
            }

            int randomIndex = Random.Range(0, availableEntries.Count);
            randomEntry = availableEntries[randomIndex];
            randomEntry.hasBeenPlayedInCycle = true;
        }

        if (randomEntry == null || randomEntry.dialogueData == null)
        {
            Debug.LogWarning("Selected dialogue entry is null or has no DialogueData.");
            isPlayingDialogue = false;
            return;
        }

        // Mark as played on this object if playOnce is enabled
        if (playOnce && randomEntry != null)
        {
            randomEntry.hasBeenPlayedOnThisObject = true;
        }

        // Mark as played globally if global tracking is enabled and we're using the shuffle path
        if (!playOnce && useGlobalDialogueTracking && randomEntry != null && randomEntry.dialogueData != null)
        {
            globallyPlayedDialogues.Add(randomEntry.dialogueData);
        }

        // Ensure subtitle UI is visible when starting a dialogue
        if (dialogueSystemReferences != null)
        {
            if (dialogueSystemReferences.dialogueText != null)
            {
                dialogueSystemReferences.dialogueText.text = string.Empty;
            }
            dialogueSystemReferences.ShowDialogueUI();
        }

        currentDialogueCoroutine = StartCoroutine(PlayDialogueCoroutine(randomEntry.dialogueData, randomEntry.dialogueAudioClip));
    }

    private IEnumerator PlayDialogueCoroutine(DialogueData dialogueData, AudioClip entryClip)
    {
        isPlayingDialogue = true;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

        // Play entry-level audio clip if provided
        if (audioSource != null && entryClip != null)
        {
            audioSource.clip = entryClip;
            audioSource.Play();
        }

        foreach (var line in dialogueData.dialogueLines)
        {
            // Set speaker image if available
            if (speakerImageUI != null && line.speakerIcon != null)
            {
                speakerImageUI.sprite = line.speakerIcon;
                speakerImageUI.gameObject.SetActive(true);
            }
            else if (speakerImageUI != null)
            {
                speakerImageUI.gameObject.SetActive(false);
            }

            // Get localized text from Unity Localization package
            string localizedText = line.localizedText != null 
                ? line.localizedText.GetLocalizedString() 
                : string.Empty;

            // Display text via DialogueSystemReferences
            if (dialogueSystemReferences != null && dialogueSystemReferences.dialogueText != null)
            {
                dialogueSystemReferences.dialogueText.text = localizedText;
            }

            // Wait strictly based on delay from DialogueData
            float waitTime = Mathf.Max(0f, line.delayBeforeNextLine);
            yield return new WaitForSeconds(waitTime);
        }

        // Dialogue sequence finished; clear subtitle text and hide subtitle UI via DialogueSystemReferences.
        // Audio is not stopped automatically so it can continue playing.
        if (dialogueSystemReferences != null && dialogueSystemReferences.dialogueText != null)
        {
            dialogueSystemReferences.dialogueText.text = string.Empty;
            dialogueSystemReferences.HideDialogueUI();
        }

        isPlayingDialogue = false;
    }

    // Localization is handled per-line via DialogueLine.localizedText.

    // Method to manually trigger dialogue
    public void PlaySpecificDialogue(DialogueData dialogueData)
    {
        if (isPlayingDialogue)
        {
            StopCoroutine(currentDialogueCoroutine);
            isPlayingDialogue = false;
        }

        // When playing a specific dialogue directly, no per-entry clip is provided here
        currentDialogueCoroutine = StartCoroutine(PlayDialogueCoroutine(dialogueData, null));
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (!isPlayingDialogue)
            {
                StartRandomDialogue();
            }

            if (dialoguePanel != null)
            {
                dialoguePanel.SetActive(true);
            }

            // Ensure subtitle UI is visible when the player is inside the trigger
            if (dialogueSystemReferences != null)
            {
                dialogueSystemReferences.ShowDialogueUI();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (dialoguePanel != null)
            {
                dialoguePanel.SetActive(false);
            }

            // Hide subtitle UI and clear text when the player leaves the trigger
            if (dialogueSystemReferences != null && dialogueSystemReferences.dialogueText != null)
            {
                dialogueSystemReferences.dialogueText.text = string.Empty;
                dialogueSystemReferences.HideDialogueUI();
            }
        }
    }

    // Method to stop current dialogue
    public void StopCurrentDialogue()
    {
        if (isPlayingDialogue)
        {
            StopCoroutine(currentDialogueCoroutine);
            isPlayingDialogue = false;

            // Stop audio when dialogue is stopped
            if (audioSource != null && audioSource.isPlaying)
            {
                audioSource.Stop();
            }

            if (dialoguePanel != null)
                dialoguePanel.SetActive(false);
        }
    }

    // Method to set AudioSource reference programmatically
    public void SetAudioSource(AudioSource newAudioSource)
    {
        audioSource = newAudioSource;
    }

    // Method to play a specific audio clip (useful for testing or external triggers)
    public void PlayAudioClip(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.clip = clip;
            audioSource.Play();
        }
    }
}