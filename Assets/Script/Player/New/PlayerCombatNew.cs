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
    private StarterAssetsInputs _inputs;

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
    public float autoSprintSpeed = 8f;
    public float strikeRange = 1.5f;
    private bool isSprinting => _inputs != null && _inputs.sprint;

    [Header("Skill Settings")]
    public float skill1ManaCost = 5f; 
    public float skill2ManaCost = 10f;
    public float skill1Cooldown = 5f;
    public float skill2Cooldown = 10f; 
    public float skill1Range = 3f; 
    public float skill2Range = 5f; 

    [Header("Radius Visualizer")]
    public LineRenderer radiusVisualizer;
    public int circleSegments = 50; 

    [Header("Dash Settings")]
    [SerializeField] private TrailRenderer trailRenderer;
    public float dashForce = 20f;
    public float dashTime = 0.2f;
    public float dashCooldown = 1f;
    private bool canDash = true;

    [Header("VFX References")]
    [SerializeField] private MeshTrail meshTrail;

    private float skill1Timer = 0f;
    private float skill2Timer = 0f;

    private bool isAttacking = false;
    private bool isApproaching = false;
    private Coroutine approachCoroutine;

    [Header("Weapon & Damage Scaling")]
    public DamageDealer weaponDamageDealer;

    [Range(0f, 3f)] public float quickAttackMultiplier = 1.0f; 
    [Range(0f, 3f)] public float skill1Multiplier = 1.8f;     
    [Range(0f, 3f)] public float skill2Multiplier = 1.2f;    

    private float currentMultiplier = 1.0f;

    [Tooltip("Jarak aman agar player berhenti di 'kulit' musuh, bukan di tengah badannya")]
    public float enemyBodyRadius = 1.2f;

    [Header("Potion System")]
    public int maxPotionCharges = 3;
    private int currentPotionCharges = 3;
    [SerializeField] private Animator potionAnimator;


    void Awake()
    {
        playerStats = GetComponent<PlayerStats>();
        _inputs = GetComponent<StarterAssetsInputs>();
    }

    void Update()
    {
        if (playerStats != null && playerStats.IsDead())
        {
            // Hentikan paksa pergerakan/rotasi dari DOTween agar mayat tidak meluncur
            transform.DOKill();

            // Batalkan semua status nyerang/ngejar
            if (isAttacking || isApproaching)
            {
                if (approachCoroutine != null) StopCoroutine(approachCoroutine);
                isAttacking = false;
                isApproaching = false;
                HideRadius();
            }
            return; // Berhenti memproses input/timer lainnya
        }


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
        if (value.isPressed && playerStats != null && !playerStats.IsDead() && !playerStats.isStunned)
        {
            Attack(0);
        }
    }

    public void OnSkill1(InputValue value)
    {
        if (value.isPressed && playerStats != null && !playerStats.IsDead() && !playerStats.isStunned)
        {
            if (skill1Timer <= 0) Attack(1);
            else Debug.Log("Skill 1 sedang Cooldown");
        }
    }

    public void OnSkill2(InputValue value)
    {
        if (value.isPressed && playerStats != null && !playerStats.IsDead() && !playerStats.isStunned)
        {
            if (skill2Timer <= 0) Attack(2);
            else Debug.Log("Skill 2 sedang Cooldown");
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

        SetActiveRadius(strikeRange);

        while (targetNode != null && Vector3.Distance(transform.position, targetNode.position) > strikeRange)
        {
            if (!isApproaching)
            {
                HideRadius();
                yield break;
            }
            FaceThis(targetNode.position);

            float currentMoveSpeed = isSprinting ? autoSprintSpeed : autoWalkSpeed;

            anim.SetFloat("Speed", currentMoveSpeed);
            anim.SetFloat("MotionSpeed", isSprinting ? 1.5f : 1f);
            float step = currentMoveSpeed * Time.deltaTime;

            Vector3 targetPos = new Vector3(targetNode.position.x, transform.position.y, targetNode.position.z);
            transform.position = Vector3.MoveTowards(transform.position, targetPos, step);
            yield return null;
        }

        HideRadius();
        isApproaching = false;
        anim.SetFloat("Speed", 0f);

        if (targetNode != null) ExecuteQuickAttackAnim(attackIndex);
        else ResetAttack();
    }

    void CancelApproach()
    {
        isApproaching = false;
        anim.SetFloat("Speed", 0f);
        HideRadius(); 
        if (approachCoroutine != null) StopCoroutine(approachCoroutine);
        ResetAttack();
    }

    void ExecuteQuickAttackAnim(int attackIndex)
    {
        isAttacking = true;
        currentMultiplier = quickAttackMultiplier; 
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

        float distance = Vector3.Distance(transform.position, target.position);

        if (distance > skill1Range)
        {
            if (approachCoroutine != null) StopCoroutine(approachCoroutine);
            approachCoroutine = StartCoroutine(ApproachAndCastSkill(target, 1, skill1Range));
        }
        else
        {
            ExecuteSkill1(); 
        }
    }

    void Skill2()
    {
        if (target == null) { ResetAttack(); return; }

        float distance = Vector3.Distance(transform.position, target.position);

        if (distance > skill2Range)
        {
            if (approachCoroutine != null) StopCoroutine(approachCoroutine);
            approachCoroutine = StartCoroutine(ApproachAndCastSkill(target, 2, skill2Range));
        }
        else
        {
            ExecuteSkill2();
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

    void ExecuteSkill1()
    {
        if (playerStats != null && playerStats.UseMana(skill1ManaCost))
        {
            skill1Timer = skill1Cooldown;
            currentMultiplier = skill1Multiplier;
            FaceThis(target.position);
            anim.SetBool("heavyAttack1", true);
            isAttacking = true;
            playerStats.isInvincible = true;
        }
        else { ResetAttack(); }
    }

    void ExecuteSkill2()
    {
        if (playerStats != null && playerStats.UseMana(skill2ManaCost))
        {
            skill2Timer = skill2Cooldown;
            currentMultiplier = skill2Multiplier;
            FaceThis(target.position);
            anim.SetBool("heavyAttack2", true);
            isAttacking = true;
            playerStats.isInvincible = true;
        }
        else { ResetAttack(); }
    }

    IEnumerator ApproachAndCastSkill(Transform targetNode, int skillIndex, float range)
    {
        isAttacking = true;
        isApproaching = true;
        SetActiveRadius(range);

        while (targetNode != null && Vector3.Distance(transform.position, targetNode.position) > range)
        {
            if (!isApproaching) { HideRadius(); yield break; }

            FaceThis(targetNode.position);

            float currentMoveSpeed = isSprinting ? autoSprintSpeed : autoWalkSpeed;
            anim.SetFloat("Speed", currentMoveSpeed);
            anim.SetFloat("MotionSpeed", isSprinting ? 1.5f : 1f);

            float step = currentMoveSpeed * Time.deltaTime;
            Vector3 targetPos = new Vector3(targetNode.position.x, transform.position.y, targetNode.position.z);
            transform.position = Vector3.MoveTowards(transform.position, targetPos, step);
            yield return null;
        }

        HideRadius();
        isApproaching = false;
        anim.SetFloat("Speed", 0f);

        if (targetNode != null)
        {
            if (skillIndex == 1) ExecuteSkill1();
            else if (skillIndex == 2) ExecuteSkill2();
        }
        else ResetAttack();
    }

    private void SetActiveRadius(float radius)
    {
        if (radiusVisualizer == null) return;

        radiusVisualizer.enabled = true;
        radiusVisualizer.positionCount = circleSegments + 1;
        radiusVisualizer.useWorldSpace = false; 

        float angle = 0f;
        for (int i = 0; i < circleSegments + 1; i++)
        {
            float x = Mathf.Sin(Mathf.Deg2Rad * angle) * radius;
            float z = Mathf.Cos(Mathf.Deg2Rad * angle) * radius;
            radiusVisualizer.SetPosition(i, new Vector3(x, 0.1f, z));
            angle += (360f / circleSegments);
        }
    }

    private void HideRadius()
    {
        if (radiusVisualizer != null)
        {
            radiusVisualizer.enabled = false;
        }
    }

    public void ResetAttack()
    {
        anim.SetBool("punch", false);
        anim.SetBool("kick", false);
        anim.SetBool("mmakick", false);
        anim.SetBool("heavyAttack1", false);
        anim.SetBool("heavyAttack2", false);

        if (playerStats != null) playerStats.isInvincible = false;

        if (thirdPersonController != null && (playerStats == null || !playerStats.IsDead()))
        {
            thirdPersonController.canMove = true;
            thirdPersonController.enabled = true;
        }

        isAttacking = false;
        isApproaching = false;
        HideRadius();

        if (approachCoroutine != null) StopCoroutine(approachCoroutine);
    }

    public void EnableWeaponHitbox()
    {
        if (weaponDamageDealer != null && playerStats != null)
        {
            float finalDamage = playerStats.attackDamage * currentMultiplier;
            weaponDamageDealer.StartDealDamage(finalDamage);
        }
    }

    public void DisableWeaponHitbox()
    {
        if (weaponDamageDealer != null)
        {
            weaponDamageDealer.EndDealDamage();
        }
    }

    public void MoveTowardsTarget(Vector3 target_, float deltaDistance, string animName)
    {
        anim.SetBool(animName, true);
        FaceThis(target_);

        float dynamicRadius = enemyBodyRadius;
        Collider targetCol = target.GetComponent<Collider>();

        if (targetCol != null)
        {
            dynamicRadius = targetCol.bounds.extents.x + 0.5f;
        }

        float currentDistance = Vector3.Distance(transform.position, target_);

        if (currentDistance <= dynamicRadius)
        {
            return;
        }

        Vector3 directionToPlayer = (transform.position - target_).normalized;
        Vector3 outerEdgePos = target_ + (directionToPlayer * dynamicRadius);

        Vector3 finalPos = Vector3.MoveTowards(transform.position, outerEdgePos, deltaDistance);
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

        float dynamicRadius = enemyBodyRadius;
        Collider targetCol = target.GetComponent<Collider>();
        if (targetCol != null) dynamicRadius = targetCol.bounds.extents.x + 0.5f;

        float currentDistance = Vector3.Distance(transform.position, target.position);

        if (currentDistance <= dynamicRadius) return;

        Vector3 dirToPlayer = (transform.position - target.position).normalized;
        Vector3 finalPos = target.position + (dirToPlayer * dynamicRadius);
        finalPos.y = transform.position.y;

        FaceThis(target.position);
        transform.DOMove(finalPos, 0.2f);
    }

    public void OnDash(InputValue value)
    {
        if (value.isPressed && canDash && !playerStats.IsDead() && !playerStats.isStunned)
        {
            if (isApproaching)
            {
                CancelApproach();
            }

            if (!isAttacking || isApproaching)
            {
                StartCoroutine(DashRoutine());
            }
        }
    }

    private IEnumerator DashRoutine()
    {
        canDash = false;
        isAttacking = true;

        if (anim != null) anim.SetTrigger("Dash");

        if (meshTrail != null) meshTrail.SetTrailActive(true);
        if (playerStats != null) playerStats.isInvincible = true;

        if (thirdPersonController != null) thirdPersonController.canMove = false;

        Vector3 dashDir = transform.forward;
        float startTime = Time.time;

        while (Time.time < startTime + dashTime)
        {
            transform.position += dashDir * dashForce * Time.deltaTime;
            yield return null;
        }

        if (meshTrail != null) meshTrail.SetTrailActive(false);
        if (playerStats != null) playerStats.isInvincible = false;
        isAttacking = false;

        if (thirdPersonController != null) thirdPersonController.canMove = true;

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    public void OnUsePotion(InputValue value)
    {
        if (value.isPressed && !playerStats.IsDead() && currentPotionCharges > 0)
        {
            // Opsional: Jangan pakai potion kalau darah sudah penuh
            if (playerStats.currentHealth >= playerStats.maxHealth) return;

            ExecuteUsePotion();
        }
    }

    private void ExecuteUsePotion()
    {
        currentPotionCharges--;

        // Panggil efek di PlayerStats
        playerStats.ApplyPotionEffect(25f, 1f, 5f);

        // Update Animator UI Potion
        if (potionAnimator != null)
        {
            // Kirim integer sisa potion (3, 2, 1, atau 0) ke Animator
            potionAnimator.SetInteger("Charges", currentPotionCharges);
        }

        Debug.Log($"<color=green>Potion digunakan! Sisa: {currentPotionCharges}</color>");
    }
}