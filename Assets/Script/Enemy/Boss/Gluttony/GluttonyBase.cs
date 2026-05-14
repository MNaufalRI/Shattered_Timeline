using UnityEngine;
using UnityEngine.AI;
using System.Collections; // Wajib untuk Coroutine

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

    private Animator anim;
    private NavMeshAgent navMeshAgent;
    private EnemyBase enemyBase;
    private DragonBoarCombat combatScript;

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
    }

    public void TakeDamage(float damage)
    {
        // Jika mati atau sedang kebal (animasi ganti fase), abaikan damage
        if (isDead || isImmune) return;

        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            Die();
        }
        else if (!isPhase2 && currentHealth <= (maxHealth * 0.5f))
        {
            // Jika HP di bawah 50% dan belum Phase 2, masuk Phase 2
            StartCoroutine(EnterPhase2Routine());
        }
        else
        {
            // Hit normal
            if (enemyBase != null && combatScript != null && !combatScript.isAttacking)
            {
                enemyBase.OnHit();
            }
        }
    }

    private IEnumerator EnterPhase2Routine()
    {
        isPhase2 = true;
        isImmune = true; // Aktifkan kekebalan

        // 1. Hentikan semua aksi Boss
        if (combatScript != null)
        {
            combatScript.StopAllCoroutines();
            combatScript.isAttacking = false;
            // Opsional: combatScript.ResetAttack(); jika ada fungsi ini
        }

        if (navMeshAgent != null && navMeshAgent.isOnNavMesh)
        {
            navMeshAgent.isStopped = true;
            navMeshAgent.velocity = Vector3.zero;
        }

        // 2. Putar Animasi Perubahan (Buat trigger "Phase2" di Animator)
        if (anim != null) anim.SetTrigger("Phase2");

        // 3. Munculkan VFX Transisi
        if (phase2VFXPrefab != null)
        {
            Instantiate(phase2VFXPrefab, transform.position, Quaternion.identity, transform);
        }

        // 4. Ganti Visual dari Biasa ke Emas
        if (normalVisuals != null) normalVisuals.SetActive(false);
        if (goldVisuals != null) goldVisuals.SetActive(true);

        // Tunggu bos selesai auman/animasi transisinya
        yield return new WaitForSeconds(phase2TransitionTime);

        // 5. Terapkan Buff Permanen
        ApplyPhase2Buffs();

        // 6. Lepas kekebalan dan biarkan bos lanjut bergerak
        isImmune = false;
        if (navMeshAgent != null && navMeshAgent.isOnNavMesh) navMeshAgent.isStopped = false;

        Debug.Log("<color=yellow>DragonBoar memasuki Phase 2!</color>");
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
}