using UnityEngine;

[CreateAssetMenu(fileName = "DialogueDataNPC", menuName = "DialogueNPC/Dialogue Data")]
public class DialogueDataNPC : ScriptableObject
{
    public string npcName;
    public string[] lines;
}
