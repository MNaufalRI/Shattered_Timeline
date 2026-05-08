using System.Collections;
using UnityEngine;
using DG.Tweening;
using UnityEngine.InputSystem;
using StarterAssets;

public class PlayerControl : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Animator anim;
    [SerializeField] private PlayerMovement2 thirdPersonController;

    private PlayerStats playerStats;

    [Header("Combat")]
    public Transform target;
    [SerializeField] private Transform attackPos;
    [SerializeField] private float quickAttackDeltaDistance;
    [SerializeField] private float heavyAttackDeltaDistance;
    [SerializeField] private float attackRange = 1f;
    [SerializeField] private float reachTime = 0.3f;
    [SerializeField] private LayerMask enemyLayer;

    [Header("Auto Approach (Kejar Musuh)")]
    public float autoWalkSpeed = 5f;
    public float strikeRange = 1.5f;

    [Header("Skill Settings")]
    public float skill1ManaCost = 5f; // Konsumsi 5 mana
    public float skill2ManaCost = 10f; // Konsumsi 10 mana
    public float skill1Cooldown = 5f; // Cooldown 5 detik
    public float skill2Cooldown = 10f; // Cooldown 10 detik

    private float skill1Timer = 0f;
    private float skill2Timer = 0f;

    private bool isAttacking = false;
    private bool isApproaching = false;
    private Coroutine approachCoroutine;

    void Awake()
    {
        playerStats = GetComponent<PlayerStats>(); // Menghubungkan PlayerStats
    }

    void Update()
    {
        // Update timer cooldown setiap frame
        if (skill1Timer > 0) skill1Timer -= Time.deltaTime;
        if (skill2Timer > 0) skill2Timer -= Time.deltaTime;
    }

    // --- FUNGSI INPUT SEND MESSAGES ---

    public void OnMove(InputValue value)
    {
        Vector2 moveInput = value.Get<Vector2>();
        if (isApproaching && moveInput.sqrMagnitude > 0.05f)
        {
            CancelApproach();
        }
    }

    public void OnQuickAttack(InputValue value)
    {
        if (value.isPressed && playerStats != null && !playerStats.IsDead())
        {
            Attack(0);
        }
    }

    public void OnSkill1(InputValue value)
    {
        if (value.isPressed && playerStats != null && !playerStats.IsDead())
        {
            // Cek cooldown sebelum memulai serangan
            if (skill1Timer <= 0) Attack(1);
            else Debug.Log("Skill 1 sedang Cooldown: " + Mathf.Ceil(skill1Timer) + " detik");
        }
    }

    public void OnSkill2(InputValue value)
    {
        if (value.isPressed && playerStats != null && !playerStats.IsDead())
        {
            // Cek cooldown sebelum memulai serangan
            if (skill2Timer <= 0) Attack(2);
            else Debug.Log("Skill 2 sedang Cooldown: " + Mathf.Ceil(skill2Timer) + " detik");
        }
    }

    // ----------------------------------

    public void Attack(int attackState)
    {
        if (isApproaching && (attackState == 1 || attackState == 2))
        {
            CancelApproach();
        }

        if (isAttacking) return;

        // Cek apakah mana mencukupi sebelum masuk ke animasi
        if (attackState == 1 && playerStats.currentMana < skill1ManaCost) return;
        if (attackState == 2 && playerStats.currentMana < skill2ManaCost) return;

        if (thirdPersonController != null)
        {
            thirdPersonController.canMove = false;
            thirdPersonController.enabled = false;
        }

        ExecuteAttackAnim(attackState);
    }

    void ExecuteAttackAnim(int attackState)
    {
        if (attackState == 0)
            QuickAttack();
        else if (attackState == 1)
            Skill1();
        else if (attackState == 2)
            Skill2();
    }

    void QuickAttack()
    {
        if (target == null)
        {
            ResetAttack();
            return;
        }

        int attackIndex = Random.Range(1, 4);
        float distance = Vector3.Distance(transform.position, target.position);

        if (distance > strikeRange)
        {
            if (approachCoroutine != null) StopCoroutine(approachCoroutine);
            approachCoroutine = StartCoroutine(ApproachAndAttack(target, attackIndex));
        }
        else
        {
            ExecuteQuickAttackAnim(attackIndex);
        }
    }

    IEnumerator ApproachAndAttack(Transform targetNode, int attackIndex)
    {
        isAttacking = true;
        isApproaching = true;

        while (targetNode != null && Vector3.Distance(transform.position, targetNode.position) > strikeRange)
        {
            if (!isApproaching) yield break;
            FaceThis(targetNode.position);
            anim.SetFloat("Speed", autoWalkSpeed);
            anim.SetFloat("MotionSpeed", 1f);
            float step = autoWalkSpeed * Time.deltaTime;
            Vector3 targetPos = new Vector3(targetNode.position.x, transform.position.y, targetNode.position.z);
            transform.position = Vector3.MoveTowards(transform.position, targetPos, step);
            yield return null;
        }

        isApproaching = false;
        anim.SetFloat("Speed", 0f);

        if (targetNode != null)
        {
            ExecuteQuickAttackAnim(attackIndex);
        }
        else
        {
            ResetAttack();
        }
    }

    void CancelApproach()
    {
        isApproaching = false;
        anim.SetFloat("Speed", 0f);
        if (approachCoroutine != null) StopCoroutine(approachCoroutine);
        ResetAttack();
    }

    void ExecuteQuickAttackAnim(int attackIndex)
    {
        isAttacking = true;
        switch (attackIndex)
        {
            case 1: MoveTowardsTarget(target.position, quickAttackDeltaDistance, "punch"); break;
            case 2: MoveTowardsTarget(target.position, quickAttackDeltaDistance, "kick"); break;
            case 3: MoveTowardsTarget(target.position, quickAttackDeltaDistance, "mmakick"); break;
        }
    }

    void Skill1()
    {
        if (target == null) { ResetAttack(); return; }

        // Konsumsi Mana dan pasang Cooldown
        if (playerStats != null && playerStats.UseMana(skill1ManaCost))
        {
            skill1Timer = skill1Cooldown;
            FaceThis(target.position);
            anim.SetBool("heavyAttack1", true);
            isAttacking = true;
        }
        else
        {
            Debug.Log("Mana tidak cukup untuk Skill 1");
            ResetAttack();
        }
    }

    void Skill2()
    {
        if (target == null) { ResetAttack(); return; }

        // Konsumsi Mana dan pasang Cooldown
        if (playerStats != null && playerStats.UseMana(skill2ManaCost))
        {
            skill2Timer = skill2Cooldown;
            FaceThis(target.position);
            anim.SetBool("heavyAttack2", true);
            isAttacking = true;
        }
        else
        {
            Debug.Log("Mana tidak cukup untuk Skill 2");
            ResetAttack();
        }
    }

    public float Skill1Timer
    {
        get { return skill1Timer; }
    }

    public float Skill2Timer
    {
        get { return skill2Timer; }
    }

    public void ResetAttack()
    {
        anim.SetBool("punch", false);
        anim.SetBool("kick", false);
        anim.SetBool("mmakick", false);
        anim.SetBool("heavyAttack1", false);
        anim.SetBool("heavyAttack2", false);

        if (thirdPersonController != null)
        {
            thirdPersonController.canMove = true;
            thirdPersonController.enabled = true;
        }

        isAttacking = false;
        isApproaching = false;

        if (approachCoroutine != null) StopCoroutine(approachCoroutine);
    }

    public void PerformAttack()
    {
        if (playerStats != null && playerStats.IsDead()) return;

        Collider[] enemies = Physics.OverlapSphere(attackPos.position, attackRange, enemyLayer);
        foreach (var enemy in enemies)
        {
            EnemySimple enemySimple = enemy.GetComponent<EnemySimple>();
            if (enemySimple != null) enemySimple.TakeDamage(playerStats.attackDamage); // Gunakan damage dari stats

            EnemyBase enemyBase = enemy.GetComponent<EnemyBase>();
            if (enemyBase != null) enemyBase.OnHit();
        }
    }

    public void MoveTowardsTarget(Vector3 target_, float deltaDistance, string animName)
    {
        anim.SetBool(animName, true);
        FaceThis(target_);
        Vector3 finalPos = Vector3.MoveTowards(transform.position, target_, deltaDistance);
        finalPos.y = transform.position.y;
        transform.DOMove(finalPos, reachTime);
    }

    public void FaceThis(Vector3 target)
    {
        Vector3 dir = target - transform.position;
        dir.y = 0;
        if (dir == Vector3.zero) return;
        Quaternion rot = Quaternion.LookRotation(dir);
        transform.DORotateQuaternion(rot, 0.2f);
    }

    public void ChangeTarget(Transform newTarget)
    {
        if (target != null)
        {
            EnemyBase oldEnemy = target.GetComponent<EnemyBase>();
            if (oldEnemy != null) oldEnemy.ActiveTarget(false);
        }
        target = newTarget;
        if (target != null)
        {
            EnemyBase newEnemy = target.GetComponent<EnemyBase>();
            if (newEnemy != null)
            {
                newEnemy.ActiveTarget(true);
                Debug.Log("Target locked via Mouse: " + newEnemy.name);
            }
        }
    }

    public void GetClose()
    {
        if (target == null) return;
        Vector3 dir = (transform.position - target.position).normalized;
        Vector3 finalPos = target.position + dir * 1.4f;
        finalPos.y = transform.position.y;
        FaceThis(target.position);
        transform.DOMove(finalPos, 0.2f);
    }
}