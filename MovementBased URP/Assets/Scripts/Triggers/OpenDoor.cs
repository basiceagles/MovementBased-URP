using UnityEngine;

public class OpenDoor : MonoBehaviour
{
    [SerializeField] private SlideMetalDoorManager slideMetalDoorManager;

    public void CallOpenDoor()
    {
        slideMetalDoorManager.OpenSlideDoor();
    }
}
