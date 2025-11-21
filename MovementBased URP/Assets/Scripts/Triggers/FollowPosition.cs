using UnityEngine;

public class FollowPosition : MonoBehaviour
{
    public GameObject targetObject;  // Drag your target GameObject here
    void Start()
    {
        targetObject = GameObject.Find("RunnerParent");
    }

    void Update()
    {
        if (targetObject != null)
        {
            transform.position = targetObject.transform.position;
        }
    }
}
