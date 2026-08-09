using System.Collections;
using SmallHedge.SoundManager;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("EnemysDebug")]
    [SerializeField] public bool isDamaged = false;

    [Header("Camera Settings")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private float sens = 1f;
    [SerializeField] private float maxYAngle = 80f;
    [SerializeField] private Vector3 cameraStartRotation = Vector3.zero;
    [SerializeField] public float tiltAmount = 5f;
    [SerializeField] private float tiltSpeedThreshold = 1f;
    [SerializeField] private float tiltSmoothTime = 0.1f;
    [SerializeField] private bool canMoveCamera = true;

    private float currentTilt;
    private float tiltVelocity;
    private float rotationX;
    private float externalMouseX;
    private float externalMouseY;

    [Header("Head Bob Settings")]
    [SerializeField] private bool enableHeadBob = true;
    [SerializeField] private float walkBobSpeed = 12f;
    [SerializeField] private float walkBobAmount = 0.05f;
    [SerializeField] private float walkBobAmountX = 0.03f;
    [SerializeField] private float walkBobAmountY = 0.02f;
    [SerializeField] private float runBobSpeed = 20f;
    [SerializeField] private float runBobAmount = 0.12f;
    [SerializeField] private float runBobAmountX = 0.06f;
    [SerializeField] private float runBobAmountY = 0.04f;
    private float defaultHeadBobY;
    private float defaultHeadBobX;
    private float headBobTimer;
    private Vector3 headBobTargetPos;

    [Header("Speed FOV & VFX Settings")]
    [SerializeField] public float normalFOV = 80f;
    [SerializeField] public float minFOV = 40f;
    [SerializeField] public float speedThreshold = 20f;
    [SerializeField] public float shrinkMultiplier = 0.5f;
    [SerializeField] public float fovSmoothTime = 0.2f;
    [SerializeField] public GameObject vfxGameobject;
    [SerializeField] public AudioSource wingAudioSource;
    [SerializeField] public float audioSpeedThreshold = 15f;
    [SerializeField] public float maxVolume = 0.8f;
    [SerializeField] public float audioSmoothTime = 0.3f;
    private float targetSpeedFOV;
    private float speedFovVelocity;
    private bool vfxWasActive;
    private float audioVolumeVelocity;

    [Header("Scanner Settings")]
    [SerializeField] private GameObject scannerPrefab;
    [SerializeField] private float growSpeed = 1f;
    [SerializeField] private float destroyDelay = 3f;
    [SerializeField] private int scanCount = 3;
    [SerializeField] private float spawnDelay = 0.1f;
    [SerializeField] private KeyCode scanKey = KeyCode.F;
    private bool isScanning;

    [Header("Movement")]
    [SerializeField] private float _currentSpeedDisplay;
    [SerializeField] public float walkSpeed = 6f;
    [SerializeField] public float runSpeed = 12f;
    [SerializeField] public float gravity = -9.81f;
    [SerializeField] private float airControlStrength = 2f;
    public bool isGrounded;
    public bool isOnAir = false;
    public bool isSafeFromRunner = false;

    [Header("Jump")]
    [SerializeField] public float jumpHeight = 2f;
    [SerializeField] private float slideJumpHeight = 2.5f;
    [SerializeField] private float slideJumpGravity = -15f;
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
    private float originalFov;
    private float currentFov;
    private float fovVelocity;
    private bool wasMoving = false;

    [Header("Slide")]
    [SerializeField] public float slideSpeed = 14f;
    [SerializeField] public float slideAcceleration = 5f;
    [SerializeField] public float maxSlideSpeed = 22f;
    [SerializeField] private float crouchScale = 0.5f;
    [SerializeField] private float crouchSpeed = 3f;
    [SerializeField] private float slideDecayDelay = 3f;
    [SerializeField] private float slideDecayRate = 5f;
    public float currentSlideSpeed;
    public bool isSliding;
    private float slideTimer;
    private bool isSlowSlide;
    private bool isCrouching;
    private bool wantsToStandUp;
    private bool wantsToSlide;

    [Header("Ground Check")]
    [SerializeField] private float groundCheckDistance = 0.2f;
    [SerializeField] private float groundCheckRadius = 0.9f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask slideLayer;
    [SerializeField] private LayerMask rampLayer;

    [Header("Surface Alignment")]
    [SerializeField] private bool enableSurfaceAlignment = true;
    [SerializeField] private float alignmentSmoothness = 8f;
    [SerializeField] private float maxSlopeAngle = 60f;
    [SerializeField] private LayerMask alignmentLayers;

    [SerializeField] private float initialAirVelocity = -5f;
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
    public bool IsMoving() => horizontalVelocity.sqrMagnitude > 0.01f && isGrounded && !isSliding;
    public bool IsRunning() => horizontalVelocity.sqrMagnitude > (runSpeed * 0.7f) * (runSpeed * 0.7f);

    [SerializeField] private bool usePlayerController = true;

    // Cached values to avoid allocations
    private float inputH;
    private float inputV;

    private void Awake()
    {
        if (controller == null)
            controller = GetComponent<CharacterController>();
    }

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

        if (controller == null)
            controller = GetComponent<CharacterController>();

        originalScale = transform.localScale;
        originalRunSpeed = runSpeed;

        if (mainCamera == null)
            mainCamera = Camera.main;

        if (mainCamera != null)
        {
            originalFov = mainCamera.fieldOfView;
            currentFov = originalFov;
            targetSpeedFOV = normalFOV;
            
            Vector3 localEuler = mainCamera.transform.localEulerAngles;
            rotationX = NormalizeAngle(localEuler.x);
            rotationX = Mathf.Clamp(rotationX, -maxYAngle, maxYAngle);

            defaultHeadBobY = mainCamera.transform.localPosition.y;
            defaultHeadBobX = mainCamera.transform.localPosition.x;
            headBobTargetPos = mainCamera.transform.localPosition;
        }

        if (vfxGameobject != null)
        {
            vfxGameobject.SetActive(false);
            vfxWasActive = false;
        }

        InitializeWingAudio();

        if (sprintFillContainer != null)
            sprintFillContainer.SetActive(false);

        SetCameraMovementEnabled(canMoveCamera);
    }

    private void InitializeWingAudio()
    {
        if (wingAudioSource == null && mainCamera != null)
        {
            wingAudioSource = mainCamera.GetComponent<AudioSource>();
        }

        if (wingAudioSource != null)
        {
            wingAudioSource.volume = 0f;
            wingAudioSource.loop = true;
            wingAudioSource.playOnAwake = false;
            if (!wingAudioSource.isPlaying)
            {
                wingAudioSource.Play();
            }
        }
    }

    public void SetSensitivity(float value)
    {
        sens = value;
    }

    public void AddExternalRotation(float x, float y)
    {
        externalMouseX += x;
        externalMouseY += y;
    }

    public void SetCameraMovementEnabled(bool value)
    {
        canMoveCamera = value;

        if (canMoveCamera)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            currentTilt = Mathf.SmoothDamp(currentTilt, 0, ref tiltVelocity, tiltSmoothTime);
            if (mainCamera != null)
            {
                mainCamera.transform.localRotation = Quaternion.Euler(rotationX, 0f, currentTilt);
            }
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
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
        HandleCameraLook();
        HandleSpeedFovAndVfx();
        HandleHeadBob();
        HandleScannerInput();

        inputH = Input.GetAxisRaw("Horizontal");
        inputV = Input.GetAxisRaw("Vertical");

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

        bool isTryingToMove = (inputH != 0f || inputV != 0f);
        bool isActuallyMoving = horizontalVelocity.sqrMagnitude > 0.01f;

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
            if (horizontalVelocity.sqrMagnitude < 0.01f || (horizontalVelocity.magnitude < minSpeedForSprint && isGrounded))
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

    private void HandleSpeedFovAndVfx()
    {
        if (mainCamera == null) return;

        float speed = currentSpeed;

        if (speed <= speedThreshold)
        {
            targetSpeedFOV = normalFOV;
        }
        else
        {
            float excessSpeed = speed - speedThreshold;
            float fovReduction = excessSpeed * shrinkMultiplier;
            targetSpeedFOV = Mathf.Max(normalFOV - fovReduction, minFOV);
        }

        if (isSprintActive)
        {
            targetSpeedFOV += sprintFovIncrease;
        }

        mainCamera.fieldOfView = Mathf.SmoothDamp(mainCamera.fieldOfView, targetSpeedFOV, ref speedFovVelocity, fovSmoothTime);

        if (vfxGameobject != null)
        {
            bool shouldBeActive = speed > speedThreshold;
            if (shouldBeActive != vfxWasActive)
            {
                vfxGameobject.SetActive(shouldBeActive);
                vfxWasActive = shouldBeActive;
            }
        }

        if (wingAudioSource != null)
        {
            float targetVolume = 0f;
            if (speed > audioSpeedThreshold)
            {
                float excessSpeed = speed - audioSpeedThreshold;
                float volumeRange = Mathf.Max(speedThreshold - audioSpeedThreshold, 1f);
                targetVolume = Mathf.Clamp01(excessSpeed / volumeRange) * maxVolume;
            }
            wingAudioSource.volume = Mathf.SmoothDamp(wingAudioSource.volume, targetVolume, ref audioVolumeVelocity, audioSmoothTime);
            if (wingAudioSource.volume < 0.01f && wingAudioSource.volume > 0f)
            {
                wingAudioSource.volume = 0f;
            }
        }
    }

    private void HandleHeadBob()
    {
        if (!enableHeadBob || mainCamera == null) return;

        float speedSqr = horizontalVelocity.sqrMagnitude;
        bool isMoving = speedSqr > 0.01f && isGrounded;

        if (isMoving && !isSliding)
        {
            bool isRunning = speedSqr > (runSpeed * 0.7f) * (runSpeed * 0.7f);
            float bobSpeed = isRunning ? runBobSpeed : walkBobSpeed;
            float bobAmount = isRunning ? runBobAmount : walkBobAmount;
            float bobAmountX = isRunning ? runBobAmountX : walkBobAmountX;
            float bobAmountY = isRunning ? runBobAmountY : walkBobAmountY;

            headBobTimer += Time.deltaTime * bobSpeed;
            float newY = defaultHeadBobY + Mathf.Sin(headBobTimer) * bobAmount + Mathf.Cos(headBobTimer * 0.8f) * bobAmountY;
            float newX = defaultHeadBobX + Mathf.Cos(headBobTimer * 0.5f) * bobAmountX;

            headBobTargetPos.x = Mathf.Lerp(mainCamera.transform.localPosition.x, newX, Time.deltaTime * 10f);
            headBobTargetPos.y = Mathf.Lerp(mainCamera.transform.localPosition.y, newY, Time.deltaTime * 10f);
            headBobTargetPos.z = mainCamera.transform.localPosition.z;
            mainCamera.transform.localPosition = headBobTargetPos;
        }
        else
        {
            headBobTimer = 0f;
            headBobTargetPos.x = Mathf.Lerp(mainCamera.transform.localPosition.x, defaultHeadBobX, Time.deltaTime * 10f);
            headBobTargetPos.y = Mathf.Lerp(mainCamera.transform.localPosition.y, defaultHeadBobY, Time.deltaTime * 10f);
            headBobTargetPos.z = mainCamera.transform.localPosition.z;
            mainCamera.transform.localPosition = headBobTargetPos;
        }
    }

    private void HandleScannerInput()
    {
        if (Input.GetKeyDown(scanKey) && !isScanning && scannerPrefab != null)
        {
            StartCoroutine(SpawnScans());
        }
    }

    private IEnumerator SpawnScans()
    {
        isScanning = true;

        for (int i = 0; i < scanCount; i++)
        {
            if (scannerPrefab != null)
            {
                GameObject scan = Instantiate(scannerPrefab, transform.position, Quaternion.identity);
                StartCoroutine(GrowScan(scan));
                Destroy(scan, destroyDelay);
            }
            yield return new WaitForSeconds(spawnDelay);
        }

        isScanning = false;
    }

    private IEnumerator GrowScan(GameObject scan)
    {
        while (scan != null)
        {
            scan.transform.localScale += Vector3.one * growSpeed * Time.deltaTime;
            yield return null;
        }
    }

    private void HandleCameraLook()
    {
        float mouseX = 0f;
        float mouseY = 0f;

        if (canMoveCamera)
        {
            mouseX = Input.GetAxis("Mouse X") + externalMouseX;
            mouseY = Input.GetAxis("Mouse Y") + externalMouseY;

            externalMouseX = 0f;
            externalMouseY = 0f;
        }

        transform.Rotate(Vector3.up * (mouseX * sens));

        if (!float.IsNaN(mouseY) && !float.IsInfinity(mouseY))
        {
            rotationX = Mathf.Clamp(rotationX - mouseY * sens, -maxYAngle, maxYAngle);
        }

        float rotationSpeed = canMoveCamera ? Mathf.Abs(Input.GetAxis("Mouse X")) : 0f;

        if (rotationSpeed > tiltSpeedThreshold && Mathf.Abs(mouseX) > 0.01f)
        {
            float tiltDirection = mouseX > 0 ? -tiltAmount : tiltAmount;
            currentTilt = Mathf.SmoothDamp(currentTilt, tiltDirection, ref tiltVelocity, tiltSmoothTime);
        }
        else
        {
            currentTilt = Mathf.SmoothDamp(currentTilt, 0, ref tiltVelocity, tiltSmoothTime);
        }

        if (mainCamera != null)
        {
            Vector3 finalRotation = new Vector3(rotationX, 0f, currentTilt);
            if (IsValidRotation(finalRotation))
            {
                mainCamera.transform.localRotation = Quaternion.Euler(finalRotation);
            }
            else
            {
                rotationX = 0f;
                currentTilt = 0f;
                mainCamera.transform.localRotation = Quaternion.Euler(cameraStartRotation);
            }
        }
    }

    private float NormalizeAngle(float angle)
    {
        while (angle > 180f) angle -= 360f;
        while (angle < -180f) angle += 360f;
        return angle;
    }

    private bool IsValidRotation(Vector3 eulerAngles)
    {
        return !float.IsNaN(eulerAngles.x) && !float.IsNaN(eulerAngles.y) && !float.IsNaN(eulerAngles.z) &&
               !float.IsInfinity(eulerAngles.x) && !float.IsInfinity(eulerAngles.y) && !float.IsInfinity(eulerAngles.z) &&
               Mathf.Abs(eulerAngles.x) <= 90f && Mathf.Abs(eulerAngles.z) <= 90f;
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
        Vector3 inputDir = transform.right * inputH + transform.forward * inputV;
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
        if (isCrouching) return crouchSpeed;
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
            if (!Input.GetKey(runKey) || horizontalVelocity.sqrMagnitude < 0.01f)
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
        SoundManager.PlaySound(SoundType.JUMP);
    }

    private void SlideJump()
    {
        velocity.y = Mathf.Sqrt(slideJumpHeight * -2f * slideJumpGravity);
        isJumping = true;
        SoundManager.PlaySound(SoundType.JUMP);
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
            if (speed >= 7f)
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
        return currentSpeed >= 7f && inputV > 0.1f;
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

        isSlowSlide = horizontalVelocity.magnitude <= 11f;

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
        float appliedDecayDelay = isSlowSlide ? 0f : slideDecayDelay;
        float appliedDecayRate = isSlowSlide ? slideDecayRate * 1.5f : slideDecayRate;

        if (slideTimer > appliedDecayDelay)
        {
            currentSlideSpeed = Mathf.Max(currentSlideSpeed - appliedDecayRate * Time.deltaTime, 0f);
        }
        else
        {
            currentSlideSpeed = currentSlideSpeed > slideSpeed ? Mathf.Max(currentSlideSpeed - slideAcceleration * 0.5f * Time.deltaTime, slideSpeed) : currentSlideSpeed;
        }

        ApplySlideVelocity(camDir);

        if (currentSlideSpeed <= 0.5f)
        {
            isSliding = false;
            isCrouching = true;
            currentSlideSpeed = 0f;
        }
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

    public void SetKeybinds(KeyCode crouch, KeyCode run, KeyCode jump)
    {
        slideKey = crouch;
        runKey = run;
        jumpKey = jump;
    }
}

