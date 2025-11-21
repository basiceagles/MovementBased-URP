using UnityEngine;
using UnityEngine.VFX;

public class VGFloorRaycatser : MonoBehaviour
{
    [SerializeField] private VisualEffect vfx;
    [SerializeField] private LayerMask floorLayer;
    [SerializeField] private float raycastDistance = 10f;
    [SerializeField] private string vfxColliderPositionName = "ColliderPosition";
    
    private RaycastHit raycastHit;

    void Update()
    {
        RaycastFloor();
    }

    private void RaycastFloor()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out raycastHit, raycastDistance, floorLayer))
        {
            Vector3 relativeHitPoint = raycastHit.point - transform.position;
            vfx.SetVector3(vfxColliderPositionName, relativeHitPoint);
        }
    }
}