using UnityEngine; 

public class PlayerController : MonoBehaviour
{
    [Header("EnemysDebug")]
    [SerializeField] public bool isDamaged = false;
    [Header("Movement")]
    [SerializeField] public float walkSpeed;
    [SerializeField] public float runSpeed;
    [SerializeField] public float gravity;
    [SerializeField] private float airControlStrength = 2f;
    public bool isGrounded;

    [Header("Jump")]
    [SerializeField] private float jumpHeight;
    [SerializeField] private float slideJumpHeight;
    [SerializeField] private float slideJumpGravity;
    public bool isJumping;
    
    [Header("Keybinds")]
    private KeyCode slideKey = KeyCode.LeftControl;
    private KeyCode runKey = KeyCode.LeftShift;
    private KeyCode jumpKey = KeyCode.Space;
    
    [Header("Slide")]
    [SerializeField] private float slideSpeed;
    [SerializeField] private float slideAcceleration;
    [SerializeField] private float maxSlideSpeed;
    [SerializeField] private float crouchScale;
    [SerializeField] private float crouchSpeed;
    public float currentSlideSpeed;
    public bool isSliding;
    private bool isCrouching;
    private bool wantsToStandUp;
    private bool wantsToSlide;
    
    [Header("Ground Check")]
    [SerializeField] private float groundCheckDistance;
    [SerializeField] private float groundCheckRadius = 0.9f;
    [SerializeField] private int gizmoLineCount = 8;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask slideLayer;
    [SerializeField] private LayerMask rampLayer;
    
    [Header("Surface Alignment")]
    [SerializeField] private bool enableSurfaceAlignment = true;
    [SerializeField] private float alignmentSmoothness = 8f;
    [SerializeField] private float maxSlopeAngle = 60f;
    [SerializeField] private LayerMask alignmentLayers;

    [SerializeField] private float initialAirVelocity;
    private CharacterController controller;
    public Vector3 velocity;
    private Vector3 originalScale;
    private bool wasGrounded;
    private bool wasOnRamp;
    private Vector3 airVelocity;
    private float previousYPosition;
    private bool isOnSlideLayer;
    private Vector3 currentSurfaceNormal = Vector3.up;
    private Vector3 targetSurfaceNormal = Vector3.up;
    
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
    public float currentSpeed => horizontalVelocity.magnitude;
    public float maxSpeed => runSpeed;
    
    private void Start()
    {
        controller = GetComponent<CharacterController>();
        originalScale = transform.localScale;
    }
    
    private void Update()
    {
        bool previouslyGrounded = wasGrounded;
        bool previouslyOnRamp = wasOnRamp;
        CheckGround();
        
        HandleAirTransitions(previouslyGrounded, previouslyOnRamp);
        
        if (!previouslyGrounded && isGrounded && !isSliding && !wantsToSlide)
        {
            currentSlideSpeed = 0f;
        }
        
        wasGrounded = isGrounded;
        wasOnRamp = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, rampLayer);
        
        if (justWallJumped && isGrounded)
        {
            justWallJumped = false;
        }
        
        HandleMovement();
        HandleJump();
        
        bool wasOnSlideLayer = isOnSlideLayer;
        HandleSlide();
        
        if (wasOnSlideLayer && !isOnSlideLayer && velocity.y < -5f)
        {
            velocity.y = -2f;
        }
        
        AlignToSurface();
        
        previousYPosition = transform.position.y;
        ApplyMovement();
    }
    
    private void HandleAirTransitions(bool previouslyGrounded, bool previouslyOnRamp)
    {
        if (isJumping || velocity.y >= 0) return;
        
        if (previouslyOnRamp && !isGrounded && isSliding)
        {
            velocity.y = initialAirVelocity;
        }
    }
    
    private void CheckGround()
    {
        float scaleMultiplier = transform.localScale.y / originalScale.y;
        
        Vector3 origin = transform.position + Vector3.up * (controller.radius * 0.5f * scaleMultiplier);
        float checkDist = groundCheckDistance + controller.radius * 0.5f * scaleMultiplier;
        float sphereRadius = controller.radius * groundCheckRadius * scaleMultiplier;
        
        isGrounded = Physics.SphereCast(origin, sphereRadius, Vector3.down, out RaycastHit hit, checkDist) &&
                     ((1 << hit.collider.gameObject.layer) & (groundLayer | slideLayer | rampLayer)) != 0;
        
        if (enableSurfaceAlignment)
        {
            targetSurfaceNormal = (isGrounded && ((1 << hit.collider.gameObject.layer) & alignmentLayers) != 0) 
                ? ClampNormal(hit.normal) : Vector3.up;
        }
    }
    
    private void HandleMovement()
    {
        Vector3 inputDir = GetInputDirection();
        
        if (isSliding && !isGrounded)
        {
            HandleAirMovement(inputDir);
            return;
        }
        
        if (isSliding || wallRunning || justWallJumped || activeGrappling)
        {
            return;
        }
        
        if (!isGrounded)
        {
            HandleAirMovement(inputDir);
            return;
        }
        
        if (wantsToSlide)
        {
            return;
        }
        
        float speed = GetMovementSpeed();
        velocity.x = inputDir.x * speed;
        velocity.z = inputDir.z * speed;
        airVelocity = horizontalVelocity;
    }
    
    private Vector3 GetInputDirection()
    {
        return transform.right * Input.GetAxisRaw("Horizontal") + transform.forward * Input.GetAxisRaw("Vertical");
    }
    
    private void HandleAirMovement(Vector3 inputDir)
    {
        if (inputDir.sqrMagnitude > 0.1f)
        {
            Vector3 currentHorizontal = horizontalVelocity;
            float currentMagnitude = currentHorizontal.magnitude;
            
            if (currentMagnitude < walkSpeed * 0.5f)
            {
                velocity.x += inputDir.x * airControlStrength * Time.deltaTime;
                velocity.z += inputDir.z * airControlStrength * Time.deltaTime;
                
                Vector3 clampedHorizontal = horizontalVelocity;
                if (clampedHorizontal.magnitude > walkSpeed)
                {
                    clampedHorizontal = clampedHorizontal.normalized * walkSpeed;
                    velocity.x = clampedHorizontal.x;
                    velocity.z = clampedHorizontal.z;
                }
            }
            else
            {
                Vector3 newDir = (currentHorizontal + inputDir * airControlStrength * Time.deltaTime).normalized;
                velocity.x = newDir.x * currentMagnitude;
                velocity.z = newDir.z * currentMagnitude;
            }
        }
    }
    
    private float GetMovementSpeed()
    {
        return isCrouching ? crouchSpeed : (Input.GetKey(runKey) ? runSpeed : walkSpeed);
    }
    
    private void HandleJump()
    {
        if (!Input.GetKeyDown(jumpKey) || !isGrounded) return;
        
        if (isSliding)
        {
            SlideJump();
        }
        else
        {
            NormalJump();
        }
    }
    
    private void NormalJump()
    {
        velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        isJumping = true;
    }
    
    private void SlideJump()
    {
        velocity.y = Mathf.Sqrt(slideJumpHeight * -2f * slideJumpGravity);
        isJumping = true;
    }
    
    private void HandleSlide()
    {
        if (Input.GetKeyDown(slideKey))
        {
            HandleSlideInput();
        }
        if (Input.GetKeyUp(slideKey))
        {
            wantsToStandUp = true;
        }
        
        if (wantsToStandUp && CanStandUp()) 
        {
            StandUp();
        }
        if (wantsToSlide && isGrounded)
        {
            float speed = Mathf.Max(currentSpeed, currentSlideSpeed);
            if (speed >= 10f)
            {
                StartSlide();
            }
            else
            {
                StartCrouch();
            }
            wantsToSlide = false; 
        }
        if (isSliding && isGrounded)
        {
            UpdateSlide();
        }
        else if (!isGrounded)
        {
            isOnSlideLayer = false;
        }
    }
    
    private void HandleSlideInput()
    {
        if (!isGrounded)
        {
            wantsToSlide = true;
        }
        else if (CanStartSlide())
        {
            StartSlide();
        }
        else
        {
            StartCrouch();
        }
    }
    
    private bool CanStartSlide()
    {
        return Input.GetKey(runKey) && currentSpeed > walkSpeed && Input.GetAxisRaw("Vertical") > 0.1f;
    }
    
    private bool CanStandUp()
    {
        return !Physics.Raycast(transform.position, Vector3.up, 1f);
    }
    
    private void StandUp()
    {
        if (!isGrounded) airVelocity = horizontalVelocity;
        
        isSliding = false;
        isCrouching = false;
        currentSlideSpeed = 0f;
        isOnSlideLayer = false;
        transform.localScale = originalScale;
        wantsToStandUp = false;
        wantsToSlide = false;
    }
    
    private void StartCrouch()
    {
        isCrouching = true;
        SetCrouchScale();
    }
    
    private void SetCrouchScale()
    {
        transform.localScale = new Vector3(originalScale.x, originalScale.y * crouchScale, originalScale.z);
    }
    
    private void StartSlide()
    {
        isSliding = true;
        if (currentSlideSpeed <= slideSpeed)
        {
            currentSlideSpeed = slideSpeed;
        }
        SetCrouchScale();
    }
    
    private void UpdateSlide()
    {
        float scaleMultiplier = transform.localScale.y / originalScale.y;
        
        Vector3 origin = transform.position + Vector3.up * (controller.radius * 0.5f * scaleMultiplier);
        float checkDist = groundCheckDistance + controller.radius * 0.5f * scaleMultiplier;
        float sphereRadius = controller.radius * groundCheckRadius * scaleMultiplier;
        
        bool onSlope = Physics.SphereCast(origin, sphereRadius, Vector3.down, out RaycastHit slopeHit, checkDist, slideLayer);
        isOnSlideLayer = onSlope;
        
        if (onSlope)
        {
            HandleSlopeSliding(slopeHit);
        }
        else if (Physics.SphereCast(origin, sphereRadius, Vector3.down, out RaycastHit groundHit, checkDist, groundLayer))
        {
            HandleGroundSliding(groundHit);
        }
    }
    
    private void HandleSlopeSliding(RaycastHit hit)
    {
        if (transform.position.y > previousYPosition + 0.01f && !isJumping)
        {
            if (CanStandUp())
            {
                StandUp();
                return;
            }
        }
        
        if (enableSurfaceAlignment && ((1 << hit.collider.gameObject.layer) & alignmentLayers) != 0)
            targetSurfaceNormal = ClampNormal(hit.normal);
        
        Vector3 camDir = transform.forward;
        camDir.y = 0;
        Vector3 slopeDir = Vector3.ProjectOnPlane(camDir.normalized, hit.normal).normalized;
        
        currentSlideSpeed = Mathf.Min(currentSlideSpeed + slideAcceleration * Time.deltaTime, maxSlideSpeed);
        ApplySlideVelocity(slopeDir);
    }
    
    private void HandleGroundSliding(RaycastHit hit)
    {
        if (enableSurfaceAlignment && ((1 << hit.collider.gameObject.layer) & alignmentLayers) != 0)
            targetSurfaceNormal = ClampNormal(hit.normal);
        
        Vector3 camDir = transform.forward;
        camDir.y = 0;
        camDir.Normalize();
        
        currentSlideSpeed = currentSlideSpeed > slideSpeed ?
            Mathf.Max(currentSlideSpeed - slideAcceleration * 0.5f * Time.deltaTime, slideSpeed) : slideSpeed;
        
        ApplySlideVelocity(camDir);
    }
    
    private void ApplySlideVelocity(Vector3 direction)
    {
        velocity.x = direction.x * currentSlideSpeed;
        velocity.z = direction.z * currentSlideSpeed;
    }
    
    private void ApplyMovement()
    {
        if (!wallRunning && !activeGrappling)
        {
            velocity.y += gravity * Time.deltaTime;
            velocity.y = Mathf.Max(velocity.y, gravity);
        }
        
        if (isGrounded && velocity.y <= 0)
        {
            if (isSliding && isOnSlideLayer)
            {
                velocity.y = -30f;
            }
            else
            {
                if (enableSurfaceAlignment && !isJumping && Vector3.Angle(Vector3.up, currentSurfaceNormal) > 5f && currentSpeed > walkSpeed * 0.8f)
                    velocity.y = -10f;
                else
                    velocity.y = -2f;
            }
            isJumping = false;
        }
        
        Vector3 previousPosition = transform.position;
        controller.Move(velocity * Time.deltaTime);
        
        CheckCeilingCollision(previousPosition);
    }
    
    private void CheckCeilingCollision(Vector3 previousPosition)
    {
        if (velocity.y > 3f && !isJumping && Mathf.Abs(transform.position.y - previousPosition.y) < 0.01f)
        {
            velocity.y = 0;
            velocity.x *= 0.7f;
            velocity.z *= 0.7f;
        }
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
    
    private Vector3 ClampNormal(Vector3 normal)
    {
        float angle = Vector3.Angle(Vector3.up, normal);
        return angle > maxSlopeAngle ? Vector3.Slerp(Vector3.up, normal, maxSlopeAngle / angle) : normal;
    }
    
    private void AlignToSurface()
    {
        if (!enableSurfaceAlignment || wallRunning || activeGrappling) return;
        
        currentSurfaceNormal = Vector3.Slerp(currentSurfaceNormal, targetSurfaceNormal, alignmentSmoothness * Time.deltaTime);
        
        Vector3 forward = Vector3.ProjectOnPlane(transform.forward, currentSurfaceNormal).normalized;
        Quaternion target = Quaternion.LookRotation(forward, currentSurfaceNormal);
        Vector3 euler = target.eulerAngles;
        euler.y = transform.eulerAngles.y;
        euler.z = transform.eulerAngles.z;
        
        transform.rotation = Quaternion.Euler(euler);
    }
    
    private void OnDrawGizmos()
    {
        if (controller == null)
        {
            controller = GetComponent<CharacterController>();
            if (controller == null) return;
        }
        
        float scaleMultiplier = 1f;
        if (originalScale.y > 0)
        {
            scaleMultiplier = transform.localScale.y / originalScale.y;
        }
        
        Vector3 origin = transform.position + Vector3.up * (controller.radius * 0.5f * scaleMultiplier);
        float checkDist = groundCheckDistance + controller.radius * 0.5f * scaleMultiplier;
        float sphereRadius = controller.radius * groundCheckRadius * scaleMultiplier;
        
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawWireSphere(origin, sphereRadius);
        
        Vector3 endPos = origin + Vector3.down * checkDist;
        Gizmos.DrawWireSphere(endPos, sphereRadius);
        
        int lineCount = Mathf.Max(3, gizmoLineCount);
        float angleStep = 360f / lineCount;
        
        for (int i = 0; i < lineCount; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            Vector3 offset = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * sphereRadius;
            Gizmos.DrawLine(origin + offset, endPos + offset);
        }
    }

    public void SetKeybinds(KeyCode crouch, KeyCode run, KeyCode jump)
    {
        slideKey = crouch;
        runKey = run;
        jumpKey = jump;
    }
}
