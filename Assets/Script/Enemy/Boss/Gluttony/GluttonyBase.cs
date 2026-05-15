using UnityEngine;
using UnityEngine.AI;
using System.Collections; // Wajib untuk Coroutine
using UnityEngine.Playables;

public class DragonBoarStats : MonoBehaviour, IDamageable
{
    [Header("Health Settings")]
    public float maxHealth = 500f;
    public float currentHealth;
    public bool isDead = false;

    [Header("Phase 2 Settings")]
    public bool isPhase2 = false;
    public bool isImmune = false; // Status kebal saat ganti fase
    public float phase2TransitionTime = 3f; // Waktu diam/animasi saat berubah
    public GameObject normalVisuals; // Masukkan Parent dari model biasa ke sini
    public GameObject goldVisuals;   // Masukkan Parent dari model emas ke sini
    public GameObject phase2VFXPrefab; // Efek ledakan/aura saat berubah emas
    public bool isPhase2Ready = false;

    [Header("Phase 2 Cutscene (Player & Camera)")]
    public GameObject phase2Camera;      // Kamera khusus untuk cutscene Phase 2
    public MonoBehaviour playerMovement; // Script pergerakan player
    public MonoBehaviour playerCombat;   // Script serangan player
    public PlayerStats playerStats;

    private Animator anim;
    private NavMeshAgent navMeshAgent;
    private EnemyBase enemyBase;
    private DragonBoarCombat combatScript;

    [Header("Reset Settings")]
    private Vector3 initialPosition;
    private Quaternion initialRotation;

    [Header("Ending Cutscene")]
    public PlayableDirector deathTimeline;



    private void Awake()
    {
        // Simpan posisi awal boss saat game dimulai
        initialPosition = transform.position;
        initialRotation = transform.rotation;
    }

    private void Start()
    {
        enemyBase = GetComponent<EnemyBase>();
        combatScript = GetComponent<DragonBoarCombat>();
        currentHealth = maxHealth;

        anim = GetComponent<Animator>();
        navMeshAgent = GetComponent<NavMeshAgent>();

        // Pastikan visual awal benar
        if (normalVisuals != null) normalVisuals.SetActive(true);
        if (goldVisuals != null) goldVisuals.SetActive(false);
        if (phase2Camera != null) phase2Camera.SetActive(false);
    }

    public void TakeDamage(float damage)
    {
        if (isDead || isImmune) return;

        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            Die();
            return; 
        }

        if (!isPhase2 && currentHealth <= (maxHealth * 0.5f))
        {
            StartCoroutine(EnterPhase2Routine());
            return; 
        }

        if (enemyBase != null && combatScript != null)
        {
            if (!combatScript.isAttacking)
            {
                enemyBase.OnHit();
            }
            else
            {
                Debug.Log("<color=cyan>Boss took damage but has Super Armor!</color>");
            }
        }
    }



    private IEnumerator EnterPhase2Routine()
    {
        isPhase2 = true;
        isImmune = true; // Boss kebal selama transisi


        // --- 1. MULAI CUTSCENE (KUNCI PLAYER & PINDAH KAMERA) ---
        if (phase2Camera != null) phase2Camera.SetActive(true);

        if (playerMovement != null) playerMovement.enabled = false;
        if (playerCombat != null) playerCombat.enabled = false;
        if (playerStats != null) playerStats.isInvincible = true; // Player kebal dari segala damage sisa

        // 2. Hentikan semua aksi Boss
        if (combatScript != null)
        {
            combatScript.StopAllCoroutines();
            combatScript.isAttacking = true;
            combatScript.timeSinceLastAttack = 0;
            anim.ResetTrigger("BasicAttack");
            anim.ResetTrigger("HeavyAttack");
            anim.ResetTrigger("Gluttony");
            anim.ResetTrigger("Hit");
        }

        if (combatScript.activeVFX != null)
        {
            Destroy(combatScript.activeVFX);
        }

        if (navMeshAgent != null && navMeshAgent.isOnNavMesh)
        {
            navMeshAgent.isStopped = true;
            navMeshAgent.velocity = Vector3.zero;
        }

        // 3. Putar Animasi Perubahan
        if (anim != null) anim.SetTrigger("Phase2");

        // 4. Munculkan VFX Transisi
        if (phase2VFXPrefab != null)
        {
            Instantiate(phase2VFXPrefab, transform.position, Quaternion.identity, transform);
        }

        // 5. Ganti Visual dari Biasa ke Emas
        if (normalVisuals != null) normalVisuals.SetActive(false);
        if (goldVisuals != null) goldVisuals.SetActive(true);

        // --- TUNGGU CUTSCENE SELESAI ---
        yield return new WaitForSeconds(phase2TransitionTime);

        // 6. Terapkan Buff Permanen
        ApplyPhase2Buffs();

        // --- 7. AKHIR CUTSCENE (BUKA KUNCI PLAYER & KEMBALIKAN KAMERA) ---
        if (phase2Camera != null) phase2Camera.SetActive(false);

        if (playerMovement != null) playerMovement.enabled = true;
        if (playerCombat != null) playerCombat.enabled = true;
        if (playerStats != null) playerStats.isInvincible = false;

        if (combatScript != null)
        {
            combatScript.isAttacking = false;
        }

        // Lepas kekebalan boss dan biarkan bos lanjut bergerak
        isImmune = false;
        isPhase2Ready = true;
        if (navMeshAgent != null && navMeshAgent.isOnNavMesh) navMeshAgent.isStopped = false;

        Debug.Log("<color=yellow>DragonBoar memasuki Phase 2! Pertarungan dilanjutkan.</color>");
    }

    private void ApplyPhase2Buffs()
    {
        // Buff Speed Permanen +30%
        if (navMeshAgent != null)
        {
            navMeshAgent.speed *= 1.3f;
        }

        // Panggil fungsi di script Combat untuk menambah Damage & Meteor
        if (combatScript != null)
        {
            combatScript.ApplyPhase2CombatBuffs();
        }
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;
        isImmune = true; // Agar tidak bisa di-hit lagi pas mati

        if (combatScript != null)
        {
            combatScript.StopAllCoroutines();
            combatScript.enabled = false;
        }

        if (anim != null)
        {
            anim.ResetTrigger("Hit");
            anim.SetTrigger("Die");
        }

        if (navMeshAgent != null)
        {
            if (navMeshAgent.isOnNavMesh) navMeshAgent.isStopped = true;
            navMeshAgent.enabled = false;
        }

        Debug.Log("<color=red>DragonBoar telah dikalahkan!</color>");
    }

    public void PlayDeathCutscene()
    {
        // 1. Matikan NavMeshAgent agar tidak terjadi konflik saat teleport
        if (navMeshAgent != null)
        {
            navMeshAgent.enabled = false;
        }

        // 2. Kembalikan posisi dan rotasi ke awal
        transform.position = initialPosition;
        transform.rotation = initialRotation;

        // 3. Jalankan Timeline
        if (deathTimeline != null)
        {
            deathTimeline.Play();
            Debug.Log("<color=cyan>Death Cutscene Started at Spawn Position!</color>");
        }
    }

    // FUNGSI BARU UNTUK MERESET BOSS (PARTIAL / SEBAGIAN)
    public void ResetBossState()
    {
        // 1. Matikan Combat & AI
        if (combatScript != null)
        {
            combatScript.StopAllCoroutines();
            combatScript.isActivated = false;
            combatScript.isAttacking = false;
        }

        if (navMeshAgent != null && navMeshAgent.isOnNavMesh)
        {
            navMeshAgent.isStopped = true;
            navMeshAgent.velocity = Vector3.zero;
        }

        // 2. Tambah Darah 20% dari HP SAAT INI
        if (!isDead)
        {
            float healAmount = currentHealth * 0.20f;
            currentHealth += healAmount;

            // Pastikan darah tidak melebihi maxHealth
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
            Debug.Log($"<color=green>Boss Healed 20%. Current HP: {currentHealth}</color>");
        }

        // CATATAN: Kita TIDAK mereset isPhase2 atau isPhase2Ready agar bos tetap di mode emas
        isImmune = false; // Pastikan kelemahannya terbuka lagi

        // 3. Kembalikan Boss ke posisi awal agar tidak diam di depan pintu
        UnityEngine.AI.NavMeshAgent agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent != null) agent.enabled = false;

        transform.position = initialPosition;
        transform.rotation = initialRotation;

        if (agent != null) agent.enabled = true;

        // 4. Hard Reset Animasi ke mode Idle (Tanpa merusak fase)
        if (anim != null)
        {
            anim.Rebind();
            anim.Update(0f);
        }

        // 5. Matikan UI Boss di layar player sampai player masuk arena lagi
        BossHealthUI uiManager = FindObjectOfType<BossHealthUI>();
        if (uiManager != null) uiManager.DeactivateBossUI();
    }

    
}