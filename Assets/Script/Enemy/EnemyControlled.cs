using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class EnemySimple : MonoBehaviour, IDamageable
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
    public float hitPauseDuration = 0.15f; 

    [Header("Rotation Settings")]
    public float rotationSpeed = 10f;

    [Tooltip("Jarak agar musuh berhenti di depan player, bukan di tengahnya")]
    public float stoppingDistanceOffset = 1.2f;

    private Transform player;
    private NavMeshAgent agent;
    private Animator anim; // [TAMBAHAN ANIMASI] Deklarasi Animator

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
        
        // [TAMBAHAN ANIMASI] Mengambil komponen Animator (bisa di objek ini atau child modelnya)
        anim = GetComponentInChildren<Animator>(); 

        originalSpeed = agent.speed;
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

        if (anim != null && agent != null && agent.isOnNavMesh)
        {
            anim.SetFloat("Speed", agent.velocity.magnitude);
        }

        if (agent == null || !agent.enabled || !agent.isOnNavMesh) return;

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

            if (agent.velocity.sqrMagnitude > 0.1f)
            {
                RotateTowards(transform.position + agent.velocity);
            }
        }

        if (isAggro && !agent.hasPath)
        {
            agent.SetDestination(player.position);
        }
    }

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

    void ChasePlayer(float distance)
    {
        RotateTowards(player.position);

        agent.isStopped = false;
        Vector3 dirFromPlayer = (transform.position - player.position).normalized;
        Vector3 stopPos = player.position + (dirFromPlayer * stoppingDistanceOffset);
        agent.SetDestination(stopPos);

        if (distance <= attackRadius)
            TryAttack();
    }

    void HandleRoaming()
    {
        if (isAggro) return;

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

    void TryAttack()
    {
        if (Time.time < lastAttackTime + attackCooldown || isHitPaused || isDead) return;

        RotateTowards(player.position);

        // [TAMBAHAN ANIMASI] Trigger animasi Attack
        if (anim != null) anim.SetTrigger("Attack");

        PlayerStats pStats = player.GetComponent<PlayerStats>();
        if (pStats != null)
        {
            pStats.TakeDamage(damageAmount);
            Debug.Log($"<color=orange>[Enemy]</color> Berhasil memukul Player! Damage: {damageAmount}");
        }
        else
        {
            var eliotRes = player.GetComponent<Eliot.AgentComponents.AgentResources>();
            if (eliotRes != null)
            {
                eliotRes.Action(new Eliot.AgentComponents.ResourceAction("Health", Eliot.AgentComponents.ResourceAffectionWay.Reduce, Mathf.RoundToInt(damageAmount)));
            }
        }

        lastAttackTime = Time.time;
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;

        health -= amount;
        isAggro = true;

        StartCoroutine(HitPause());

        if (health <= 0)
            Die();
    }

    IEnumerator HitPause()
    {
        isHitPaused = true;
        agent.speed = 0f;

        // [TAMBAHAN ANIMASI] Trigger animasi Hi

        yield return new WaitForSeconds(hitPauseDuration);

        if (!isDead && agent != null && agent.enabled)
        {
            agent.speed = originalSpeed;
        }

        isHitPaused = false;
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        StopAllCoroutines();

        // [TAMBAHAN ANIMASI] Trigger animasi mati
        if (anim != null) anim.SetTrigger("Die");

        if (agent != null)
        {
            agent.isStopped = true;
            agent.enabled = false;
        }

        DropLoot();

        // [PENTING] Ubah waktu hancur objek dari 0.2f menjadi lebih lama (misal 2.5f atau 3f) 
        // agar animasi mati sempat diputar sampai selesai sebelum objek menghilang.
        Destroy(gameObject, 2.5f); 
    }

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