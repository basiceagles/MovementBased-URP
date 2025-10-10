using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float minSpeed;
    [SerializeField] public float maxSpeed;
    [SerializeField] public float currentSpeed;
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float boostDecay;
    [SerializeField] private float speedDecayRate = 10f;
    public Transform cam;

    [Header("Jump")]
    [SerializeField] public float jumpForce;
    [SerializeField] private float jumpCooldown;
    [SerializeField] private float airMultiplier;
    [SerializeField] private float airControlStrength;
    [SerializeField] private float jumpMomentumMultiplier;
    [SerializeField] private float jumpBoostDuration;
    [SerializeField] public float gravity;

    [Header("Ground Check")]
    [SerializeField] private LayerMask whatIsGround;
    [SerializeField] private LayerMask slopeLayer;
    [SerializeField] private float groundCheckDistance;
    public Transform player;
    public bool grounded;

    [Header("Audio")]
    //[SerializeField] private SoundPlayer soundPlayer;
    [SerializeField] private float minStepInterval = 0.6f;
    [SerializeField] private float maxStepInterval = 0.3f;
    [SerializeField] private float minPitch = 0.8f;
    [SerializeField] private float maxPitch = 1.3f;

    public bool freeze;
    public bool activeGrappling;
    public float verticalVelocity;
    public Vector3 horizontalVelocity;
    public bool wallRunning;
    public bool hasJumpedFromGround;

    private CharacterController controller;
    private Vector3 moveDirection;
    private bool sliding;
    private bool readyToJump;
    private bool hasDoubleJump;
    private float jumpMomentumTimer;
    private float targetJumpVelocity;
    private float slideMomentumTimer;
    private float targetSlideVelocity;
    private float stepTimer; 

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        readyToJump = true;
        //if (soundPlayer == null)
            //soundPlayer = FindObjectOfType<SoundPlayer>();
    }

    private void Update()
    {
        GroundCheck();
        
        if (freeze)
        {
            horizontalVelocity = Vector3.zero;
            verticalVelocity = 0f;
            return;
        }

        if (Input.GetKeyDown(KeyCode.Space))
            HandleJumpInput();

        if (!wallRunning && !sliding)
            MovePlayer();
    }

    private void GroundCheck()
    {
        if (controller == null || player == null) return;
        
        LayerMask allLayers = whatIsGround | slopeLayer;
        Vector3 pos = player.position;
        float radius = controller.radius * 0.9f;
        
        grounded = Physics.Raycast(pos, Vector3.down, groundCheckDistance, allLayers) ||
                   Physics.Raycast(pos + transform.right * radius, Vector3.down, groundCheckDistance, allLayers) ||
                   Physics.Raycast(pos - transform.right * radius, Vector3.down, groundCheckDistance, allLayers) ||
                   Physics.Raycast(pos + transform.forward * radius, Vector3.down, groundCheckDistance, allLayers) ||
                   Physics.Raycast(pos - transform.forward * radius, Vector3.down, groundCheckDistance, allLayers) ||
                   CheckWallTop(pos);
    }
    
    private bool CheckWallTop(Vector3 pos)
    {
        return Physics.Raycast(pos, Vector3.down, out RaycastHit hit, groundCheckDistance) && hit.collider.CompareTag("Wall") && Vector3.Dot(hit.normal, Vector3.up) > 0.7f;
    }


    private void HandleJumpInput()
    {
        if (sliding) return;

        if (readyToJump && grounded)
        {
            readyToJump = false;
            hasJumpedFromGround = true;
            hasDoubleJump = true;
            Jump();
            Invoke(nameof(ResetJump), jumpCooldown);
        }
        else if (!grounded && hasDoubleJump && !wallRunning)
        {
            hasDoubleJump = false;
            Jump();
        }
        else if (!grounded && !hasJumpedFromGround && readyToJump && !wallRunning)
        {
            hasJumpedFromGround = true;
            hasDoubleJump = true;
            Jump();
            Invoke(nameof(ResetJump), jumpCooldown);
        }
    }

    private void MovePlayer()
    {
        float vertical = Input.GetAxisRaw("Vertical");
        float horizontal = Input.GetAxisRaw("Horizontal");
        
        moveDirection = transform.forward * vertical + transform.right * horizontal;
        bool moving = moveDirection.magnitude > 0.1f;
        bool movingForward = vertical > 0.1f;

        if (grounded)
            HandleGroundMovement(moving, movingForward);
        else
            HandleAirMovement(moving);
            
        ApplyGravity();
        controller.Move((horizontalVelocity + Vector3.up * verticalVelocity) * Time.deltaTime);
    }

    private void HandleGroundMovement(bool moving, bool movingForward)
    {
        if (currentSpeed > maxSpeed)
            currentSpeed = Mathf.MoveTowards(currentSpeed, maxSpeed, boostDecay * Time.deltaTime);
        
        if (movingForward)
            currentSpeed = Mathf.Max(minSpeed, Mathf.MoveTowards(currentSpeed, maxSpeed, acceleration * Time.deltaTime));
        else if (moving)
            currentSpeed = minSpeed;
        else
            currentSpeed = 0f;
        
        if (verticalVelocity <= 0)
            horizontalVelocity = moving ? moveDirection.normalized * currentSpeed : Vector3.zero;
            
        if (verticalVelocity < 0 && controller.isGrounded && readyToJump)
        {
            hasJumpedFromGround = false;
            hasDoubleJump = false;
        }
        
        HandleFootsteps(moving);
    }

    private void HandleFootsteps(bool moving)
    {
        //if (!moving || soundPlayer == null || maxSpeed <= 0)
        //{
        //    stepTimer = 0f;
        //    return;
        //}
        
        //stepTimer += Time.deltaTime;
        //float speedRatio = Mathf.Clamp01(currentSpeed / maxSpeed);
        //float interval = Mathf.Lerp(minStepInterval, maxStepInterval, speedRatio);
        
        //if (stepTimer >= interval)
        //{
            //soundPlayer.PlaySoundWithPitch(0, Mathf.Lerp(minPitch, maxPitch, speedRatio));
            //stepTimer = 0f;
        //}
    }

    private void HandleAirMovement(bool moving)
    {
        ApplyMomentum(ref jumpMomentumTimer, targetJumpVelocity);
        ApplyMomentum(ref slideMomentumTimer, targetSlideVelocity);
        
        if (moving)
        {
            float minTarget = horizontalVelocity.magnitude * 0.95f;
            float airTarget = Mathf.Min(currentSpeed * airMultiplier, maxSpeed * 0.8f);
            Vector3 targetVelocity = moveDirection.normalized * Mathf.Max(minTarget, airTarget);
            horizontalVelocity = Vector3.Lerp(horizontalVelocity, targetVelocity, airControlStrength * Time.deltaTime);
        }
        else
        {
            horizontalVelocity = Vector3.MoveTowards(horizontalVelocity, Vector3.zero, speedDecayRate * Time.deltaTime * 0.5f);
        }
    }
    
    private void ApplyMomentum(ref float timer, float targetVelocity)
    {
        if (timer <= 0) return;
        
        timer -= Time.deltaTime;
        float progress = 1f - (timer / jumpBoostDuration);
        float currentTarget = Mathf.Lerp(horizontalVelocity.magnitude, targetVelocity, progress);
        
        if (horizontalVelocity.magnitude > 0.1f)
        {
            horizontalVelocity = horizontalVelocity.normalized * currentTarget;
        }
    }

    private void ApplyGravity()
    {
        if (!activeGrappling && !wallRunning && !grounded)
        {
            verticalVelocity += gravity * Time.deltaTime;
        }
    }

    public void Jump()
    {
        verticalVelocity = jumpForce;
        
        if (horizontalVelocity.magnitude > 0.1f && currentSpeed > 0)
        {
            float boost = 1f + (currentSpeed / maxSpeed * jumpMomentumMultiplier * 0.7f);
            targetJumpVelocity = horizontalVelocity.magnitude * boost;
            jumpMomentumTimer = jumpBoostDuration;
        }
        else
        {
            targetJumpVelocity = jumpMomentumTimer = 0f;
        }
    }

    private void ResetJump() => readyToJump = true;
    
    public void ApplySlideMomentum(float slideSpeed)
    {
        if (horizontalVelocity.magnitude > 0.1f && slideSpeed > 0)
        {
            float speedRatio = slideSpeed / maxSpeed;
            float boost = 1f + (speedRatio * jumpMomentumMultiplier * 0.3f);
            float targetSpeed = horizontalVelocity.magnitude * boost;
            targetSpeed = Mathf.Min(targetSpeed, maxSpeed * 1.1f);
            targetSlideVelocity = targetSpeed;
            slideMomentumTimer = jumpBoostDuration;

            if (verticalVelocity < -2f)
            {
                verticalVelocity = -2f;
            }
        }
        else
        {
            targetSlideVelocity = 0f;
            slideMomentumTimer = 0f;
        }
    }

    public void SetSliding(bool isSliding)
    {
        sliding = isSliding;
    }

    public void SetWallRunning(bool isWallRunning)
    {
        wallRunning = isWallRunning;
    }
    public void DisableDoubleJump()
    {
        hasDoubleJump = false;
    }
    public void EnableDoubleJump()
    {
        hasDoubleJump = true;
    }
    public float GetCurrentSpeed()
    {
        return currentSpeed;
    }
    public void ApplySpeedBoost(float speed)
    {
        if (speed > currentSpeed)
        {
            currentSpeed = speed;
        }
    }
    
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        float dotUp = Vector3.Dot(hit.normal, Vector3.up);
        float dotDown = Vector3.Dot(hit.normal, Vector3.down);
        
        if (dotUp < 0.7f && dotDown < 0.7f && hit.collider.CompareTag("Untagged") && hit.collider.gameObject.layer == 0)
        {
            currentSpeed = minSpeed;
            horizontalVelocity = Vector3.zero;
        }
        
        if (dotDown > 0.7f && verticalVelocity > 0 && !hit.collider.CompareTag("Wall"))
        {
            verticalVelocity = 0f;
        }
    }
}