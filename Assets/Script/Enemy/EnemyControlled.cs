using UnityEngine;
using UnityEngine.AI;

public class EnemySimple : MonoBehaviour
{
    [Header("Loot Settings")]
    public GameObject itemPrefab;
    [Range(0, 100)]
    public float dropChance = 20f;

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
        if (playerObj != null) player = playerObj.transform;

        roamTimer = roamWaitTime;
    }

    void Update()
    {
        if (isDead || player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= detectionRadius)
        {
            agent.SetDestination(player.position);
            agent.isStopped = false;

            if (distanceToPlayer <= attackRadius)
            {
                TryAttack();
            }
        }
        else
        {
            HandleRoaming();
        }
    }

    void HandleRoaming()
    {
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            roamTimer += Time.deltaTime;

            if (roamTimer >= roamWaitTime)
            {
                Vector3 randomDest = GetRandomRoamPosition(homePosition, roamRadius);
                agent.SetDestination(randomDest);
                agent.isStopped = false;
                roamTimer = 0;
            }
        }
    }

    Vector3 GetRandomRoamPosition(Vector3 center, float distance)
    {
        Vector3 randomPos = Random.insideUnitSphere * distance;
        randomPos += center;

        NavMeshHit hit;
        NavMesh.SamplePosition(randomPos, out hit, distance, 1);
        return hit.position;
    }


    void TryAttack()
    {
        if (Time.time >= lastAttackTime + attackCooldown)
        {
            Debug.Log("Capsule menyerang Player!");
            PlayerStats pStats = player.GetComponent<PlayerStats>();
            if (pStats != null) pStats.TakeDamage(damageAmount);
            lastAttackTime = Time.time;
        }
    }

    public void TakeDamage(float damage)
    {
        health -= damage;
        if (health <= 0) Die();
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        float randomValue = Random.Range(0f, 100f);

        if (randomValue <= dropChance)
        {
            if (itemPrefab != null)
            {
                Instantiate(itemPrefab, transform.position + Vector3.up * 0.5f, Quaternion.identity);
                Debug.Log("Beruntung! Item dijatuhkan.");
            }
        }
        else
        {
            Debug.Log("Tidak beruntung, item tidak muncul.");
        }

        if (agent != null)
        {
            agent.isStopped = true;
            agent.enabled = false;
        }

        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);


        if (Application.isPlaying)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(homePosition, roamRadius);
        }
        else
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, roamRadius);
        }
    }
}