using System.Collections;
using UnityEngine;
using DG.Tweening;
using UnityEngine.InputSystem;
using StarterAssets;
using UnityEngine.UI;

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

    [Header("Auto Target Settings")]
    [Tooltip("Radius pencarian musuh otomatis saat menyerang")]
    public float autoTargetRadius = 10f;
    [Tooltip("Sudut pandang ke depan untuk auto-target (derajat, misal 180 = seluruh depan)")]
    public float autoTargetAngle = 180f;

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
    public Image potionFillImage;

    [Header("Skill 3 Settings (Upgrade Only)")]
    public float skill3ManaCost = 15f;
    public float skill3Cooldown = 8f;
    public float skill3Range = 7f;
    private float skill3Timer = 0f;

    [Header("Spawn Locations")]
    [Tooltip("Tarik objek kosong yang ada di depan Player ke sini")]
    public Transform slashSpawnPoint;

    [Header("VFX Prefabs")]
    public GameObject slashProjectilePrefab;

    void Awake()
    {
        playerStats = GetComponent<PlayerStats>();
        _inputs = GetComponent<StarterAssetsInputs>();
    }

    void Update()
    {
        if (playerStats != null && playerStats.IsDead())
        {
            transform.DOKill();
            if (isAttacking || isApproaching)
            {
                if (approachCoroutine != null) StopCoroutine(approachCoroutine);
                isAttacking = false;
                isApproaching = false;
                HideRadius();
            }
            return;
        }

        if (skill1Timer > 0) skill1Timer -= Time.deltaTime;
        if (skill2Timer > 0) skill2Timer -= Time.deltaTime;
        if (skill3Timer > 0) skill3Timer -= Time.deltaTime;
    }

    // =========================================================
    // AUTO TARGET: Cari musuh terdekat dalam radius & sudut pandang
    // =========================================================
    private void TryAutoTarget()
    {
        // Kalau sudah ada target aktif dan masih hidup/valid, tetap pakai
        if (target != null)
        {
            // Cek apakah target masih valid (belum mati / masih aktif)
            IDamageable damageable = target.GetComponent<IDamageable>();
            // Jika objek masih aktif, pertahankan target
            if (target.gameObject.activeInHierarchy)
                return;
            else
                ClearTarget(); // target sudah mati, kosongkan
        }

        // Cari semua collider dalam radius
        Collider[] hits = Physics.OverlapSphere(transform.position, autoTargetRadius, enemyLayer);

        Transform closest = null;
        float closestDot = -1f; // dot product terbesar = paling depan

        foreach (var hit in hits)
        {
            if (!hit.gameObject.activeInHierarchy) continue;

            Vector3 dirToEnemy = (hit.transform.position - transform.position).normalized;
            dirToEnemy.y = 0;

            Vector3 forward = transform.forward;
            forward.y = 0;

            float dot = Vector3.Dot(forward.normalized, dirToEnemy);

            // Konversi sudut ke dot product threshold
            float angleThreshold = Mathf.Cos(autoTargetAngle * 0.5f * Mathf.Deg2Rad);

            if (dot >= angleThreshold && dot > closestDot)
            {
                closestDot = dot;
                closest = hit.transform;
            }
        }

        // Fallback: kalau tidak ada di depan, ambil yang paling dekat saja
        if (closest == null && hits.Length > 0)
        {
            float minDist = float.MaxValue;
            foreach (var hit in hits)
            {
                if (!hit.gameObject.activeInHierarchy) continue;
                float dist = Vector3.Distance(transform.position, hit.transform.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    closest = hit.transform;
                }
            }
        }

        if (closest != null)
        {
            ChangeTarget(closest);
        }
    }

    private void ClearTarget()
    {
        if (target != null)
        {
            EnemyBase oldEnemy = target.GetComponent<EnemyBase>();
            if (oldEnemy != null) oldEnemy.ActiveTarget(false);
        }
        target = null;
    }
    // =========================================================

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
            TryAutoTarget(); // << Auto-target sebelum menyerang
            Attack(0);
        }
    }

    public void OnSkill1(InputValue value)
    {
        if (value.isPressed && playerStats != null && !playerStats.IsDead() && !playerStats.isStunned)
        {
            TryAutoTarget(); // << Auto-target sebelum skill
            if (skill1Timer <= 0) Attack(1);
            else Debug.Log("Skill 1 sedang Cooldown");
        }
    }

    public void OnSkill2(InputValue value)
    {
        if (value.isPressed && playerStats != null && !playerStats.IsDead() && !playerStats.isStunned)
        {
            TryAutoTarget(); // << Auto-target sebelum skill
            if (skill2Timer <= 0) Attack(2);
            else Debug.Log("Skill 2 sedang Cooldown");
        }
    }

    public void OnSkill3(InputValue value)
    {
        if (value.isPressed && playerStats != null && !playerStats.IsDead() && !playerStats.isStunned)
        {
            if (PlayerWeaponManager.Instance != null && !PlayerWeaponManager.Instance.IsWeaponMaxLevel())
            {
                Debug.Log("<color=orange>Skill 3 masih terkunci. Upgrade Senjata dulu!</color>");
                return;
            }

            TryAutoTarget(); // << Auto-target sebelum skill
            if (skill3Timer <= 0 && playerStats.currentMana >= skill3ManaCost)
            {
                Attack(3);
            }
            else
            {
                if (skill3Timer > 0) Debug.Log("Skill 3 Cooldown");
                else Debug.Log("Mana tidak cukup");
            }
        }
    }

    public void Attack(int attackState)
    {
        if (isApproaching && (attackState == 1 || attackState == 2 || attackState == 3))
        {
            CancelApproach();
        }

        if (isAttacking) return;

        if (attackState == 1 && playerStats.currentMana < skill1ManaCost) return;
        if (attackState == 2 && playerStats.currentMana < skill2ManaCost) return;
        if (attackState == 3 && playerStats.currentMana < skill3ManaCost) return;

        if (thirdPersonController != null)
        {
            thirdPersonController.canMove = false;
            thirdPersonController.enabled = false;
        }

        ExecuteAttackAnim(attackState);
    }

    void ExecuteAttackAnim(int attackState)
    {
        if (attackState == 0) QuickAttack();
        else if (attackState == 1) Skill1();
        else if (attackState == 2) Skill2();
        else if (attackState == 3) Skill3();
    }

    void Skill3()
    {
        if (target == null)
        {
            ResetAttack();
            return;
        }

        float distance = Vector3.Distance(transform.position, target.position);

        if (distance > skill3Range)
        {
            if (approachCoroutine != null) StopCoroutine(approachCoroutine);
            approachCoroutine = StartCoroutine(ApproachAndCastSkill(target, 3, skill3Range));
        }
        else
        {
            ExecuteSkill3();
        }
    }

    void ExecuteSkill3()
    {
        if (playerStats != null && playerStats.UseMana(skill3ManaCost))
        {
            skill3Timer = skill3Cooldown;
            currentMultiplier = 2.5f;

            if (target != null) FaceThis(target.position);

            anim.SetBool("heavyAttack3", true);
            isAttacking = true;
            playerStats.isInvincible = true;
        }
        else { ResetAttack(); }
    }

    public void SpawnSlashProjectileEvent()
    {
        if (slashProjectilePrefab != null && slashSpawnPoint != null)
        {
            Instantiate(slashProjectilePrefab, slashSpawnPoint.position, slashSpawnPoint.rotation);
            Debug.Log("<color=lime>[SKILL 3]</color> Projectile Slash diluncurkan via Animation Event!");
        }
        else
        {
            Debug.LogError("Gagal spawn Skill 3: Prefab atau SpawnPoint belum ditarik di Inspector PlayerControl!");
        }
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

        float dynamicRange = GetDynamicStrikeRange(strikeRange);

        if (distance > dynamicRange)
        {
            if (approachCoroutine != null) StopCoroutine(approachCoroutine);
            approachCoroutine = StartCoroutine(ApproachAndAttack(target, attackIndex, dynamicRange));
        }
        else
        {
            ExecuteQuickAttackAnim(attackIndex);
        }
    }

    IEnumerator ApproachAndAttack(Transform targetNode, int attackIndex, float range)
    {
        isAttacking = true;
        isApproaching = true;

        SetActiveRadius(range);

        while (targetNode != null && targetNode.gameObject.activeInHierarchy &&
               Vector3.Distance(transform.position, targetNode.position) > range)
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

        if (targetNode != null && targetNode.gameObject.activeInHierarchy)
            ExecuteQuickAttackAnim(attackIndex);
        else
            ResetAttack();
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

    public float Skill1Timer => skill1Timer;
    public float Skill2Timer => skill2Timer;
    public float Skill3Timer => skill3Timer;

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

        while (targetNode != null && targetNode.gameObject.activeInHierarchy &&
               Vector3.Distance(transform.position, targetNode.position) > range)
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

        if (targetNode != null && targetNode.gameObject.activeInHierarchy)
        {
            if (skillIndex == 1) ExecuteSkill1();
            else if (skillIndex == 2) ExecuteSkill2();
            else if (skillIndex == 3) ExecuteSkill3();
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
            radiusVisualizer.enabled = false;
    }

    public void ResetAttack()
    {
        anim.SetBool("punch", false);
        anim.SetBool("kick", false);
        anim.SetBool("mmakick", false);
        anim.SetBool("heavyAttack1", false);
        anim.SetBool("heavyAttack2", false);
        anim.SetBool("heavyAttack3", false);

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
        if (weaponDamageDealer == null)
        {
            Debug.LogError("<color=red>[HITBOX ERROR]</color> weaponDamageDealer NULL!");
            return;
        }

        if (!weaponDamageDealer.gameObject.activeInHierarchy)
        {
            Debug.LogError($"<color=red>[HITBOX ERROR]</color> DamageDealer object '{weaponDamageDealer.gameObject.name}' tidak aktif!");
            return;
        }

        if (playerStats != null)
        {
            float finalDamage = (playerStats.attackDamage + weaponDamageDealer.weaponDamage) * currentMultiplier;
            weaponDamageDealer.StartDealDamage(finalDamage);
        }
    }

    public void DisableWeaponHitbox()
    {
        if (weaponDamageDealer != null)
            weaponDamageDealer.EndDealDamage();
    }

    public void MoveTowardsTarget(Vector3 target_, float deltaDistance, string animName)
    {
        anim.SetBool(animName, true);
        FaceThis(target_);

        float dynamicRadius = enemyBodyRadius;
        Collider targetCol = target.GetComponent<Collider>();

        if (targetCol != null)
            dynamicRadius = targetCol.bounds.extents.x + 0.5f;

        float currentDistance = Vector3.Distance(transform.position, target_);

        if (currentDistance <= dynamicRadius)
            return;

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
                Debug.Log("Auto-target locked: " + newEnemy.name);
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
                CancelApproach();

            if (!isAttacking || isApproaching)
                StartCoroutine(DashRoutine());
        }
    }

    private IEnumerator DashRoutine()
    {
        canDash = false;
        isAttacking = true;
        CharacterController controller = GetComponent<CharacterController>();

        if (anim != null) anim.SetTrigger("Dash");
        if (meshTrail != null) meshTrail.SetTrailActive(true);
        if (playerStats != null) playerStats.isInvincible = true;
        if (thirdPersonController != null) thirdPersonController.canMove = false;

        Vector3 dashDir = transform.forward;
        float startTime = Time.time;

        while (Time.time < startTime + dashTime)
        {
            if (controller != null)
                controller.Move(dashDir * dashForce * Time.deltaTime);
            else
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
        if (value.isPressed && !playerStats.IsDead() && playerStats.currentPotions > 0)
        {
            if (playerStats.currentHealth >= playerStats.maxHealth) return;
            ExecuteUsePotion();
        }
    }

    private void ExecuteUsePotion()
    {
        playerStats.currentPotions--;
        if (anim != null) anim.SetTrigger("Drink");
        playerStats.ApplyPotionEffect(25f, 1f, 5f);
        RefreshPotionUI(playerStats.currentPotions);
        Debug.Log($"<color=green>Potion digunakan! Sisa: {playerStats.currentPotions}</color>");
    }

    private float GetDynamicStrikeRange(float baseRange)
    {
        if (target == null) return baseRange;

        float bodyRadius = enemyBodyRadius;
        Collider targetCol = target.GetComponent<Collider>();
        if (targetCol != null)
            bodyRadius = targetCol.bounds.extents.x + 0.2f;

        return baseRange + bodyRadius;
    }

    public void RefreshPotionUI(int currentCharges)
    {
        if (potionFillImage != null && playerStats != null)
        {
            float fillPercentage = (float)currentCharges / playerStats.maxPotions;
            potionFillImage.fillAmount = fillPercentage;
        }
    }

    public void UpdateWeaponDamageDealer(DamageDealer newWeaponDealer)
    {
        weaponDamageDealer = newWeaponDealer;
        Debug.Log("<color=yellow>PlayerControl:</color> Damage Dealer berhasil diganti ke senjata baru!");
    }
}