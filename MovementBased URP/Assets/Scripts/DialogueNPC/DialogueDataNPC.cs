using UnityEngine;
using UnityEngine.Localization;

[CreateAssetMenu(fileName = "DialogueDataNPC", menuName = "DialogueNPC/Dialogue Data")]
public class DialogueDataNPC : ScriptableObject
{
    [Header("NPC Identification")]
    [Tooltip("Unique identifier for this NPC (used for dialogue lookup)")]
    public string npcId;

    [Header("Localization")]
    [SerializeField] private LocalizedString _npcName = new LocalizedString();
    [SerializeField] private LocalizedString[] _lines = new LocalizedString[0];
    
    [Header("Audio")]
    public AudioClip[] voiceClips;
    public float[] audioClipPlayDurations;

    // Public properties with null checks
    public LocalizedString npcName 
    { 
        get 
        { 
            if (_npcName == null) 
                _npcName = new LocalizedString();
            return _npcName; 
        } 
        set { _npcName = value; } 
    }

    public LocalizedString[] lines 
    { 
        get 
        { 
            if (_lines == null) 
                _lines = new LocalizedString[0];
            return _lines; 
        } 
        set { _lines = value; } 
    }
}
