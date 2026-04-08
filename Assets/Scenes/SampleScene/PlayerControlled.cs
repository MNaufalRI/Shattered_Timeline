using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerControlled : MonoBehaviour
{
    [SerializeField] CharacterController controller;
    [SerializeField] float speed = 5f;
    [SerializeField] float turnSpeed = 10f;
    [SerializeField] float gravityValue = -20f;
    [SerializeField] float jumpHeight = 2f;
    [SerializeField] GameObject activeChar;

    private Vector3 playerVelocity;
    private bool groundedPlayer;
    private Animator anim;

    void Start()
    {
        anim = activeChar.GetComponent<Animator>();
        controller.minMoveDistance = 0.001f;
    }

    void Update()
    {
        groundedPlayer = controller.isGrounded;
        if (groundedPlayer && playerVelocity.y < 0)
        {
            playerVelocity.y = -2f;
        }

        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        float moveHorizontal = 0;
        if (keyboard.aKey.isPressed) moveHorizontal = -1;
        else if (keyboard.dKey.isPressed) moveHorizontal = 1;

        float moveVertical = 0;
        if (keyboard.wKey.isPressed) moveVertical = 1;
        else if (keyboard.sKey.isPressed) moveVertical = -1;

        Vector3 moveDirection = new Vector3(moveHorizontal, 0, moveVertical).normalized;

        if (moveDirection.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(moveDirection.x, moveDirection.z) * Mathf.Rad2Deg;
            Quaternion targetRotation = Quaternion.Euler(0, targetAngle, 0);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);

            controller.Move(moveDirection * speed * Time.deltaTime);
        }

        if (keyboard.spaceKey.wasPressedThisFrame && groundedPlayer)
        {
            playerVelocity.y = Mathf.Sqrt(jumpHeight * -3.0f * gravityValue);
            anim.Play("JumpStart");
        }

        playerVelocity.y += gravityValue * Time.deltaTime;
        controller.Move(playerVelocity * Time.deltaTime);

        UpdateAnimation(moveDirection);
    }

    void UpdateAnimation(Vector3 moveDirection)
    {
        if (!groundedPlayer)
        {
            if (!anim.GetCurrentAnimatorStateInfo(0).IsName("JumpStart"))
            {
                anim.Play("JumpAir");
            }
        }
        else
        {
            if (moveDirection.magnitude >= 0.1f)
            {
                anim.Play("WalkForward");
            }
            else
            {
                anim.Play("Idle01");
            }
        }
    }
}