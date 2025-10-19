using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float walkSpeed;
    [SerializeField] public float runSpeed;
    [SerializeField] public float gravity;
    [SerializeField] private float airControlStrength = 2f;
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
    private bool wantsToStandUp;
    
    [Header("Ground Check")]
    [SerializeField] private float groundCheckDistance;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask slideLayer;
    [SerializeField] private LayerMask rampLayer;

    [SerializeField] private float initialAirVelocity;
    private CharacterController controller;
    public Vector3 velocity;
    private Vector3 originalScale;
    private bool wasGrounded;
    private bool wasOnRamp;
    private Vector3 airVelocity;

    //Для нового инпут контроля
    public InputActionAsset inputActions;
    private InputAction m_Jump;
    private InputAction m_WalkController;
    private InputAction m_Crouch;
    private InputAction m_Run;
    private InputAction m_Swing;
    private InputAction m_Rope;
    
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

        //Для нового инпут контроля
        m_Jump = InputSystem.actions.FindAction("Jump");
        m_WalkController = InputSystem.actions.FindAction("Move");
        m_Crouch = InputSystem.actions.FindAction("Crouch");
        m_Run = InputSystem.actions.FindAction("Sprint");
    }
    
    private void Update()
    {
        bool previouslyGrounded = wasGrounded;
        bool previouslyOnRamp = wasOnRamp;
        CheckGround();
        
        if (previouslyOnRamp && !isGrounded && isSliding && !isJumping && velocity.y < 0)
        {
            velocity.y = initialAirVelocity;
        }
        else if (previouslyGrounded && !previouslyOnRamp && !isGrounded && isSliding && !isJumping && velocity.y < 0)
        {
            velocity.y = 0f; 
        }
        else if (previouslyGrounded && !isGrounded && !isJumping && !isSliding && velocity.y < 0)
        {
            velocity.y = 0f; 
        }
        
        wasGrounded = isGrounded;
        wasOnRamp = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, rampLayer);
        
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
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, groundCheckDistance))
        {
            // Проверяем слои или верхнюю плоскость стены
            bool isOnLayer = ((1 << hit.collider.gameObject.layer) & (groundLayer | slideLayer | rampLayer)) != 0;
            bool isOnWallTop = hit.collider.CompareTag("Wall") && Vector3.Dot(hit.normal, Vector3.up) > 0.5f;
            isGrounded = isOnLayer || isOnWallTop;
        }
        else
        {
            isGrounded = false;
        }
    }
    
    private void HandleMovement()
    {
        if (isSliding || wallRunning || justWallJumped || activeGrappling) return;
        
        if (!isGrounded)
        { 
            Vector3 inputDir = transform.right * Input.GetAxisRaw("Horizontal") + transform.forward * Input.GetAxisRaw("Vertical");
            if (inputDir.sqrMagnitude > 0.1f)
            {
                Vector3 currentHorizontal = new Vector3(velocity.x, 0, velocity.z);
                float currentMagnitude = currentHorizontal.magnitude;
                Vector3 newDir = (currentHorizontal + inputDir * airControlStrength * Time.deltaTime).normalized;
                velocity.x = newDir.x * currentMagnitude;
                velocity.z = newDir.z * currentMagnitude;
            }
            else if (airVelocity.sqrMagnitude > 0.1f)
            {
                velocity.x = airVelocity.x;
                velocity.z = airVelocity.z;
            }
            return;
        }
        
        Vector3 direction = transform.right * Input.GetAxisRaw("Horizontal") + transform.forward * Input.GetAxisRaw("Vertical");
        float speed = isCrouching ? crouchSpeed : (m_Run.IsPressed() ? runSpeed : walkSpeed);
        
        velocity.x = direction.x * speed;
        velocity.z = direction.z * speed;
        airVelocity = Vector3.zero;
    }
    
    private void HandleJump()
    {
        if (m_Jump.WasPressedThisFrame())
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
        if (m_Crouch.WasPressedThisFrame() && isGrounded)
        {
            if (m_Run.IsPressed())
            {
                float forwardInput = Input.GetAxisRaw("Vertical");
                if (currentSpeed > walkSpeed && forwardInput > 0.1f)
                {
                    isSliding = true;
                    currentSlideSpeed = slideSpeed;
                    transform.localScale = new Vector3(originalScale.x, originalScale.y * crouchScale, originalScale.z);
                }
            }
            else
            {
                isCrouching = true;
                transform.localScale = new Vector3(originalScale.x, originalScale.y * crouchScale, originalScale.z);
            }
        }
        
        if (m_Crouch.WasReleasedThisFrame())
        {
            wantsToStandUp = true;
        }
        
        if (wantsToStandUp && !Physics.Raycast(transform.position, Vector3.up, 1f))
        {
            if (!isGrounded)
            {
                airVelocity = new Vector3(velocity.x, 0f, velocity.z);
            }
            
            isSliding = false;
            isCrouching = false;
            currentSlideSpeed = 0f;
            transform.localScale = originalScale;
            wantsToStandUp = false;
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
        if (!isJumping && isGrounded)
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
        if (!isJumping && isGrounded)
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
    
    public void ClearAirVelocity()
    {
        airVelocity = Vector3.zero;
    }
}
