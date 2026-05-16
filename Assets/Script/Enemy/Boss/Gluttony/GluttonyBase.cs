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



    private void Awake()
    {
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

        if (anim != null) anim.SetTrigger("Phase2");

        if (phase2VFXPrefab != null)
        {
            Instantiate(phase2VFXPrefab, transform.position, Quaternion.identity, transform);
        }

        if (normalVisuals != null) normalVisuals.SetActive(false);
        if (goldVisuals != null) goldVisuals.SetActive(true);

 
        yield return new WaitForSeconds(phase2TransitionTime);

  
        ApplyPhase2Buffs();

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
        // Buff Speed Permanen +30%
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

        if (questManager != null)
        {
            questManager.BossDefeated();
        }

        Debug.Log("<color=red>DragonBoar telah dikalahkan!</color>");
    }

    public void PlayDeathCutscene()
    {
        if (navMeshAgent != null)
        {
            navMeshAgent.enabled = false;
        }

        transform.position = initialPosition;
        transform.rotation = initialRotation;

        if (deathTimeline != null)
        {
            deathTimeline.Play();
            Debug.Log("<color=cyan>Death Cutscene Started at Spawn Position!</color>");
        }
    }

    public void ResetBossState()
    {
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

        if (!isDead)
        {
            float healAmount = currentHealth * 0.20f;
            currentHealth += healAmount;

            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
            Debug.Log($"<color=green>Boss Healed 20%. Current HP: {currentHealth}</color>");
        }

        isImmune = false; 


        UnityEngine.AI.NavMeshAgent agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent != null) agent.enabled = false;

        transform.position = initialPosition;
        transform.rotation = initialRotation;

        if (agent != null) agent.enabled = true;

        if (anim != null)
        {
            anim.Rebind();
            anim.Update(0f);
        }

        BossHealthUI uiManager = FindObjectOfType<BossHealthUI>();
        if (uiManager != null) uiManager.DeactivateBossUI();
    }

    
}