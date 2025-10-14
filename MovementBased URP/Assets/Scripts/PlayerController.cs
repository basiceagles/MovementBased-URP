using SmallHedge.SoundManager;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float walkSpeed;
    [SerializeField] public float runSpeed;
    [SerializeField] public float gravity;
    public bool isGrounded;

    [Header("Jump")]
    [SerializeField] private float jumpHeight;
    [SerializeField] private float slideJumpHeight;
    [SerializeField] private float slideJumpGravity;
    public bool isJumping;
    
    [Header("Slide")]
    [SerializeField] private KeyCode slideKey;
    [SerializeField] private float slideSpeed;
    [SerializeField] private float slideAcceleration;
    [SerializeField] private float maxSlideSpeed;
    [SerializeField] private float crouchScale;
    [SerializeField] private float crouchSpeed;
    public float currentSlideSpeed;
    public bool isSliding;
    private bool isCrouching;
    
    [Header("Ground Check")]
    [SerializeField] private float groundCheckDistance;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask slideLayer;

    [SerializeField] private float initialAirVelocity;
    private CharacterController controller;
    public Vector3 velocity;
    private Vector3 originalScale;
    private bool wasGrounded;
    private Vector3 airVelocity;
    
    public bool wallRunning, activeGrappling;
    private bool justWallJumped;
    public bool grounded => isGrounded;
    public Vector3 horizontalVelocity
    {
        get => new Vector3(velocity.x, 0f, velocity.z);
        set { velocity.x = value.x; velocity.z = value.z; }
    }
    public float verticalVelocity
    {
        get => velocity.y;
        set => velocity.y = value;
    }
    public float currentSpeed => new Vector3(velocity.x, 0, velocity.z).magnitude;
    public float maxSpeed => runSpeed;
    
    private void Start()
    {
        controller = GetComponent<CharacterController>();
        originalScale = transform.localScale;
    }
    
    private void Update()
    {
        bool previouslyGrounded = wasGrounded;
        CheckGround();
        
        if (previouslyGrounded && !isGrounded && isSliding && !isJumping && velocity.y < 0)
        {
            velocity.y = initialAirVelocity;
        }
        
        wasGrounded = isGrounded;
        
        if (justWallJumped && isGrounded)
        {
            justWallJumped = false;
        }
        
        HandleMovement();
        HandleJump();
        HandleSlide();
        ApplyMovement();
    }
    
    private void CheckGround()
    {
        isGrounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, groundLayer | slideLayer);
    }
    
    private void HandleMovement()
    {
        if (isSliding || wallRunning || justWallJumped || activeGrappling) return;
        
        if (!isGrounded)
        {
            if (airVelocity.sqrMagnitude > 0.1f)
            {
                velocity.x = airVelocity.x;
                velocity.z = airVelocity.z;
            }
            return;
        }
        
        Vector3 direction = transform.right * Input.GetAxisRaw("Horizontal") + transform.forward * Input.GetAxisRaw("Vertical");
        float speed = isCrouching ? crouchSpeed : (Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed);
        
        velocity.x = direction.x * speed;
        velocity.z = direction.z * speed;
        airVelocity = Vector3.zero;
    }
    
    private void HandleJump()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (isSliding)
            {
                SlideJump();
            }
            else if (isGrounded)
            {
                NormalJump();
            }
        }
    }
    
    private void NormalJump()
    {
        velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
    }
    
    private void SlideJump()
    {
        velocity.y = Mathf.Sqrt(slideJumpHeight * -2f * slideJumpGravity);
        isJumping = true;
    }
    
    private void HandleSlide()
    {
        if (Input.GetKeyDown(slideKey) && isGrounded)
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                isSliding = true;
                currentSlideSpeed = slideSpeed;
                transform.localScale = new Vector3(originalScale.x, originalScale.y * crouchScale, originalScale.z);
            }
            else
            {
                isCrouching = true;
                transform.localScale = new Vector3(originalScale.x, originalScale.y * crouchScale, originalScale.z);
            }
        }
        
        if (Input.GetKeyUp(slideKey))
        {
            if (!isGrounded)
            {
                airVelocity = new Vector3(velocity.x, 0f, velocity.z);
            }
            
            isSliding = false;
            isCrouching = false;
            currentSlideSpeed = 0f;
            transform.localScale = originalScale;
        }
        
        if (isSliding && isGrounded)
        {
            UpdateSlide();
        }
    }
    
    private void UpdateSlide()
    {
        Vector3 origin = transform.position + Vector3.up * 0.1f;
        bool onSlope = Physics.Raycast(origin, Vector3.down, out RaycastHit slopeHit, groundCheckDistance, slideLayer);
        bool onGround = Physics.Raycast(origin, Vector3.down, out RaycastHit groundHit, groundCheckDistance, groundLayer);
        
        if (!onGround && !onSlope)
        {
            return;
        }
        
        if (onSlope)
        {
            HandleSlopeSliding(slopeHit);
        }
        else
        {
            HandleGroundSliding(groundHit);
        }
    }
    
    private void HandleSlopeSliding(RaycastHit hit)
    {
        if (!isJumping)
        {
            velocity.y = gravity;
        }
        
        Vector3 camDir = transform.forward;
        camDir.y = 0;
        Vector3 slopeDir = Vector3.ProjectOnPlane(camDir.normalized, hit.normal).normalized;
        
        currentSlideSpeed = Mathf.Min(currentSlideSpeed + slideAcceleration * Time.deltaTime, maxSlideSpeed);
        velocity.x = slopeDir.x * currentSlideSpeed;
        velocity.z = slopeDir.z * currentSlideSpeed;
    }
    
    private void HandleGroundSliding(RaycastHit hit)
    {
        if (!isJumping)
        {
            velocity.y = gravity;
        }
        
        Vector3 camDir = transform.forward;
        camDir.y = 0;
        camDir.Normalize();
        
        if (currentSlideSpeed > slideSpeed)
        {
            currentSlideSpeed = Mathf.Max(currentSlideSpeed - slideAcceleration * 0.5f * Time.deltaTime, slideSpeed);
        }
        else
        {
            currentSlideSpeed = slideSpeed;
        }
        
        velocity.x = camDir.x * currentSlideSpeed;
        velocity.z = camDir.z * currentSlideSpeed;
    }
    
    private void ApplyMovement()
    {
        if (!wallRunning && !activeGrappling)
        {
            velocity.y += gravity * Time.deltaTime;
        }
        
        if (isGrounded && velocity.y <= 0)
        {
            isJumping = false;            
        }
        
        controller.Move(velocity * Time.deltaTime);
        
    }
    
    public void SetWallRunning(bool value)
    {
        wallRunning = value;
    }
    
    public void SetWallJumped()
    {
        justWallJumped = true;
    }
}
