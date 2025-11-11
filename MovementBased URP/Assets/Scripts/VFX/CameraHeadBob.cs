using UnityEngine;

public class CameraHeadBob : MonoBehaviour
{
    [SerializeField] private float walkBobSpeed = 12f;
    [SerializeField] private float walkBobAmount = 0.05f;
    [SerializeField] private float walkBobAmountX = 0.03f;
    [SerializeField] private float walkBobAmountY = 0.02f;
    [SerializeField] private float runBobSpeed = 20f;
    [SerializeField] private float runBobAmount = 0.12f;
    [SerializeField] private float runBobAmountX = 0.06f;
    [SerializeField] private float runBobAmountY = 0.04f;
    [SerializeField] private PlayerController playerController;
    
    private float defaultYPos;
    private float defaultXPos;
    private float timer;

    private void Start()
    {
        defaultYPos = transform.localPosition.y;
        defaultXPos = transform.localPosition.x;
        
        if (playerController == null)
        {
            playerController = FindObjectOfType<PlayerController>();
        }
    }

    private void Update()
    {
        if (playerController == null) return;

        float speed = playerController.currentSpeed;
        bool isMoving = speed > 0.1f && playerController.grounded;

        if (isMoving)
        {
            bool isRunning = speed > playerController.maxSpeed * 0.7f;
            float bobSpeed = isRunning ? runBobSpeed : walkBobSpeed;
            float bobAmount = isRunning ? runBobAmount : walkBobAmount;
            float bobAmountX = isRunning ? runBobAmountX : walkBobAmountX;
            float bobAmountY = isRunning ? runBobAmountY : walkBobAmountY;

            timer += Time.deltaTime * bobSpeed;
            float newY = defaultYPos + Mathf.Sin(timer) * bobAmount + Mathf.Cos(timer * 0.8f) * bobAmountY;
            float newX = defaultXPos + Mathf.Cos(timer * 0.5f) * bobAmountX;
            
            transform.localPosition = new Vector3(Mathf.Lerp(transform.localPosition.x, newX, Time.deltaTime * 10f), Mathf.Lerp(transform.localPosition.y, newY, Time.deltaTime * 10f), transform.localPosition.z);
        }
        else
        {
            timer = 0f;
            transform.localPosition = new Vector3(Mathf.Lerp(transform.localPosition.x, defaultXPos, Time.deltaTime * 10f), Mathf.Lerp(transform.localPosition.y, defaultYPos, Time.deltaTime * 10f), transform.localPosition.z);
        }
    }
}