using UnityEngine;

public class WallRunning : MonoBehaviour
{
    [Header("Wall Run Settings")]
    [SerializeField] private float wallStickForce = 8f;
    [SerializeField] private float wallRunSpeed = 12f;
    [SerializeField] private float wallRunAcceleration = 15f;
    [SerializeField] private float wallRunDeceleration = 10f;
    [SerializeField] private float wallRunBrakeDeceleration = 20f;
    
    [Header("Wall Run Durations & Slip")]
    [Tooltip("Длительность идеального зацепа на стене (секунды).")]
    [SerializeField] private float idealGripDuration = 2f;
    [Tooltip("Длительность окна соскальзывания перед полным падением (секунды).")]
    [SerializeField] private float slipDuration = 2f;
    [Tooltip("Максимальная скорость соскальзывания вниз.")]
    [SerializeField] private float maxSlipSpeed = 12f;

    [Header("Wall Detection")]
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private float sphereRadius = 0.4f;
    
    [Header("Wall Jump Settings")]
    [SerializeField] private float wallJumpHeight = 7f;
    [SerializeField] private float wallJumpForce = 15f;
    [SerializeField] private float wallJumpCooldown = 0.3f;

    [Header("Slide Detach Settings")]
    [SerializeField] private KeyCode slideKey = KeyCode.LeftControl;
    [SerializeField] private float slideDetachCooldown = 0.5f;

    [Header("Camera Tilt")]
    [SerializeField] private float wallTiltAngle = 15f;
    [SerializeField] private float tiltSpeed = 5f;
    
    [Header("References")]
    [SerializeField] private Transform cam;
    [SerializeField] private JumpFallSounds jumpFallSounds;

    private KeyCode jumpKey = KeyCode.Space;

    private PlayerController pc;
    private bool isWallRunning;
    private Vector3 wallNormal;
    private Vector3 wallFaceNormal;
    private Vector3 wallForward;
    private bool isActivelyWallRunning;
    public bool IsWallRunning => isWallRunning;
    public bool IsActivelyWallRunning => isActivelyWallRunning;
    private float currentTilt;
    private float wallJumpTimer;
    private float slideDetachTimer;
    private float wallTimer;
    private float currentWallRunSpeed;
    private Collider lastWall;
    private Collider currentWall;

    private static readonly Collider[] wallBuffer = new Collider[16];

    private void Start()
    {
        pc = GetComponent<PlayerController>();
        if (cam == null && Camera.main != null)
        {
            cam = Camera.main.transform;
        }
    }

    private void Update()
    {
        if (wallJumpTimer > 0f)
        {
            wallJumpTimer -= Time.deltaTime;
        }

        if (slideDetachTimer > 0f)
        {
            slideDetachTimer -= Time.deltaTime;
        }
        
        if (isWallRunning)
        {
            if (Input.GetKeyDown(slideKey))
            {
                DetachWithSlide();
                return;
            }

            if (Input.GetKeyDown(jumpKey))
            {
                WallJump();
                return;
            }

            HandleWallRun();
            ApplyWallTilt();
        }
        else
        {
            isActivelyWallRunning = false;
            ResetTilt();
        }
    }

    private void FixedUpdate()
    {
        CheckForWalls();
    }
    
    private void CheckForWalls()
    {
        if (wallJumpTimer > 0f || slideDetachTimer > 0f)
        {
            if (isWallRunning)
            {
                StopWallRun();
            }
            return;
        }
        
        if (pc.grounded)
        {
            if (isWallRunning)
            {
                StopWallRun();
            }
            
            lastWall = null;
            currentTilt = 0f;
            wallTimer = 0f;
            return;
        }
        
        Vector3 origin = transform.position;
        float currentRadius = sphereRadius;

        // Если мы уже бежим по стене — проверяем, осталась ли стена сбоку от нас
        if (isWallRunning)
        {
            // Пускаем луч строго перпендикулярно в плоскость стены, по которой бежим
            if (Physics.Raycast(origin, -wallFaceNormal, out RaycastHit sideHit, sphereRadius + 0.6f, wallLayer))
            {
                // Стена рядом — обновляем нормаль плоскости
                if (Mathf.Abs(Vector3.Dot(sideHit.normal, Vector3.up)) < 0.3f)
                {
                    wallNormal = sideHit.normal;
                    wallFaceNormal = sideHit.normal;
                    currentWall = sideHit.collider;
                    return;
                }
            }

            // Если стена сбоку закончилась (угол/край стены) — мгновенно открепляемся и летим по прямой/дуге
            lastWall = currentWall;
            slideDetachTimer = 0.2f;
            StopWallRun();
            return;
        }

        // Если мы еще не на стене — ищем ближайшую стену вокруг
        int hitCount = Physics.OverlapSphereNonAlloc(origin, currentRadius, wallBuffer, wallLayer);
        if (hitCount == 0) return;

        Collider chosenWall = null;
        float closestDist = float.MaxValue;

        for (int i = 0; i < hitCount; i++)
        {
            Collider wallCollider = wallBuffer[i];
            if (wallCollider == null) continue;
            if (wallCollider == lastWall) continue;

            Vector3 pt = wallCollider.ClosestPoint(origin);
            Vector3 dir = pt - origin;
            float distSqr = dir.sqrMagnitude;
            Vector3 dirNorm = dir.normalized;

            if (Mathf.Abs(Vector3.Dot(dirNorm, Vector3.up)) < 0.3f)
            {
                if (distSqr < closestDist)
                {
                    closestDist = distSqr;
                    chosenWall = wallCollider;
                }
            }
        }

        if (chosenWall != null)
        {
            Vector3 closestPoint = chosenWall.ClosestPoint(origin);
            Vector3 directionToWall = (closestPoint - origin).normalized;
            if (Mathf.Abs(Vector3.Dot(directionToWall, Vector3.up)) < 0.3f)
            {
                if (Physics.Raycast(origin - directionToWall * 0.1f, directionToWall, out RaycastHit hit, currentRadius + 0.6f, wallLayer))
                {
                    wallNormal = hit.normal;
                    wallFaceNormal = hit.normal;
                    currentWall = chosenWall;

                    if (CanStartWallRun(chosenWall, hit.normal))
                    {
                        StartWallRun();
                    }
                }
            }
        }
    }
    
    private bool CanStartWallRun(Collider wall, Vector3 normal)
    {
        if (pc.grounded || slideDetachTimer > 0f || wallJumpTimer > 0f) return false;

        float vertical = Input.GetAxisRaw("Vertical");
        float horizontal = Input.GetAxisRaw("Horizontal");
        
        Vector3 camFwd = cam != null ? cam.forward : transform.forward;
        Vector3 camRight = cam != null ? cam.right : transform.right;
        Vector3 inputDir = (camFwd * vertical + camRight * horizontal);
        inputDir.y = 0f;

        // Не цепляемся, если игрок не нажимает кнопки движения
        if (inputDir.sqrMagnitude < 0.01f)
        {
            return false;
        }

        Vector3 wallTangent = Vector3.Cross(normal, Vector3.up).normalized;
        Vector3 lookDir = cam != null ? cam.forward : transform.forward;
        lookDir.y = 0f;
        lookDir.Normalize();

        Vector3 forwardAlongWall = Vector3.Dot(lookDir, wallTangent) >= 0f ? wallTangent : -wallTangent;
        float forwardInput = Vector3.Dot(inputDir.normalized, forwardAlongWall);

        // Цепляемся только если есть направленное движение вперед вдоль стены
        if (forwardInput <= 0.1f)
        {
            return false;
        }

        return true;
    }

    private void StartWallRun()
    {
        isWallRunning = true;
        wallTimer = 0f;

        wallForward = GetWallForward();
        float speedInWallDirection = Vector3.Dot(pc.horizontalVelocity, wallForward);
        currentWallRunSpeed = Mathf.Clamp(speedInWallDirection, 0f, wallRunSpeed);
        if (currentWallRunSpeed < 2f && Input.GetAxisRaw("Vertical") > 0.1f)
        {
            currentWallRunSpeed = Mathf.Max(currentWallRunSpeed, 2f);
        }

        pc.SetWallRunning(true);
    }

    private void StopWallRun()
    {
        if (isWallRunning)
        {
            // Сохраняем чистый горизонтальный импульс бега вперед (без боковых сил и толчков)
            Vector3 cleanForward = wallForward * currentWallRunSpeed;
            pc.horizontalVelocity = new Vector3(cleanForward.x, 0f, cleanForward.z);
        }

        isWallRunning = false;
        isActivelyWallRunning = false;
        wallTimer = 0f;
        currentWallRunSpeed = 0f;
        pc.SetWallRunning(false);
    }

    private void DetachWithSlide()
    {
        slideDetachTimer = slideDetachCooldown;
        lastWall = currentWall;
        StopWallRun();
    }

    private Vector3 GetWallForward()
    {
        Vector3 wallTangent = Vector3.Cross(wallFaceNormal, Vector3.up).normalized;
        Vector3 lookDir = cam != null ? cam.forward : transform.forward;
        lookDir.y = 0f;
        lookDir.Normalize();

        if (Vector3.Dot(lookDir, wallTangent) < 0f)
        {
            return -wallTangent;
        }
        return wallTangent;
    }

    private void HandleWallRun()
    {
        wallTimer += Time.deltaTime;

        float totalWallTime = idealGripDuration + slipDuration;
        if (wallTimer > totalWallTime)
        {
            lastWall = currentWall;
            StopWallRun();
            return;
        }

        if (wallTimer <= idealGripDuration)
        {
            pc.verticalVelocity = 0f;
        }
        else
        {
            float slipProgress = (wallTimer - idealGripDuration) / slipDuration;
            pc.verticalVelocity = -Mathf.Lerp(0f, maxSlipSpeed, slipProgress);
        }

        float vertical = Input.GetAxisRaw("Vertical");
        float horizontal = Input.GetAxisRaw("Horizontal");
        
        Vector3 camFwd = cam != null ? cam.forward : transform.forward;
        Vector3 camRight = cam != null ? cam.right : transform.right;
        Vector3 inputDir = (camFwd * vertical + camRight * horizontal);
        inputDir.y = 0f;

        wallForward = GetWallForward();

        // Проверка препятствия/угла впереди: если уперлись в перпендикулярную стену, сбрасываем бег
        Vector3 rayOrigin = transform.position;
        float playerRadius = pc.controller != null ? pc.controller.radius : 0.5f;
        float forwardCheckDist = playerRadius + 0.3f;
        
        if (Physics.SphereCast(rayOrigin, 0.15f, wallForward, out RaycastHit forwardHit, forwardCheckDist, wallLayer))
        {
            if (Vector3.Dot(forwardHit.normal, wallForward) < -0.2f)
            {
                // Достигли тупика/внутреннего угла — открепляемся и падаем прямо
                lastWall = currentWall;
                slideDetachTimer = 0.2f;
                currentWallRunSpeed = 0f;
                StopWallRun();
                return;
            }
        }

        if (inputDir.sqrMagnitude > 0.01f)
        {
            inputDir.Normalize();
            float forwardInput = Vector3.Dot(inputDir, wallForward);

            if (forwardInput > 0.1f)
            {
                // Бег вперед по направлению взгляда
                float targetSpeed = wallRunSpeed * Mathf.Clamp01(forwardInput);
                currentWallRunSpeed = Mathf.MoveTowards(currentWallRunSpeed, targetSpeed, wallRunAcceleration * Time.deltaTime);
            }
            else if (forwardInput < -0.1f)
            {
                // Нажатие S (назад) — плавное торможение
                currentWallRunSpeed = Mathf.MoveTowards(currentWallRunSpeed, 0f, wallRunBrakeDeceleration * Time.deltaTime);
                if (currentWallRunSpeed <= 0.1f)
                {
                    currentWallRunSpeed = 0f;
                    lastWall = currentWall;
                    slideDetachTimer = 0.2f;
                    StopWallRun();
                    return;
                }
            }
            else
            {
                // Вращение камеры / боковой ввод — плавное замедление без мгновенного срыва
                currentWallRunSpeed = Mathf.MoveTowards(currentWallRunSpeed, 0f, wallRunDeceleration * Time.deltaTime);
            }
        }
        else
        {
            // Нет ввода — плавное замедление до остановки
            currentWallRunSpeed = Mathf.MoveTowards(currentWallRunSpeed, 0f, wallRunDeceleration * Time.deltaTime);
            if (currentWallRunSpeed <= 0.1f)
            {
                currentWallRunSpeed = 0f;
                lastWall = currentWall;
                slideDetachTimer = 0.2f;
                StopWallRun();
                return;
            }
        }

        currentWallRunSpeed = Mathf.Max(currentWallRunSpeed, 0f);
        isActivelyWallRunning = currentWallRunSpeed > 0.5f;

        // Чистое движение вдоль стены
        Vector3 forwardVelocity = wallForward * currentWallRunSpeed;
        pc.horizontalVelocity = new Vector3(forwardVelocity.x, 0f, forwardVelocity.z);
    }

    private void WallJump()
    {
        Vector3 lookDir = cam != null ? cam.forward : transform.forward;
        lookDir.y = 0f;
        lookDir.Normalize();
        
        pc.horizontalVelocity = lookDir * wallJumpForce;
        pc.verticalVelocity = wallJumpHeight;
        
        wallJumpTimer = wallJumpCooldown;
        lastWall = currentWall;
        
        pc.SetWallJumped();
        
        isWallRunning = false;
        isActivelyWallRunning = false;
        wallTimer = 0f;
        currentWallRunSpeed = 0f;
        pc.SetWallRunning(false);

        if (jumpFallSounds != null)
        {
            jumpFallSounds.CallJumpSoundMethod();
        }
    }
    
    private void ApplyWallTilt()
    {
        float wallSide = Vector3.Dot(transform.right, wallNormal);
        float targetTilt = wallSide > 0 ? -wallTiltAngle : wallTiltAngle;
        
        currentTilt = Mathf.Lerp(currentTilt, targetTilt, tiltSpeed * Time.deltaTime);
        
        Vector3 rotation = transform.localEulerAngles;
        rotation.x = 0f;  
        rotation.z = currentTilt;  
        transform.localEulerAngles = rotation;
    }
    
    private void ResetTilt()
    {
        if (Mathf.Abs(currentTilt) < 0.01f)
        {
            currentTilt = 0f;
        }
        else
        {
            currentTilt = Mathf.Lerp(currentTilt, 0f, tiltSpeed * Time.deltaTime);
        }
        
        if (Mathf.Abs(currentTilt) > 0.001f || Mathf.Abs(transform.localEulerAngles.z) > 0.001f)
        {
            Vector3 rotation = transform.localEulerAngles;
            rotation.x = 0f;  
            rotation.z = currentTilt;  
            transform.localEulerAngles = rotation;
        }
    }
    
    private void OnDrawGizmos()
    {
        Vector3 origin = transform.position;
        Collider[] wallColliders = Physics.OverlapSphere(origin, sphereRadius, wallLayer);
        bool wallDetected = wallColliders.Length > 0;
        Gizmos.color = wallDetected ? Color.red : Color.gray;
        Gizmos.DrawWireSphere(origin, sphereRadius);
    }

    public void SetWallRunningKey(KeyCode newKey)
    {
        jumpKey = newKey;
    }

    public void SetSlideKey(KeyCode newKey)
    {
        slideKey = newKey;
    }
}