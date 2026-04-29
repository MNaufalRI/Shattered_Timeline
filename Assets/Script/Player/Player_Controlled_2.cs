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

    public Animator anim;

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
        anim = GetComponent<Animator>();

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
            ApplyGravity();
            return;
        }

        if (stats != null && stats.IsDead())
        {
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

    void UpdateAnimation(Vector3 moveDirection)
    {
        if (stats.IsDead() || !canControl || isAttacking || isDefending) return;

        var stateInfo = anim.GetCurrentAnimatorStateInfo(0);

        if (stateInfo.IsName("GetHit") && stateInfo.normalizedTime < 0.8f)
        {
            return;
        }

        if (moveDirection.magnitude >= 0.1f)
        {
            string targetAnim = isSprinting ? "BattleRunForward" : "WalkForward";
            if (!stateInfo.IsName(targetAnim))
                anim.Play(targetAnim);
        }
        else
        {
            if (!stateInfo.IsName("Idle01"))
                anim.Play("Idle01");
        }
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
                eSimp.TakeDamage(stats.attackDamage);
            }
        }
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
        if (isAttacking || isDefending) return;

        float targetSpeed = (moveDirection.magnitude >= 0.1f) ? (isSprinting ? sprintSpeed : walkSpeed) : 0f;
        float currentSpeed = Mathf.MoveTowards(currentMoveVelocity.magnitude, targetSpeed, acceleration * Time.deltaTime);
        currentMoveVelocity = moveDirection.normalized * currentSpeed;

        controller.Move(currentMoveVelocity * Time.deltaTime);
    }

    void ApplyRotation(Vector3 moveDirection)
    {
        if (moveDirection.magnitude < 0.1f || isAttacking || isDefending) return;
        Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
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
        if (wantsDefend && CurrentStamina > 0f && !isAttacking)
        {
            isDefending = true;
            CurrentStamina -= defendStaminaCostPerSec * Time.deltaTime;
            anim.Play(anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.9f ? "DefendMaintain" : "DefendStart");
        }
        else if (isDefending)
        {
            isDefending = false;
            anim.Play("Idle01");
        }
    }

    void HandleAttackInput()
    {
        if (Keyboard.current.spaceKey.wasPressedThisFrame && !isDefending)
        {
            float now = Time.time;
            if (now - lastAttackStartTime < minTimeBetweenCombos) return;
            if (now - lastInputTime > comboResetTime) comboStep = 0;

            if (isAttacking) { pendingCombo = true; lastInputTime = now; return; }
            ExecuteComboStep(now);
        }
    }

    void ExecuteComboStep(float now)
    {
        if (comboStep >= maxComboSteps) comboStep = 0;
        anim.Play(comboStart[comboStep]);
        isTransitioning = isAttacking = true;
        pendingCombo = false;
        lastInputTime = lastAttackStartTime = now;
        comboStep++;
    }

    void CheckAttackFinished()
    {
        if (!isAttacking || isTransitioning) { isTransitioning = false; return; }
        var info = anim.GetCurrentAnimatorStateInfo(0);
        if (!info.IsName("Attack01") && !info.IsName("Attack02") && !info.IsName("Attack03")) { isAttacking = false; return; }
        if (info.normalizedTime >= 0.85f)
        {
            if (pendingCombo) ExecuteComboStep(Time.time);
            else isAttacking = false;
        }
    }
}