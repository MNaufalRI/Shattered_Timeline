using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

public class PlayerControlled : MonoBehaviour
{
    [Header("References")]
    [SerializeField] CharacterController controller;
    [SerializeField] GameObject activeChar;
    [SerializeField] Transform cameraTransform;

    [Header("Movement")]
    [SerializeField] float walkSpeed = 6f;
    [SerializeField] float sprintSpeed = 10f;
    [SerializeField] float acceleration = 10f;
    [SerializeField] float deceleration = 15f;
    [SerializeField] float turnSpeed = 15f;

    [Header("Stamina")]
    [SerializeField] float maxStamina = 100f;
    [SerializeField] float staminaDrainRate = 20f;
    [SerializeField] float staminaRegenRate = 10f;
    [SerializeField] float staminaRegenDelay = 1.5f;
    [SerializeField] float defendStaminaCostPerSec = 15f;
    public float CurrentStamina { get; private set; }
    public float MaxStamina => maxStamina;

    [Header("Jump & Gravity")]
    [SerializeField] float gravityValue = -20f;
    [SerializeField] float jumpHeight = 2f;

    [Header("Combat")]
    [SerializeField] float attackCooldown = 0.8f;
    [SerializeField] float comboResetTime = 1.2f;
    [SerializeField] int maxComboSteps = 3;
    [SerializeField] bool canAirAttack = true;

    private Vector3 playerVelocity;
    private Vector3 currentMoveVelocity;
    private bool groundedPlayer;
    private bool isSprinting;
    private float staminaRegenTimer;
    private Animator anim;

    private int comboStep = 0;
    private float lastAttackTime = -99f;
    private float lastInputTime = -99f;
    private bool isAttacking = false;
    private bool isDefending = false;

    private readonly string[] comboAnims = { "Attack01", "Attack02Start", "Attack03Start", "Attack04" };


    void Start()
    {
        anim = activeChar.GetComponent<Animator>();
        controller.minMoveDistance = 0.001f;
        CurrentStamina = maxStamina;

        if (cameraTransform == null)
            cameraTransform = Camera.main.transform;
    }

    void Update()
    {
        groundedPlayer = controller.isGrounded;
        if (groundedPlayer && playerVelocity.y < 0)
            playerVelocity.y = -2f;

        Vector3 moveDirection = GetMovementDirection();

        HandleSprint(moveDirection);
        HandleDefend();
        HandleAttackInput();
        HandleJumpAttack();
        CheckAttackFinished();

        ApplyMovement(moveDirection);
        ApplyRotation(moveDirection);
        ApplyJumpAndGravity();
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
            currentMoveVelocity = Vector3.MoveTowards(
                currentMoveVelocity, Vector3.zero, deceleration * Time.deltaTime);
            controller.Move(currentMoveVelocity * Time.deltaTime);
            return;
        }

        float currentSpeed = isSprinting ? sprintSpeed : walkSpeed;
        Vector3 targetVelocity = moveDirection * currentSpeed;
        float rate = moveDirection.magnitude >= 0.1f ? acceleration : deceleration;

        currentMoveVelocity = Vector3.MoveTowards(
            currentMoveVelocity, targetVelocity, rate * Time.deltaTime);

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

    void ApplyJumpAndGravity()
    {
        var keyboard = Keyboard.current;
        if (keyboard != null &&
            keyboard.spaceKey.wasPressedThisFrame &&
            groundedPlayer &&
            !isAttacking &&
            !isDefending)
        {
            playerVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravityValue);
            anim.Play("JumpStart");
        }

        playerVelocity.y += gravityValue * Time.deltaTime;
        controller.Move(playerVelocity * Time.deltaTime);
    }

    void UpdateAnimation(Vector3 moveDirection)
    {
        if (isAttacking || isDefending) return;

        if (!groundedPlayer)
        {
            var stateInfo = anim.GetCurrentAnimatorStateInfo(0);
            if (!stateInfo.IsName("JumpStart") && !stateInfo.IsName("JumpAir"))
                anim.Play("JumpAir");
        }
        else if (moveDirection.magnitude >= 0.1f)
        {
            anim.Play(isSprinting ? "BattleRunForward" : "WalkForward");
        }
        else
        {
            anim.Play("Idle01");
        }
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

        if (!keyboard.zKey.wasPressedThisFrame) return;
        if (isDefending) return;

        float now = Time.time;
        if (now - lastAttackTime < attackCooldown) return;

        if (now - lastInputTime > comboResetTime)
            comboStep = 0;

        string animName = comboAnims[comboStep % maxComboSteps];
        anim.Play(animName);

        isAttacking = true;
        lastAttackTime = now;
        lastInputTime = now;
        comboStep++;

        if (comboStep >= maxComboSteps)
            comboStep = 0;
    }

    void HandleJumpAttack()
    {
        if (!canAirAttack) return;

        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        if (keyboard.zKey.wasPressedThisFrame && !groundedPlayer)
        {
            anim.Play("JumpAirAttack");
            isAttacking = true;
        }
    }

    void CheckAttackFinished()
    {
        if (!isAttacking) return;

        var info = anim.GetCurrentAnimatorStateInfo(0);
        bool inAttackAnim = info.IsName("Attack01") ||
                            info.IsName("Attack02Start") ||
                            info.IsName("Attack02Maintain") ||
                            info.IsName("Attack03Start") ||
                            info.IsName("Attack04") ||
                            info.IsName("JumpAirAttack");

        if (inAttackAnim && info.normalizedTime >= 0.85f)
            isAttacking = false;
    }

    public bool IsGrounded() => groundedPlayer;
    public bool IsSprinting() => isSprinting;
    public bool IsAttacking() => isAttacking;
    public bool IsDefending() => isDefending;
}