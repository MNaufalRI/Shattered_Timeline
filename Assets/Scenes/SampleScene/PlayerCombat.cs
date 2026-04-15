using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombat : MonoBehaviour
{
    [Header("References")]
    [SerializeField] GameObject activeChar;
    [SerializeField] PlayerControlled movement;   

    [Header("Attack Settings")]
    [SerializeField] float attackCooldown = 0.8f;    
    [SerializeField] float comboResetTime = 1.2f;    
    [SerializeField] int maxComboSteps = 3;      

    [Header("Defend Settings")]
    [SerializeField] float defendStaminaCostPerSec = 15f;  

    [Header("Air Attack")]
    [SerializeField] bool canAirAttack = true;

    // ─── State ───────────────────────────────
    private Animator anim;
    private int comboStep = 0;
    private float lastAttackTime = -99f;
    private float lastInputTime = -99f;
    private bool isAttacking = false;
    private bool isDefending = false;

    private readonly string[] comboTriggers = { "Attack01", "Attack02Start", "Attack03Start" };

    void Start()
    {
        anim = activeChar.GetComponent<Animator>();
        if (movement == null)
            movement = GetComponent<PlayerControlled>();
    }

    void Update()
    {
        HandleDefend();
        HandleAttackInput();
        HandleJumpAttack();
        CheckComboReset();
      
    }

    void HandleDefend()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        bool wantsDefend = keyboard.jKey.isPressed;
        bool hasStamina = movement.CurrentStamina > 0f;

     

        if (wantsDefend && hasStamina && !isAttacking)
        {
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

        bool attackPressed = keyboard.zKey.wasPressedThisFrame;
        if (!attackPressed) return;

        if (isDefending) return;

        float now = Time.time;

        if (now - lastAttackTime < attackCooldown) return;

        if (now - lastInputTime > comboResetTime)
            comboStep = 0;

        anim.SetInteger("ComboStep", comboStep);
        anim.SetTrigger("DoAttack");
        anim.Play("BattleRunForward");

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

        if (keyboard.zKey.wasPressedThisFrame && !movement.IsGrounded())
        {
            anim.Play("JumpAirAttack");
            isAttacking = true;
        }
    }


    void CheckComboReset()
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



    public bool IsAttacking() => isAttacking;
    public bool IsDefending() => isDefending;
    public int CurrentCombo() => comboStep;
}