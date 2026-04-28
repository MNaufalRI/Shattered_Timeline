using UnityEngine;
using UnityEngine.InputSystem;

public class Player_Controlled_2 : MonoBehaviour
{
    [Header("References")]
    [SerializeField] CharacterController controller;
    [SerializeField] GameObject activeChar;
    [SerializeField] Transform cameraTransform;

    [Header("Projectile Settings")]
    [SerializeField] GameObject projectilePrefab;
    [SerializeField] Transform projectileSpawnPoint;
    [SerializeField] float projectileSpeed = 20f;
    [SerializeField] float projectileLifeTime = 3f;

    [Header("Movement")]
    [SerializeField] float walkSpeed = 6f;
    [SerializeField] float sprintSpeed = 10f;
    [SerializeField] float acceleration = 10f;
    [SerializeField] float turnSpeed = 15f;

    [Header("Stamina")]
    [SerializeField] float maxStamina = 100f;
    [SerializeField] float staminaDrainRate = 20f;
    [SerializeField] float staminaRegenRate = 10f;
    [SerializeField] float staminaRegenDelay = 1.5f;
    [SerializeField] float defendStaminaCostPerSec = 15f;
    public float CurrentStamina { get; private set; }
    public float MaxStamina => maxStamina;

    [Header("Gravity")]
    [SerializeField] float gravityValue = -20f;

    [Header("Combat")]
    [SerializeField] float comboResetTime = 1.5f;
    [SerializeField] float minTimeBetweenCombos = 0.4f;

    [Header("Melee Settings")]
    [SerializeField] float attackRange = 2f; 
    [SerializeField] LayerMask enemyLayer;  
    private PlayerStats stats;

    private Vector3 playerVelocity;
    private Vector3 currentMoveVelocity;
    private bool groundedPlayer;
    private bool isSprinting;
    private float staminaRegenTimer;
    private Animator anim;
    private bool isTransitioning = false;

    private int comboStep = 0;
    private float lastInputTime = -99f;
    private float lastAttackStartTime = -99f;
    private bool isAttacking = false;
    private bool isDefending = false;
    private bool pendingCombo = false;
    public bool canControl = true;

    private static readonly string[] comboStart = {
        "Attack01",
        "Attack02",
        "Attack03",
    };
    private const int maxComboSteps = 3;

    void Start()
    {
        anim = activeChar.GetComponent<Animator>();
        controller.minMoveDistance = 0.001f;
        CurrentStamina = maxStamina;
        stats = GetComponent<PlayerStats>();

        if (cameraTransform == null)
            cameraTransform = Camera.main.transform;
    }

    void Update()
    {
        if (!canControl)
        {
            if (anim != null) anim.Play("Idle01");
            ApplyGravity();
            return;
        }

        groundedPlayer = controller.isGrounded;
        if (groundedPlayer && playerVelocity.y < 0)
            playerVelocity.y = -2f;

        Vector3 moveDirection = GetMovementDirection();

        HandleSprint(moveDirection);
        HandleDefend();
        HandleAttackInput();
        CheckAttackFinished();

        ApplyMovement(moveDirection);
        ApplyRotation(moveDirection);
        ApplyGravity();
        UpdateAnimation(moveDirection);
    }

    Vector3 GetMovementDirection()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return Vector3.zero;

        float h = 0f, v = 0f;
        if (keyboard.aKey.isPressed) h = -1f;
        else if (keyboard.dKey.isPressed) h = 1f;
        if (keyboard.wKey.isPressed) v = 1f;
        else if (keyboard.sKey.isPressed) v = -1f;

        if (h == 0f && v == 0f) return Vector3.zero;

        Vector3 camForward = cameraTransform.forward;
        Vector3 camRight = cameraTransform.right;
        camForward.y = 0f; camRight.y = 0f;
        camForward.Normalize(); camRight.Normalize();

        return (camForward * v + camRight * h).normalized;
    }

    public void Hit()
    {
        Collider[] hitEnemies = Physics.OverlapSphere(transform.position + transform.forward, attackRange, enemyLayer);

        foreach (Collider enemy in hitEnemies)
        {

            if (enemy.TryGetComponent<EnemySimple>(out EnemySimple eSimp))
            {
                Debug.Log("Berhasil memukul: " + enemy.name);

                eSimp.TakeDamage(stats.attackDamage);
            }
            else
            {
                Debug.Log(enemy.name + " terkena area hit tapi tidak punya skrip EnemySimple.");
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position + transform.forward, attackRange);
    }

    void HandleSprint(Vector3 moveDirection)
    {
        var keyboard = Keyboard.current;
        bool wantsToSprint = keyboard != null &&
                             keyboard.leftShiftKey.isPressed &&
                             moveDirection.magnitude >= 0.1f &&
                             groundedPlayer &&
                             !isAttacking &&
                             !isDefending;

        if (wantsToSprint && CurrentStamina > 0f)
        {
            isSprinting = true;
            CurrentStamina -= staminaDrainRate * Time.deltaTime;
            CurrentStamina = Mathf.Max(CurrentStamina, 0f);
            staminaRegenTimer = 0f;
        }
        else
        {
            isSprinting = false;
            staminaRegenTimer += Time.deltaTime;
            if (staminaRegenTimer >= staminaRegenDelay)
            {
                CurrentStamina += staminaRegenRate * Time.deltaTime;
                CurrentStamina = Mathf.Min(CurrentStamina, maxStamina);
            }
        }
    }

    void ApplyMovement(Vector3 moveDirection)
    {
        if (isAttacking || isDefending)
        {
            currentMoveVelocity = Vector3.zero;
            controller.Move(Vector3.zero);
            return;
        }

        float targetSpeed = (moveDirection.magnitude >= 0.1f) ? (isSprinting ? sprintSpeed : walkSpeed) : 0f;

        float currentSpeed = currentMoveVelocity.magnitude;

        currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, acceleration * Time.deltaTime);

        if (moveDirection.magnitude >= 0.1f)
        {
            currentMoveVelocity = moveDirection.normalized * currentSpeed;
        }
        else
        {
            currentMoveVelocity = currentMoveVelocity.normalized * currentSpeed;
        }

        controller.Move(currentMoveVelocity * Time.deltaTime);
    }

    void ApplyRotation(Vector3 moveDirection)
    {
        if (moveDirection.magnitude < 0.1f) return;
        if (isAttacking || isDefending) return;

        Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
        transform.rotation = Quaternion.Slerp(
            transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
    }

    void ApplyGravity()
    {
        playerVelocity.y += gravityValue * Time.deltaTime;
        controller.Move(playerVelocity * Time.deltaTime);
    }

    void HandleDefend()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        bool wantsDefend = keyboard.jKey.isPressed;
        bool hasStamina = CurrentStamina > 0f;

        if (wantsDefend && hasStamina && !isAttacking)
        {
            CurrentStamina -= defendStaminaCostPerSec * Time.deltaTime;
            CurrentStamina = Mathf.Max(0f, CurrentStamina);

            if (!isDefending)
            {
                isDefending = true;
                anim.Play("DefendStart");
            }
            else
            {
                var info = anim.GetCurrentAnimatorStateInfo(0);
                if (info.IsName("DefendStart") && info.normalizedTime >= 0.9f)
                    anim.Play("DefendMaintain");
            }
        }
        else
        {
            if (isDefending)
            {
                isDefending = false;
                anim.Play("Idle01");
            }
        }
    }

    void HandleAttackInput()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        bool attackPressed = keyboard.spaceKey.wasPressedThisFrame;

        if (!attackPressed) return;
        if (isDefending) return;

        float now = Time.time;

        if (now - lastAttackStartTime < minTimeBetweenCombos) return;

        if (now - lastInputTime > comboResetTime)
            comboStep = 0;

        if (isAttacking)
        {
            var info = anim.GetCurrentAnimatorStateInfo(0);

            if (info.normalizedTime < 0.4f) return;

            pendingCombo = true;
            lastInputTime = now;
            return;
        }

        ExecuteComboStep(now);
    }

    void ExecuteComboStep(float now)
    {
        if (comboStep >= maxComboSteps) comboStep = 0;

        anim.Play(comboStart[comboStep]);

        isTransitioning = true;
        isAttacking = true;
        pendingCombo = false;

        lastInputTime = now;
        lastAttackStartTime = now;

        comboStep++;
        if (comboStep >= maxComboSteps) comboStep = 0;
    }

    public void SpawnProjectile()
    {
        if (projectilePrefab == null || projectileSpawnPoint == null) return;

        GameObject go = Instantiate(projectilePrefab, projectileSpawnPoint.position, transform.rotation);
        Rigidbody rb = go.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.linearVelocity = transform.forward * projectileSpeed;
        }

        Destroy(go, projectileLifeTime);
    }

    void CheckAttackFinished()
    {
        if (!isAttacking) return;

        if (isTransitioning)
        {
            isTransitioning = false;
            return;
        }

        var info = anim.GetCurrentAnimatorStateInfo(0);

        bool inAttackAnim =
            info.IsName("Attack01") ||
            info.IsName("Attack02") ||
            info.IsName("Attack03");

        if (!inAttackAnim) { isAttacking = false; return; }

        if (info.normalizedTime >= 0.85f)
        {
            if (pendingCombo && (Time.time - lastInputTime) <= comboResetTime)
            {
                ExecuteComboStep(Time.time);
            }
            else
            {
                isAttacking = false;
            }
        }
    }

    void UpdateAnimation(Vector3 moveDirection)
    {
        if (isAttacking || isDefending) return;

        if (moveDirection.magnitude >= 0.1f)
            anim.Play(isSprinting ? "BattleRunForward" : "WalkForward");
        else
            anim.Play("Idle01");
    }

    public bool IsGrounded() => groundedPlayer;
    public bool IsSprinting() => isSprinting;
    public bool IsAttacking() => isAttacking;
    public bool IsDefending() => isDefending;
}