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
        catch (System.Exception)
        {
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

        if (deathTimeline != null)
        {
            deathTimeline.Play();
        }

        ExpReward reward = GetComponent<ExpReward>();
        if (reward != null) reward.GiveExp();
    }

    public void ResetBossState()
    {
        if (combatScript != null)
        {
            combatScript.StopAllCoroutines();
            combatScript.isActivated = false;
            combatScript.isAttacking = false;
            combatScript.ForceStopGluttonyVFX();
        }

        NavMeshAgent agent = GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.enabled = false;
        }

        transform.position = initialPosition;
        transform.rotation = initialRotation;

        if (agent != null)
        {
            agent.enabled = true;
            if (agent.isOnNavMesh)
            {
                agent.isStopped = true;
                agent.velocity = Vector3.zero;
            }
        }

        isDead = false;
        currentHealth = maxHealth;

        isPhase2 = false;
        isPhase2Ready = false;
        isImmune = false;

        if (normalVisuals != null) normalVisuals.SetActive(true);
        if (goldVisuals != null) goldVisuals.SetActive(false);
        if (phase2Camera != null) phase2Camera.SetActive(false);

        if (!gameObject.activeInHierarchy)
        {
            gameObject.SetActive(true);
        }

        if (anim != null)
        {
            anim.Rebind();
            anim.Update(0f);
        }

        BossHealthUI uiManager = FindObjectOfType<BossHealthUI>();
        if (uiManager != null) uiManager.DeactivateBossUI();
    }
}