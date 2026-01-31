using SmallHedge.SoundManager;
using UnityEngine; 

public class PlayerController : MonoBehaviour
{
    
    [Header("EnemysDebug")]
    [SerializeField] public bool isDamaged = false;
    [Header("Movement")]
    [SerializeField] private float _currentSpeedDisplay;
    [SerializeField] public float walkSpeed;
    [SerializeField] public float runSpeed;
    [SerializeField] public float gravity;
    [SerializeField] private float airControlStrength = 2f;
    public bool isGrounded;
    public bool isOnAir = false;
    public bool isSafeFromRunner = false;

    [Header("Jump")]
    [SerializeField] public float jumpHeight;
    [SerializeField] private float slideJumpHeight;
    [SerializeField] private float slideJumpGravity;
    public bool isJumping;
    
    [Header("Keybinds")]
    private KeyCode slideKey = KeyCode.LeftControl;
    private KeyCode runKey = KeyCode.LeftShift;
    private KeyCode jumpKey = KeyCode.Space;
    
    [Header("Sprint Hold System")]
    [SerializeField] private bool sprintHoldEnabled = true;
    [SerializeField] private float sprintHoldDuration = 2f;
    [SerializeField] private float sprintSpeedBonus = 5f;
    [SerializeField] private float sprintSlideBonus = 5f; 
    [SerializeField] private float sprintFovIncrease = 20f;
    [SerializeField] private float sprintFovSmoothTime = 1f;
    [SerializeField] private float speedBuffer = 2f; 
    [SerializeField] private UnityEngine.UI.Image sprintFillImage;
    [SerializeField] private GameObject sprintFillContainer;
    private float sprintHoldTimer = 0f;
    private bool isSprintHolding = false;
    private bool isSprintActive = false;
    private float originalRunSpeed;
    [SerializeField] private Camera mainCamera;
    private float originalFov;
    private float currentFov;
    private float fovVelocity;
    private bool wasMoving = false; 
    
    [Header("Slide")]
    [SerializeField] public float slideSpeed;
    [SerializeField] public float slideAcceleration;
    [SerializeField] public float maxSlideSpeed;
    [SerializeField] private float crouchScale;
    [SerializeField] private float crouchSpeed;
    [SerializeField] private float slideDecayDelay = 3f;
    [SerializeField] private float slideDecayRate = 5f;
    public float currentSlideSpeed;
    public bool isSliding;
    private float slideTimer;
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
    [SerializeField] public CharacterController controller;
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
    public bool IsSprintPressed => Input.GetKey(runKey);
    public bool HasSpeedBoost => isSprintActive;
    public bool CanWallRun => IsSprintPressed || HasSpeedBoost;
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

    [SerializeField] private bool usePlayerController = true;

    private void Start()
    {
        string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (currentScene == "Intro4" || currentScene == "Hub")
        {
            if (PlayerPrefs.GetInt("HubReached", 0) == 0)
            {
                PlayerPrefs.SetInt("HubReached", 1);
                PlayerPrefs.Save();
            }
        }

        controller = GetComponent<CharacterController>();
        originalScale = transform.localScale;
        originalRunSpeed = runSpeed;
        if (mainCamera != null)
        {
            originalFov = mainCamera.fieldOfView;
            currentFov = originalFov;
        }
            
        if (sprintFillContainer != null)
            sprintFillContainer.SetActive(false);
    }

    public void SetMovementEnabled(bool value)
    {
        if (value)
        {
            usePlayerController = true;
        }
        else
        {
            usePlayerController = false;
            velocity = Vector3.zero;
        }
    }
    
    private void Update()
    {
        if (mainCamera != null)
        {
            mainCamera.fieldOfView = Mathf.SmoothDamp(mainCamera.fieldOfView, currentFov, ref fovVelocity, sprintFovSmoothTime);
        }


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
        
        bool isTryingToMove = (Input.GetAxisRaw("Horizontal") != 0f || Input.GetAxisRaw("Vertical") != 0f);
        bool isActuallyMoving = horizontalVelocity.magnitude > 0.1f;
        
        if (isTryingToMove && !isActuallyMoving && wasMoving && isSprintActive)
        {
            ResetSprint();
        }
        wasMoving = isActuallyMoving;

        if (!usePlayerController) return;
        
        HandleSprintInput();
        HandleMovement();
        HandleJump();
        
        if (isSprintActive && !isSliding)
        {
            float minSpeedForSprint = (originalRunSpeed + sprintSpeedBonus) - speedBuffer;
            if (horizontalVelocity.magnitude < 0.1f || (horizontalVelocity.magnitude < minSpeedForSprint && isGrounded))
            {
                ResetSprint();
            }
        }
        
        bool wasOnSlideLayer = isOnSlideLayer;
        HandleSlide();
        
        if (wasOnSlideLayer && !isOnSlideLayer && velocity.y < -5f)
        {
            velocity.y = -2f;
        }
        
        AlignToSurface();
        
        previousYPosition = transform.position.y;
        ApplyMovement();
        
        _currentSpeedDisplay = currentSpeed;
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
            bool isOnAlignmentLayer = isGrounded && ((1 << hit.collider.gameObject.layer) & alignmentLayers) != 0;
            targetSurfaceNormal = isOnAlignmentLayer ? ClampNormal(hit.normal) : Vector3.up;
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
        Vector3 inputDir = transform.right * Input.GetAxisRaw("Horizontal") + transform.forward * Input.GetAxisRaw("Vertical");
        return inputDir.normalized;
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
        return (Input.GetKey(runKey) || isSprintActive) ? runSpeed : walkSpeed;
    }

    public void EnableSprintHold(bool enabled)
    {
        sprintHoldEnabled = enabled;
    }
    
    private void HandleSprintInput()
    {
        if (!sprintHoldEnabled || isSprintActive) return;
        
        float minRunSpeed = walkSpeed + 0.1f; 

        bool isRunning = Input.GetKey(runKey) && 
                       (isGrounded || (isSprintActive && !isGrounded)) && 
                       horizontalVelocity.magnitude > minRunSpeed;
        
        if (isRunning && !isSprintHolding && !isSprintActive)
        {
            if (isSprintHolding)
            {
                StopAllCoroutines();
            }
            StartCoroutine(HoldSprintRoutine());
        }
        else if (!isRunning && isSprintHolding)
        {
            StopAllCoroutines();
            isSprintHolding = false;
            sprintHoldTimer = 0f;
            
            if (sprintFillContainer != null)
                sprintFillContainer.SetActive(false);
                
            if (sprintFillImage != null)
                sprintFillImage.fillAmount = 0f;
        }
    }
    
    private System.Collections.IEnumerator HoldSprintRoutine()
    {
        isSprintHolding = true;
        
        if (sprintFillContainer != null)
            sprintFillContainer.SetActive(true);
        
        while (sprintHoldTimer < sprintHoldDuration)
        {
            if (!Input.GetKey(runKey) || horizontalVelocity.magnitude < 0.1f)
            {
                isSprintHolding = false;
                sprintHoldTimer = 0f;
                
                if (sprintFillContainer != null)
                    sprintFillContainer.SetActive(false);
                    
                if (sprintFillImage != null)
                    sprintFillImage.fillAmount = 0f;

                    
                yield break;
            }
            
            sprintHoldTimer += Time.deltaTime;
            
            if (sprintFillImage != null)
                sprintFillImage.fillAmount = sprintHoldTimer / sprintHoldDuration;
                
            yield return null;
        }
        
        ActivateSprint();
    }
    
    private void ActivateSprint()
    {
        isSprintActive = true;
        runSpeed += sprintSpeedBonus;
        
        if (mainCamera != null)
            currentFov = originalFov + sprintFovIncrease;
            
        if (sprintFillContainer != null)
            sprintFillContainer.SetActive(false);

        SoundManager.PlaySound(SoundType.UICONFIRM);
    }
    
    private void ResetSprint()
    {
        if (!isSliding)
        {
            isSprintActive = false;
            isSprintHolding = false;
            sprintHoldTimer = 0f;
            runSpeed = originalRunSpeed;
            
            if (mainCamera != null)
                currentFov = originalFov;
                
            if (sprintFillContainer != null)
                sprintFillContainer.SetActive(false);
                
            if (sprintFillImage != null)
                sprintFillImage.fillAmount = 0f;
        }
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
        return currentSpeed > 10f && Input.GetAxisRaw("Vertical") > 0.1f;
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
        slideTimer = 0f;
        currentSlideSpeed = Mathf.Max(horizontalVelocity.magnitude, slideSpeed);
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
        
        Vector3 camDir = transform.forward;
        camDir.y = 0;
        Vector3 slopeDir = Vector3.ProjectOnPlane(camDir.normalized, hit.normal).normalized;
        
        currentSlideSpeed = Mathf.Min(currentSlideSpeed + slideAcceleration * Time.deltaTime, maxSlideSpeed);
        ApplySlideVelocity(slopeDir);
    }
    
    private void HandleGroundSliding(RaycastHit hit)
    {
        Vector3 camDir = transform.forward;
        camDir.y = 0;
        camDir.Normalize();
        
        slideTimer += Time.deltaTime;
        
        if (slideTimer > slideDecayDelay)
        {
            currentSlideSpeed = Mathf.Max(currentSlideSpeed - slideDecayRate * Time.deltaTime, 0f);
        }
        else
        {
            currentSlideSpeed = currentSlideSpeed > slideSpeed ?
                Mathf.Max(currentSlideSpeed - slideAcceleration * 0.5f * Time.deltaTime, slideSpeed) : slideSpeed;
        }
        
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
                float scaleMultiplier = transform.localScale.y / originalScale.y;
                Vector3 origin = transform.position + Vector3.up * (controller.radius * 0.5f * scaleMultiplier);
                float checkDist = groundCheckDistance + controller.radius * 0.5f * scaleMultiplier;
                float sphereRadius = controller.radius * groundCheckRadius * scaleMultiplier;
                bool onSlideLayer = Physics.SphereCast(origin, sphereRadius, Vector3.down, out RaycastHit hit, checkDist, slideLayer);
                
                if (onSlideLayer)
                {
                    velocity.y = -30f;
                }
                else if (enableSurfaceAlignment && !isJumping && Vector3.Angle(Vector3.up, currentSurfaceNormal) > 5f && currentSpeed > walkSpeed * 0.8f)
                {
                    velocity.y = -10f;
                }
                else
                {
                    velocity.y = -2f;
                }
            }
            isJumping = false;
        }
        
        CollisionFlags collisionFlags = controller.Move(velocity * Time.deltaTime);
        CheckCeilingCollision(collisionFlags);
    }
    
    private void CheckCeilingCollision(CollisionFlags collisionFlags)
    {
        if ((collisionFlags & CollisionFlags.Above) != 0 && velocity.y > 0)
        {
            velocity.y = 0f;
            isJumping = false; 
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
        if (!enableSurfaceAlignment || wallRunning || activeGrappling || justWallJumped) return;
        
        currentSurfaceNormal = Vector3.Slerp(currentSurfaceNormal, targetSurfaceNormal, alignmentSmoothness * Time.deltaTime);
        
        Vector3 forward = Vector3.ProjectOnPlane(transform.forward, currentSurfaceNormal).normalized;
        Quaternion target = Quaternion.LookRotation(forward, currentSurfaceNormal);
        Vector3 euler = target.eulerAngles;
        euler.y = transform.eulerAngles.y;
        
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
