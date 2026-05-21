using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using UnityEngine.Playables;

public class DragonBoarStats : MonoBehaviour, IDamageable
{
    [Header("Health Settings")]
    public float maxHealth = 500f;
    public float currentHealth;
    public bool isDead = false;

    [Header("Phase 2 Settings")]
    public bool isPhase2 = false;
    public bool isImmune = false;
    public float phase2TransitionTime = 3f;
    public GameObject normalVisuals;
    public GameObject goldVisuals;
    public GameObject phase2VFXPrefab;
    public bool isPhase2Ready = false;

    [Header("Phase 2 Cutscene (Player & Camera)")]
    public GameObject phase2Camera;
    public MonoBehaviour playerMovement;
    public MonoBehaviour playerCombat;
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
    public BossQuestManager questManager;

    [Tooltip("Masukkan objek Dummy Boss yang ada di tengah arena ke sini")]
    public GameObject dummyCutsceneBoss;



    private void Awake()
    {
        initialPosition = transform.position;
        initialRotation = transform.rotation;
        currentHealth = maxHealth;
    }

    private void Start()
    {
        enemyBase = GetComponent<EnemyBase>();
        combatScript = GetComponent<DragonBoarCombat>();

        anim = GetComponent<Animator>();
        navMeshAgent = GetComponent<NavMeshAgent>();

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
        isImmune = true;

        if (phase2Camera != null) phase2Camera.SetActive(true);

        if (playerMovement != null) playerMovement.enabled = false;
        if (playerCombat != null) playerCombat.enabled = false;
        if (playerStats != null) playerStats.isInvincible = true;

        if (combatScript != null)
        {
            combatScript.StopAllCoroutines();
            combatScript.isAttacking = true;
            combatScript.timeSinceLastAttack = 0;

            if (anim != null)
            {
                anim.ResetTrigger("BasicAttack");
                anim.ResetTrigger("HeavyAttack");
                anim.ResetTrigger("Gluttony");
                anim.ResetTrigger("Hit");
            }
            if (combatScript.activeVFX != null)
            {
                Destroy(combatScript.activeVFX);
            }
        }

        if (navMeshAgent != null && navMeshAgent.isOnNavMesh)
        {
            navMeshAgent.isStopped = true;
            navMeshAgent.velocity = Vector3.zero;
        }

        if (anim != null) anim.SetTrigger("Phase2");

        if (phase2VFXPrefab != null)
        {
            Instantiate(phase2VFXPrefab, transform.position, Quaternion.identity, transform);
        }

        if (normalVisuals != null) normalVisuals.SetActive(false);
        if (goldVisuals != null) goldVisuals.SetActive(true);


        yield return new WaitForSeconds(phase2TransitionTime);


        try
        {
            ApplyPhase2Buffs();
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("<color=orange>Ada error di ApplyPhase2Buffs, tapi Cutscene dipaksa lanjut!</color> " + e);
        }


        if (phase2Camera != null) phase2Camera.SetActive(false);

        if (playerMovement != null) playerMovement.enabled = true;
        if (playerCombat != null) playerCombat.enabled = true;
        if (playerStats != null) playerStats.isInvincible = false;

        if (combatScript != null)
        {
            combatScript.isAttacking = false;
        }

        isImmune = false;
        isPhase2Ready = true;
        if (navMeshAgent != null && navMeshAgent.isOnNavMesh) navMeshAgent.isStopped = false;

        Debug.Log("<color=yellow>DragonBoar memasuki Phase 2! Pertarungan dilanjutkan.</color>");
    }

    private void ApplyPhase2Buffs()
    {
        if (navMeshAgent != null)
        {
            navMeshAgent.speed *= 1.3f;
        }

        if (combatScript != null)
        {
            combatScript.ApplyPhase2CombatBuffs();
        }
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;
        isImmune = true;

        if (combatScript != null)
        {
            combatScript.StopAllCoroutines();
            combatScript.enabled = false;
        }

        if (navMeshAgent != null)
        {
            if (navMeshAgent.isOnNavMesh) navMeshAgent.isStopped = true;
            navMeshAgent.enabled = false;
        }

        if (questManager != null)
        {
            questManager.BossDefeated();
        }


        gameObject.SetActive(false);

        if (dummyCutsceneBoss != null)
        {
            dummyCutsceneBoss.SetActive(true);
        }
        else
        {
            Debug.LogWarning("Dummy Boss belum dimasukkan ke Inspector!");
        }

        if (deathTimeline != null)
        {
            deathTimeline.Play();
        }
        ExpReward reward = GetComponent<ExpReward>();
        if (reward != null) reward.GiveExp();
    }

    // =========================================================
    // RESET BOSS: Dipanggil saat player mati — boss kembali ke awal
    // HP di-reset PENUH, Phase 2 dibatalkan, semua state dikembalikan
    // =========================================================
    public void ResetBossState()
    {
        // Hentikan semua coroutine dan state combat boss
        if (combatScript != null)
        {
            combatScript.StopAllCoroutines();
            combatScript.isActivated = false;
            combatScript.isAttacking = false;
            combatScript.ForceStopGluttonyVFX();
        }

        // Hentikan NavMeshAgent sebelum teleport
        NavMeshAgent agent = GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.enabled = false;
        }

        // Reset posisi & rotasi ke posisi awal
        transform.position = initialPosition;
        transform.rotation = initialRotation;

        // Aktifkan kembali NavMeshAgent setelah pindah posisi
        if (agent != null)
        {
            agent.enabled = true;
            if (agent.isOnNavMesh)
            {
                agent.isStopped = true;
                agent.velocity = Vector3.zero;
            }
        }

        // ============================================================
        // RESET HP PENUH — Ini yang berbeda dari versi lama (heal 20%)
        // ============================================================
        isDead = false;
        currentHealth = maxHealth;
        Debug.Log($"<color=lime>Boss HP direset penuh: {currentHealth} / {maxHealth}</color>");

        // Reset Phase 2 sepenuhnya
        isPhase2 = false;
        isPhase2Ready = false;
        isImmune = false;

        // Kembalikan tampilan visual ke Phase 1
        if (normalVisuals != null) normalVisuals.SetActive(true);
        if (goldVisuals != null) goldVisuals.SetActive(false);
        if (phase2Camera != null) phase2Camera.SetActive(false);

        // Aktifkan kembali gameObject jika sebelumnya dinonaktifkan (saat "mati" fake)
        if (!gameObject.activeInHierarchy)
        {
            gameObject.SetActive(true);
        }

        // Reset animator
        if (anim != null)
        {
            anim.Rebind();
            anim.Update(0f);
        }

        // Sembunyikan Boss UI
        BossHealthUI uiManager = FindObjectOfType<BossHealthUI>();
        if (uiManager != null) uiManager.DeactivateBossUI();

        // Reset BossArenaTrigger agar cutscene bisa diputar ulang
        BossArenaTrigger[] triggers = FindObjectsOfType<BossArenaTrigger>();
        foreach (var trigger in triggers)
        {
            trigger.ResetTrigger();
        }

        Debug.Log("<color=yellow>Boss telah direset sepenuhnya. Siap bertarung dari awal!</color>");
    }
}