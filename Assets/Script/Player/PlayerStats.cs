using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("Attack Stats")]
    public float attackDamage = 20f; 

    private Animator anim;
    private bool isDead = false;

    void Awake()
    {
        currentHealth = maxHealth;
        anim = GetComponentInChildren<Animator>();

        if (anim == null)
        {
            Debug.LogError("Animator TIDAK ditemukan di Player atau anaknya!");
        }
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;
        currentHealth -= damage;
        if (currentHealth <= 0) PlayerDie();
    }

    void PlayerDie()
    {
        if (isDead) return;
        isDead = true;
        if (anim != null) anim.SetTrigger("Die");
        Debug.Log(" Player Mati");
    }
}