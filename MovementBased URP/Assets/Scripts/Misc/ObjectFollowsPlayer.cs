using UnityEngine;

public class ObjectFollowsPlayer : MonoBehaviour
{
    [Header("Target Settings")]
    [Tooltip("The GameObject to follow")]
    public Transform targetToFollow;

    [Header("Axis Settings")]
    [Tooltip("Follow on the X axis")]
    public bool followX = true;

    [Tooltip("Follow on the Y axis")]
    public bool followY = true;

    [Tooltip("Follow on the Z axis")]
    public bool followZ = true;

    [Header("Offset Settings")]
    [Tooltip("Optional offset from the target position")]
    public Vector3 positionOffset = Vector3.zero;

    void Update()
    {
        FollowTarget();
    }

    void FollowTarget()
    {
        // Check if target is assigned
        if (targetToFollow == null)
        {
            // Optional: You can add a warning here if needed
            // Debug.LogWarning("Target to follow is not assigned!");
            return;
        }

        // Get the current position of this GameObject
        Vector3 currentPosition = transform.position;
        
        // Get the target position with offset
        Vector3 targetPosition = targetToFollow.position + positionOffset;

        // Apply following only on selected axes
        if (followX) currentPosition.x = targetPosition.x;
        if (followY) currentPosition.y = targetPosition.y;
        if (followZ) currentPosition.z = targetPosition.z;

        // Update position
        transform.position = currentPosition;
    }

    // Optional: Add this method to draw gizmos in the Scene view for debugging
    void OnDrawGizmosSelected()
    {
        if (targetToFollow != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, targetToFollow.position);
            
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(targetToFollow.position + positionOffset, 0.5f);
        }
    }
}