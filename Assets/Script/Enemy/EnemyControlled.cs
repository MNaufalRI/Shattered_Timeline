using UnityEngine;
using UnityEngine.AI;

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

    private Transform player;
    private NavMeshAgent agent;
    private float lastAttackTime;
    private bool isDead = false;

    private Vector3 homePosition;
    private float roamTimer;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        homePosition = transform.position;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;

        roamTimer = roamWaitTime;
    }

    void Update()
    {
        if (isDead || player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= detectionRadius)
        {
            ChasePlayer(distance);
        }
        else
        {
            HandleRoaming();
        }
    }

    void ChasePlayer(float distance)
    {
        agent.isStopped = false;
        agent.SetDestination(player.position);

        if (distance <= attackRadius)
        {
            TryAttack();
        }
    }

    void HandleRoaming()
    {
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            roamTimer += Time.deltaTime;

            if (roamTimer >= roamWaitTime)
            {
                Vector3 newPos = GetRandomRoamPosition(homePosition, roamRadius);
                agent.SetDestination(newPos);
                roamTimer = 0f;
            }
        }
    }

    Vector3 GetRandomRoamPosition(Vector3 center, float distance)
    {
        Vector3 randomPos = Random.insideUnitSphere * distance + center;

        NavMeshHit hit;
        NavMesh.SamplePosition(randomPos, out hit, distance, NavMesh.AllAreas);

        return hit.position;
    }

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

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        health -= damage;

        if (health <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        DropLoot();

        if (agent != null)
        {
            agent.isStopped = true;
            agent.enabled = false;
        }

        Destroy(gameObject);
    }

    void DropLoot()
    {
        if (lootTable == null || lootTable.Length == 0)
        {
            Debug.Log("Loot table kosong!");
            return;
        }

        foreach (var loot in lootTable)
        {
            if (loot.prefab == null) continue;

            float roll = Random.Range(0f, 100f);

            if (roll <= loot.dropChance)
            {
                Vector3 spawnPos = transform.position + Vector3.up * 0.5f;
                Instantiate(loot.prefab, spawnPos, Quaternion.identity);

                Debug.Log("Drop: " + loot.prefab.name);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRadius);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(Application.isPlaying ? homePosition : transform.position, roamRadius);
    }
}

[System.Serializable]
public class LootItem
{
    public GameObject prefab;
    [Range(0, 100)] public float dropChance;
}