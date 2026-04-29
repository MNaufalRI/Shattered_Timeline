using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("Attack Stats")]
    public float attackDamage = 20f;

    private Animator anim;
    private bool isDead = false;
    public bool IsDead() => isDead;

    void Awake()
    {
        currentHealth = maxHealth;
        anim = GetComponentInChildren<Animator>();
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            PlayerDie();
        }
        else
        {
            if (anim != null)
            {
                anim.ResetTrigger("Hit");
                anim.SetTrigger("Hit");
                Debug.Log("Trigger Hit dinyalakan!");
            }
        }
    }

    void PlayerDie()
    {
        if (isDead) return;
        isDead = true;

        if (anim != null)
        {
            anim.Play("Die", 0, 0f);
        }

        Debug.Log("Player Mati");

        Player_Controlled_2 controller = GetComponent<Player_Controlled_2>();
        if (controller != null)
        {
            controller.canControl = false;
        }
    }
}