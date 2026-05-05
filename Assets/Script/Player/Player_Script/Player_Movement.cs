using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] CharacterController controller;
    [SerializeField] Transform cameraTransform;

    [Header("Movement")]
    [SerializeField] float walkSpeed = 6f;
    [SerializeField] float sprintSpeed = 10f;
    [SerializeField] float acceleration = 10f;
    [SerializeField] float turnSpeed = 15f;

    [Header("Gravity")]
    [SerializeField] float gravityValue = -20f;

    [Header("Stamina")]
    [SerializeField] float maxStamina = 100f;
    [SerializeField] float staminaDrainRate = 20f;
    [SerializeField] float staminaRegenRate = 10f;
    [SerializeField] float staminaRegenDelay = 1.5f;

    private Vector3 velocity;
    private Vector3 moveVelocity;

    private bool grounded;
    private bool isSprinting;

    private float stamina;
    private float regenTimer;

    public bool canControl = true;

    void Start()
    {
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

        ApplyMovement(moveDir);
        ApplyRotation(moveDir);
        ApplyGravity();
    }

    Vector3 GetMovementDirection()
    {
        var kb = Keyboard.current;
        if (kb == null) return Vector3.zero;

        float h = (kb.aKey.isPressed ? -1 : 0) + (kb.dKey.isPressed ? 1 : 0);
        float v = (kb.wKey.isPressed ? 1 : 0) + (kb.sKey.isPressed ? -1 : 0);

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
        if (dir.magnitude < 0.1f) return;

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
            grounded;

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
}