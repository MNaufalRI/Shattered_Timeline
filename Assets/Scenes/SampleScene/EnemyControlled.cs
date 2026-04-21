using UnityEngine;
using UnityEngine.AI;

public class EnemySimple : MonoBehaviour
{
    [Header("Stats")]
    public float health = 50f;
    public float detectionRadius = 10f;
    public float attackRadius = 1.5f; 
    public float attackCooldown = 1.5f;
    public float damageAmount = 10f;

    private Transform player;
    private NavMeshAgent agent;
    private float lastAttackTime;
    private bool isDead = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;
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
            agent.isStopped = true;
        }
    }

    void TryAttack()
    {
        if (Time.time >= lastAttackTime + attackCooldown)
        {
            Debug.Log("Capsule menyerang Player!");

            PlayerStats pStats = player.GetComponent<PlayerStats>();
            if (pStats != null)
            {
                pStats.TakeDamage(damageAmount);
            }

            lastAttackTime = Time.time;
        }
    }

    public void TakeDamage(float damage)
    {
        health -= damage;
        Debug.Log("Capsule kena hit! Sisa HP: " + health);

        if (health <= 0) Die();
    }

    void Die()
    {
        if (isDead) return; 
        isDead = true;

        Debug.Log(gameObject.name + " telah dikalahkan dan langsung hilang!");

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
    }
}