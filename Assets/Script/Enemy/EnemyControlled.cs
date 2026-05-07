using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class EnemySimple : MonoBehaviour
{
    [Header("Loot Settings")]
    public LootItem[] lootTable;

    [Header("Stats")]
    public float health = 50f;
    public float detectionRadius = 10f;
    public float attackRadius = 1.5f;
    public float attackCooldown = 1.5f;
    public float damageAmount = 10f;

    [Header("Roaming Settings")]
    public float roamRadius = 5f;
    public float roamWaitTime = 3f;

    [Header("Hit Settings")]
    public float hitPauseDuration = 0.15f; // lebih natural

    [Header("Rotation Settings")]
    public float rotationSpeed = 10f;

    private Transform player;
    private NavMeshAgent agent;

    private float lastAttackTime;
    private bool isDead = false;
    private bool isHitPaused = false;
    private bool isAggro = false;

    private Vector3 homePosition;
    private float roamTimer;

    private float originalSpeed;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        // simpan speed awal
        originalSpeed = agent.speed;

        // penting: rotasi manual biar gak glitch
        agent.updateRotation = false;

        homePosition = transform.position;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;

        roamTimer = roamWaitTime;
    }

    void Update()
    {
        if (isDead || player == null) return;
        if (agent == null || !agent.enabled || !agent.isOnNavMesh) return;

        // Saat kena hit → cuma hadap player (tidak jalan)
        if (isHitPaused)
        {
            RotateTowards(player.position);
            return;
        }

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= detectionRadius)
        {
            isAggro = true;
            ChasePlayer(distance);
        }
        else
        {
            isAggro = false;
            HandleRoaming();
        }

        // 🔥 FAIL SAFE (anti stuck)
        if (isAggro && !agent.hasPath)
        {
            agent.SetDestination(player.position);
        }
    }

    // ================= ROTATION =================

    void RotateTowards(Vector3 targetPos)
    {
        Vector3 dir = targetPos - transform.position;
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.001f) return;

        Quaternion rot = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            rot,
            Time.deltaTime * rotationSpeed
        );
    }

    // ================= CHASE =================

    void ChasePlayer(float distance)
    {
        RotateTowards(player.position);

        agent.isStopped = false;
        agent.SetDestination(player.position);

        if (distance <= attackRadius)
            TryAttack();
    }

    // ================= ROAM =================

    void HandleRoaming()
    {
        if (isAggro) return;

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            roamTimer += Time.deltaTime;

            if (roamTimer >= roamWaitTime)
            {
                Vector3 newPos = GetRandomRoamPosition(homePosition, roamRadius);

                RotateTowards(newPos);
                agent.SetDestination(newPos);

                roamTimer = 0f;
            }
        }
    }

    Vector3 GetRandomRoamPosition(Vector3 center, float distance)
    {
        for (int i = 0; i < 5; i++)
        {
            Vector3 random = Random.insideUnitSphere * distance;
            random.y = 0f;

            Vector3 pos = center + random;

            if (NavMesh.SamplePosition(pos, out NavMeshHit hit, distance, NavMesh.AllAreas))
                return hit.position;
        }

        return homePosition;
    }

    // ================= ATTACK =================

    void TryAttack()
    {
        if (Time.time < lastAttackTime + attackCooldown) return;

        PlayerStats pStats = player.GetComponent<PlayerStats>();
        if (pStats != null)
        {
            pStats.TakeDamage(damageAmount);
        }

        lastAttackTime = Time.time;
    }

    // ================= DAMAGE =================

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        health -= damage;
        isAggro = true;

        StartCoroutine(HitPause());

        if (health <= 0)
            Die();
    }

    IEnumerator HitPause()
    {
        isHitPaused = true;

        // 🔥 HANYA pause speed (TIDAK reset path!)
        agent.speed = 0f;

        yield return new WaitForSeconds(hitPauseDuration);

        if (!isDead && agent != null && agent.enabled)
        {
            agent.speed = originalSpeed;
        }

        isHitPaused = false;
    }

    // ================= DEATH =================

    void Die()
    {
        if (isDead) return;
        isDead = true;

        StopAllCoroutines();

        if (agent != null)
        {
            agent.isStopped = true;
            agent.enabled = false;
        }

        DropLoot();

        Destroy(gameObject, 0.2f);
    }

    // ================= LOOT =================

    void DropLoot()
    {
        if (lootTable == null) return;

        foreach (var loot in lootTable)
        {
            if (loot.prefab == null) continue;

            if (Random.Range(0f, 100f) <= loot.dropChance)
            {
                Instantiate(loot.prefab, transform.position + Vector3.up * 0.5f, Quaternion.identity);
            }
        }
    }

    // ================= DEBUG =================

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRadius);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(
            Application.isPlaying ? homePosition : transform.position,
            roamRadius
        );
    }
}

[System.Serializable]
public class LootItem
{
    public GameObject prefab;
    [Range(0, 100)] public float dropChance;
}