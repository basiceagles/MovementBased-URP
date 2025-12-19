using UnityEngine;
using SmallHedge.SoundManager;

public class MusicManagerPlayerByName : MonoBehaviour
{
    [SerializeField] private string musicName;
    public void PlasySound()
    {
        if (System.Enum.TryParse<SoundType>(musicName, out SoundType soundType))
        {
            SoundManager.PlaySound(soundType);
        }
    }
}
