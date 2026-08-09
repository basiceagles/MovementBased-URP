using UnityEngine;

public class CallCameraShake : MonoBehaviour
{
    private CameraShake cameraShake;

    [Header("Constant Shake Settings")]
    public bool onStart;
    public bool onEnd;

    private void Start()
    {
        if (Camera.main != null)
        {
            cameraShake = Camera.main.GetComponent<CameraShake>();
        }
        if (cameraShake == null)
        {
            cameraShake = FindObjectOfType<CameraShake>();
        }
    }

    public void CallCameraShaker()
    {
        if (cameraShake != null) cameraShake.Shake();
    }

    public void CallHeavyCameraShaker()
    {
        if (cameraShake != null) cameraShake.HeavyShake();
    }

    public void CallCameraShakerMethod()
    {
        if (cameraShake == null) return;

        if (onStart)
        {
            cameraShake.ConstantlyShakeStart();
        }

        if (onEnd)
        {
            cameraShake.ConstantlyShakeStop();
        }
    }
}

