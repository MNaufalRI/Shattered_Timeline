using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombat : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Animator anim;
    [SerializeField] PlayerMovement movement;
    [SerializeField] PlayerStats stats;

    [Header("Combat")]
    [SerializeField] float attackRange = 2f;
    [SerializeField] LayerMask enemyLayer;
    [SerializeField] float attackCooldown = 0.2f;
    [SerializeField] float comboResetTime = 1.5f;
    [SerializeField] float maxAttackDuration = 1f;

    private float nextAttackTime;
    private float lastInputTime;

    private bool isAttacking;
    private bool pendingCombo;

    private int comboStep = 0;
    private float attackTimer;

    void Update()
    {
        HandleAttack();

        if (isAttacking)
        {
            attackTimer -= Time.deltaTime;

            if (attackTimer <= 0f)
                EndAttack();
        }
    }

    void HandleAttack()
    {
        var kb = Keyboard.current;

        if (!kb.spaceKey.wasPressedThisFrame)
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
        isAttacking = true;
        attackTimer = maxAttackDuration;

        anim.SetTrigger("Attack");
        anim.SetInteger("ComboStep", comboStep);

        movement.canControl = false;

        comboStep++;
        if (comboStep >= 3) comboStep = 0;
    }

    public void EndAttack()
    {
        isAttacking = false;
        pendingCombo = false;

        movement.canControl = true;
    }

    public void CheckCombo()
    {
        if (pendingCombo)
        {
            pendingCombo = false;
            StartAttack();
        }
    }

    public void Hit()
    {
        Collider[] enemies = Physics.OverlapSphere(
            transform.position + transform.forward,
            attackRange,
            enemyLayer
        );

        foreach (var enemy in enemies)
        {
            if (enemy.TryGetComponent<EnemySimple>(out var e))
            {
                e.TakeDamage(stats.attackDamage);
            }
        }
    }
}