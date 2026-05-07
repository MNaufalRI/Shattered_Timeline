using UnityEngine;
using UnityEngine.InputSystem;

public class Player_Controlled_3 : MonoBehaviour
{
    [Header("References")]
    [SerializeField] CharacterController controller;
    [SerializeField] Transform cameraTransform;

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

    [Header("Gravity")]
    [SerializeField] float gravityValue = -20f;

    [Header("Combat")]
    [SerializeField] float comboResetTime = 1.5f;
    [SerializeField] float minTimeBetweenCombos = 0.2f;

    [Header("Melee")]
    [SerializeField] float attackRange = 2f;
    [SerializeField] LayerMask enemyLayer;

    [Header("Dash")]
    [SerializeField] float dashSpeed = 15f;
    [SerializeField] float dashDuration = 0.2f;
    [SerializeField] float dashCooldown = 1f;

    [SerializeField] float attackCooldown = 0.2f;
    float attackTimer = 0f;
    [SerializeField] float maxAttackDuration = 1f;


    private float nextAttackTime = 0f;
    private Quaternion lockedRotation;

    private bool isDashing = false;
    private float dashTime;
    private float dashCooldownTimer;
    private Vector3 dashDirection;

    private Animator anim;
    private PlayerStats stats;

    private Vector3 velocity;
    private Vector3 moveVelocity;

    private bool grounded;
    private bool isSprinting;
    private bool isAttacking;
    private bool isDefending;
    private GatherableItem currentGatherItem;

    private bool pendingCombo = false; 

    private float stamina;
    private float regenTimer;

    private int comboStep = 0;
    private float lastAttackTime = -99f;
    private float lastInputTime = -99f;

    public bool canControl = true;
    float attackDurationTimer = 0f;


    void Start()
    {
        anim = GetComponent<Animator>();
        stats = GetComponent<PlayerStats>();

        canControl = true;
        stamina = maxStamina;

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

        grounded = controller.isGrounded;
        if (grounded && velocity.y < 0)
            velocity.y = -2f;

        Vector3 moveDir = GetMovementDirection();

        HandleSprint(moveDir);
        HandleDefend();
        HandleDash();
        HandleAttack();

        ApplyMovement(moveDir);
        ApplyRotation(moveDir);
        ApplyGravity();

        UpdateAnimation(moveDir);

        if (isAttacking)
        {
            attackDurationTimer -= Time.deltaTime;

            if (attackDurationTimer <= 0f)
            {
                EndAttack();
            }
        }
    }

    bool IsInAttackAnimation()
    {
        AnimatorStateInfo state = anim.GetCurrentAnimatorStateInfo(0);
        return state.IsTag("Attack");
    }

    public void SetGathering(bool state)
    {
        if (anim != null)
        {
            anim.SetBool("isGathering", state);
        }
    }

    public void StartGathering(GatherableItem item)
    {
        currentGatherItem = item;

        canControl = false;
        SetGathering(true);
    }



    void HandleDash()
    {
        var kb = Keyboard.current;

        if (kb.qKey.wasPressedThisFrame && dashCooldownTimer <= 0f && !isDashing)
        {
            StartDash();
        }

        if (dashCooldownTimer > 0f)
            dashCooldownTimer -= Time.deltaTime;
    }

    void StartDash()
    {
        isDashing = true;
        dashTime = dashDuration;
        dashCooldownTimer = dashCooldown;

        dashDirection = transform.forward;

        if (stats != null)
            stats.isInvincible = true;

        anim.SetTrigger("Dash");
    }


    Vector3 GetMovementDirection()
    {
        var kb = Keyboard.current;
        if (kb == null) return Vector3.zero;

        float h = (kb.aKey.isPressed ? -1 : 0) + (kb.dKey.isPressed ? 1 : 0);
        float v = (kb.wKey.isPressed ? 1 : 0) + (kb.sKey.isPressed ? -1 : 0);

        if (h == 0 && v == 0) return Vector3.zero;
        if (isAttacking)
            return Vector3.zero;

        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;

        forward.y = 0;
        right.y = 0;

        forward.Normalize();
        right.Normalize();

        return (forward * v + right * h).normalized;
    }

    void ApplyMovement(Vector3 dir)
    {
        if (isDashing)
        {
            controller.Move(dashDirection * dashSpeed * Time.deltaTime);

            dashTime -= Time.deltaTime;

            if (dashTime <= 0f)
            {
                isDashing = false;

                if (stats != null)
                    stats.isInvincible = false;
            }

            return;
        }

        if (isAttacking || isDefending)
        {
            moveVelocity = Vector3.zero;
            return;
        }

        float targetSpeed = dir.magnitude > 0.1f
            ? (isSprinting ? sprintSpeed : walkSpeed)
            : 0f;

        float currentSpeed = Mathf.MoveTowards(
            moveVelocity.magnitude,
            targetSpeed,
            acceleration * Time.deltaTime
        );

        moveVelocity = dir * currentSpeed;
        controller.Move(moveVelocity * Time.deltaTime);
    }

    void ApplyRotation(Vector3 dir)
    {
        if (dir.magnitude < 0.1f || isAttacking || isDefending) return;

        Quaternion targetRot = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRot,
            turnSpeed * Time.deltaTime
        );
    }

    void ApplyGravity()
    {
        velocity.y += gravityValue * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }


    void HandleSprint(Vector3 dir)
    {
        var kb = Keyboard.current;

        bool wantsSprint =
            kb.leftShiftKey.isPressed &&
            dir.magnitude > 0.1f &&
            grounded &&
            !isAttacking &&
            !isDefending;

        if (wantsSprint && stamina > 0)
        {
            isSprinting = true;
            stamina -= staminaDrainRate * Time.deltaTime;
            regenTimer = 0;
        }
        else
        {
            isSprinting = false;
            regenTimer += Time.deltaTime;

            if (regenTimer >= staminaRegenDelay)
            {
                stamina += staminaRegenRate * Time.deltaTime;
                stamina = Mathf.Min(stamina, maxStamina);
            }
        }
    }


    void HandleDefend()
    {
        var kb = Keyboard.current;

        bool wantsDefend = kb.jKey.isPressed;

        if (wantsDefend && stamina > 0 && !isAttacking)
        {
            isDefending = true;
            stamina -= defendStaminaCostPerSec * Time.deltaTime;
        }
        else
        {
            isDefending = false;
        }

        anim.SetBool("isDefending", isDefending);
    }

    void HandleAttack()
    {
        var kb = Keyboard.current;

        if (!kb.spaceKey.wasPressedThisFrame || isDefending || isDashing)
            return;

        float now = Time.time;

        if (now < nextAttackTime)
            return;

        if (now - lastInputTime > comboResetTime)
            comboStep = 0;

        lastInputTime = now;

        if (isAttacking)
        {
            pendingCombo = true;
            return;
        }

        StartAttack();

        nextAttackTime = now + attackCooldown;
    }

    void StartAttack()
    {
        anim.SetTrigger("Attack");
        anim.SetInteger("ComboStep", comboStep);

        anim.SetBool("canBeInterrupted", false);

        isAttacking = true;
        attackDurationTimer = maxAttackDuration; 

        lockedRotation = transform.rotation;
        moveVelocity = Vector3.zero;

        comboStep++;

        if (comboStep >= 3)
            comboStep = 0;
    }

    public void CheckCombo()
    {
        if (pendingCombo)
        {
            pendingCombo = false;
            StartAttack();
        }
    }

    public void EndAttack()
    {
        isAttacking = false;
        pendingCombo = false;

        anim.SetBool("canBeInterrupted", true);
    }



    void UpdateAnimation(Vector3 dir)
    {
        float speed = moveVelocity.magnitude / sprintSpeed;

        anim.SetFloat("Speed", speed, 0.1f, Time.deltaTime);
        anim.SetBool("isMoving", speed > 0.1f);
        anim.SetBool("isSprinting", isSprinting);
        anim.SetBool("isAttacking", isAttacking);
    }
}

