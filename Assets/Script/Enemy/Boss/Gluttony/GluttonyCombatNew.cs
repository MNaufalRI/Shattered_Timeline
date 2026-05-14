using StarterAssets;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(DragonBoarStats))]
[RequireComponent(typeof(NavMeshAgent))]
public class DragonBoarCombat : MonoBehaviour
{
    [Header("Targeting")]
    public Transform player;
    private PlayerStats playerStats;

    [Header("Attack Settings")]
    public float attackRange = 2.5f;
    public float basicAttackDamage = 5f;
    public float heavyAttackDamage = 10f;

    [Header("Heavy Attack (High Speed Pursuit)")]
    public float chargeSpeed = 12f;
    public float rageChargeSpeed = 25f; 
    public float forceHeavyAttackTime = 5f; 
    private float normalSpeed;

    [Header("Gluttony Settings")]
    public float gluttonyAnimDuration = 3f;
    public float extraStunTime = 4f;
    public float pullSpeed = 5f; // Kecepatan sedotan boss
    public float suckDuration = 2f; // Berapa lama boss menyedot sebelum stun
    public float maxDistanceTrigger = 15f; // Jarak untuk auto-trigger Gluttony
    public float autoSkillCooldown = 15f; // Waktu aman di awal game (15 detik)
    public float gluttonyAutoCooldown = 20f; // Jeda 20 detik antar auto-skill
    private float gluttonyAutoTimer = 0f;

    private float gameTimer = 0f;

    [Header("Cooldowns")]
    public float attackCooldown = 2f;
    private float attackTimer = 0f;
    private float timeSinceLastAttack = 0f;

    private Animator anim;
    private NavMeshAgent agent;
    private DragonBoarStats stats;

    [HideInInspector] public bool isAttacking = false;
    private float normalAngularSpeed;
    private float normalAcceleration;

    [Header("Rotten Expulsion (Acid Meteor)")]
    public Transform mouthTransform; 
    public GameObject projectileUpPrefab; 
    public GameObject acidMeteorPrefab; 
    public int meteorCount = 3;
    public float meteorDelay = 0.5f;

    [Header("Attack Probability (Total must be 100)")]
    [Range(0f, 100f)] public float basicAttackChance = 35f;
    [Range(0f, 100f)] public float heavyAttackChance = 30f;
    [Range(0f, 100f)] public float gluttonyChance = 15f;
    [Range(0f, 100f)] public float rottenExpulsionChance = 20f;

    [Header("VFX Settings")]
    public GameObject gluttonyVFXPrefab; // Pasang prefab partikel sedotan di sini
    public Transform GluttonyTransform;     // Posisi asal sedotan (misal di mulut)
    private GameObject activeVFX;        // Untuk menyimpan instance yang sedang jalan

    private bool isStealingSpeed = false; // Mencegah dobel efek Steal

    private void Start()
    {
        anim = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        stats = GetComponent<DragonBoarStats>();

        normalSpeed = agent.speed;
        normalAngularSpeed = agent.angularSpeed;
        normalAcceleration = agent.acceleration;

        if (player != null)
        {
            playerStats = player.GetComponent<PlayerStats>();
        }
    }

    private void Update()
    {
        if (stats.isDead || player == null) return;

        gameTimer += Time.deltaTime;
        if (gluttonyAutoTimer > 0) gluttonyAutoTimer -= Time.deltaTime;
        if (isAttacking) return;

        timeSinceLastAttack += Time.deltaTime;
        if (attackTimer > 0) attackTimer -= Time.deltaTime;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // --- LOGIKA KHUSUS: GLUTTONY AUTO-TRIGGER ---
        if (gameTimer >= 15f && distanceToPlayer > maxDistanceTrigger && gluttonyAutoTimer <= 0)
        {
            StartCoroutine(GluttonyRoutine());
            return;
        }

        if (timeSinceLastAttack >= forceHeavyAttackTime)
        {
            StartCoroutine(HeavyAttackRoutine(true));
            return;
        }

        if (distanceToPlayer <= attackRange)
        {
            // Berhenti dan hadap player
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
            anim.SetFloat("Speed", 0f);
            Vector3 lookPos = new Vector3(player.position.x, transform.position.y, player.position.z);
            transform.LookAt(lookPos);

            if (attackTimer <= 0f)
            {
                ChooseRandomAttack();
            }
        }
        else
        {
            agent.isStopped = false;
            agent.SetDestination(player.position);
            anim.SetFloat("Speed", agent.velocity.magnitude);
        }
    }

    private void ChooseRandomAttack()
    {
        // Hitung total chance tanpa Gluttony dulu
        float totalChance = basicAttackChance + heavyAttackChance + rottenExpulsionChance;

        // HANYA masukkan Gluttony ke dalam "Gacha" jika cooldown 20 detiknya sudah habis
        if (gluttonyAutoTimer <= 0)
        {
            totalChance += gluttonyChance;
        }

        float rand = Random.Range(0f, totalChance);

        if (rand < basicAttackChance)
        {
            StartCoroutine(BasicAttackRoutine());
        }
        else if (rand < basicAttackChance + heavyAttackChance)
        {
            StartCoroutine(HeavyAttackRoutine(false));
        }
        // Pastikan juga di sini mengecek gluttonyAutoTimer
        else if (gluttonyAutoTimer <= 0 && rand < basicAttackChance + heavyAttackChance + gluttonyChance)
        {
            StartCoroutine(GluttonyRoutine());
        }
        else
        {
            StartCoroutine(RottenExpulsionRoutine());
        }
    }

    private IEnumerator BasicAttackRoutine()
    {
        isAttacking = true;
        timeSinceLastAttack = 0f;
        anim.SetTrigger("BasicAttack");

        yield return new WaitForSeconds(0.5f);

        if (Vector3.Distance(transform.position, player.position) <= attackRange + 0.5f)
        {
            DealDamageToPlayer(basicAttackDamage);
        }

        yield return new WaitForSeconds(0.5f);
        ResetAttack();
    }
    private IEnumerator HeavyAttackRoutine(bool isRageMode)
    {
        isAttacking = true;
        timeSinceLastAttack = 0f;

        agent.speed = isRageMode ? rageChargeSpeed : chargeSpeed;
        agent.angularSpeed = 2000f;
        agent.acceleration = 100f;
        agent.isStopped = false;

        bool hasTriggeredAnim = false;

        while (Vector3.Distance(transform.position, player.position) > attackRange)
        {
            agent.SetDestination(player.position);
            anim.SetFloat("Speed", agent.velocity.magnitude);

            if (!hasTriggeredAnim && Vector3.Distance(transform.position, player.position) <= attackRange + 1.5f)
            {
                anim.SetTrigger("HeavyAttack");
                hasTriggeredAnim = true;
            }
            yield return null;
        }

        agent.isStopped = true;
        agent.velocity = Vector3.zero;
        anim.SetFloat("Speed", 0f);

        float windUpTimer = 0.8f;
        while (windUpTimer > 0)
        {
            if (isRageMode)
            {
                Vector3 lookPos = player.position - transform.position;
                lookPos.y = 0;
                if (lookPos != Vector3.zero)
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookPos), Time.deltaTime * 10f);

                transform.position = Vector3.MoveTowards(transform.position, player.position, Time.deltaTime * 2f);
            }
            windUpTimer -= Time.deltaTime;
            yield return null;
        }
        bool isHit;
        if (isRageMode)
        {
            isHit = true; 
        }
        else
        {
            isHit = Vector3.Distance(transform.position, player.position) <= attackRange + 1.5f;
        }

        if (isHit)
        {
            DealDamageToPlayer(heavyAttackDamage);
            if (playerStats != null)
            {
                playerStats.ApplyStun(1.5f);
            }
        }

        yield return new WaitForSeconds(0.4f);
        yield return new WaitForSeconds(0.6f);
        ResetAttack();
    }

    private IEnumerator GluttonyRoutine()
    {
        isAttacking = true;
        timeSinceLastAttack = 0f; // Reset timer attack
        gluttonyAutoTimer = gluttonyAutoCooldown;

        // --- PENTING: HENTIKAN PERGERAKAN BOSS ---
        if (agent != null)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero; // Hilangkan momentum agar tidak meluncur/sliding
        }

        // Buat bos menghadap ke arah player sebelum menyedot
        if (player != null)
        {
            transform.LookAt(new Vector3(player.position.x, transform.position.y, player.position.z));
        }

        anim.SetTrigger("Gluttony");
        yield return new WaitForSeconds(1f);

        // --- FASE 1: MUNCULKAN VFX ---
        if (gluttonyVFXPrefab != null && GluttonyTransform != null)
        {
            // Munculkan VFX di posisi mulut dengan rotasi asli mulut
            activeVFX = Instantiate(gluttonyVFXPrefab, GluttonyTransform.position, GluttonyTransform.rotation);

            // Jadikan child agar posisinya & rotasinya TERKUNCI pada mulut bos
            activeVFX.transform.parent = GluttonyTransform;
        }

        PlayerMovement2 playerMove = player.GetComponent<PlayerMovement2>();
        CharacterController controller = player.GetComponent<CharacterController>();

        if (playerMove != null)
        {
            playerMove.MoveSpeed *= 0.2f;
            playerMove.SprintSpeed *= 0.2f;
            playerMove.enabled = false;
        }

        // --- FASE 2: PENARIKAN ---
        float timer = 0f;
        while (timer < suckDuration)
        {
            timer += Time.deltaTime;

            // KODE LookAt DIHAPUS DARI SINI: VFX sekarang murni mengikuti rotasi GluttonyTransform

            if (player != null && controller != null)
            {
                Vector3 pullDirection = (transform.position - player.position).normalized;
                pullDirection.y = 0;
                controller.Move(pullDirection * (pullSpeed * Time.deltaTime));
            }
            yield return null;
        }

        // --- FASE 3: MATIKAN VFX & STUN ---
        // Hentikan partikel (lebih bagus pakai Stop() daripada Destroy langsung agar sisa partikel menghilang halus)
        if (activeVFX != null)
        {
            ParticleSystem ps = activeVFX.GetComponent<ParticleSystem>();
            if (ps != null) ps.Stop();
            Destroy(activeVFX, 4f); // Hapus objek setelah sisa partikel hilang
        }

        if (playerStats != null) playerStats.ApplyStun(3f);
        yield return new WaitForSeconds(1f);

        if (stats != null && stats.isPhase2)
        {
            StartCoroutine(GluttonySpeedStealRoutine(playerMove));
        }
        else
        {
            // Kembalikan speed player ke normal jika MASIH PHASE 1
            if (playerMove != null)
            {
                playerMove.enabled = true;
                playerMove.MoveSpeed /= 0.2f;
                playerMove.SprintSpeed /= 0.2f;
            }
        }

        ResetAttack();
    }

    private IEnumerator GluttonySpeedStealRoutine(PlayerMovement2 playerMove)
    {
        // Jika efek Steal masih berjalan, JANGAN tumpuk lagi (Fail-safe)
        if (isStealingSpeed) yield break;
        isStealingSpeed = true;

        // --- MULAI EFEK STEAL SPEED ---
        if (playerMove != null)
        {
            playerMove.enabled = true; 

            // Kembalikan dulu dari debuff 0.2f (fase sedot), lalu terapkan debuff 40% (dikali 0.6f)
            playerMove.MoveSpeed = (playerMove.MoveSpeed / 0.2f) * 0.6f;
            playerMove.SprintSpeed = (playerMove.SprintSpeed / 0.2f) * 0.6f;
        }

        // Simpan speed boss sebelum buff 40%
        float originalSpeedBeforeSteal = normalSpeed;

        // Tambah speed boss 40%
        normalSpeed *= 1.4f;
        if (agent != null) agent.speed = normalSpeed;

        Debug.Log("<color=orange>Gluttony Speed Steal Active! (5 Seconds)</color>");

        // --- TUNGGU 5 DETIK ---
        yield return new WaitForSeconds(5f);

        // --- KEMBALIKAN KE KONDISI NORMAL PHASE 2 ---
        if (playerMove != null)
        {
            playerMove.MoveSpeed /= 0.6f;
            playerMove.SprintSpeed /= 0.6f;
        }

        normalSpeed = originalSpeedBeforeSteal;
        if (!isAttacking && agent != null) agent.speed = normalSpeed;

        isStealingSpeed = false;
        Debug.Log("<color=cyan>Gluttony Speed Steal Ended.</color>");
    }

    private void DealDamageToPlayer(float damageAmount)
    {
        if (playerStats != null && !playerStats.isInvincible)
        {
            playerStats.TakeDamage(damageAmount);
        }
    }

    private void ResetAttack()
    {
        isAttacking = false;
        agent.speed = normalSpeed;
        agent.angularSpeed = normalAngularSpeed;
        agent.acceleration = normalAcceleration;

        agent.isStopped = false;
        attackTimer = attackCooldown;
        timeSinceLastAttack = 0f;
        if (agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh)
        {
            agent.isStopped = false;
        }
        if (anim != null)
        {
            anim.SetBool("isAttacking", false);
        }
    }

    private IEnumerator RottenExpulsionRoutine()
    {
        isAttacking = true;
        if (anim != null) anim.SetBool("isAttacking", true);

        agent.isStopped = true;
        transform.LookAt(new Vector3(player.position.x, transform.position.y, player.position.z));
        anim.SetTrigger("SpitAcid");

        yield return new WaitForSeconds(1f);

        if (projectileUpPrefab != null && mouthTransform != null)
        {
            GameObject spit = Instantiate(projectileUpPrefab, mouthTransform.position, mouthTransform.rotation);
            Destroy(spit, 1f);
        }

        StartCoroutine(SpawnMeteorsSequence());

        yield return new WaitForSeconds(1f);

        ResetAttack();

    }

    private IEnumerator SpawnMeteorsSequence()
    {
        yield return new WaitForSeconds(1.5f);

        for (int i = 0; i < meteorCount; i++)
        {
            if (player != null)
            {
                Vector3 spawnPos = player.position;
                Instantiate(acidMeteorPrefab, spawnPos, Quaternion.identity);
            }

            yield return new WaitForSeconds(meteorDelay);
        }
    }

    public void ApplyPhase2CombatBuffs()
    {
        // 1. Tambah 1 meteor permanen
        meteorCount += 1;

        // 2. Tambah damage 20%
        basicAttackDamage *= 1.2f;
        heavyAttackDamage *= 1.2f;

        // 3. Tambah speed boss 30% permanen
        // SANGAT PENTING: Update normalSpeed agar tidak tertimpa saat ResetAttack()
        normalSpeed *= 1.3f;
        chargeSpeed *= 1.3f;     // Opsional: Charge berat juga makin cepat
        rageChargeSpeed *= 1.3f; // Opsional: Rage charge makin cepat

        if (!isAttacking && agent != null)
        {
            agent.speed = normalSpeed;
        }

        Debug.Log("<color=yellow>Phase 2 Combat Buffs Applied! Damage, Speed, and Meteor increased.</color>");
    }


}