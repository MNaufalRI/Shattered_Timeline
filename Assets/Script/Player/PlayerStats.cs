using UnityEngine;
using System.Collections;

public class PlayerStats : MonoBehaviour
{
    public float maxHealth = 100f;
    public float currentHealth;
    public bool isInvincible = false;

    [Header("Attack Stats")]
    public float attackDamage = 20f;

    [Header("Respawn")]
    public Transform spawnPoint;
    public float respawnDelay = 2f;

    [SerializeField] ScreenFader fader;

    private Animator anim;
    private bool isDead = false;
    public bool IsDead() => isDead;

    private Player_Controlled_3 controller;

    void Awake()
    {
        currentHealth = maxHealth;

        anim = GetComponentInChildren<Animator>();
        controller = GetComponent<Player_Controlled_3>();
    }

    public void TakeDamage(float damage)
    {
        if (isDead || isInvincible) return;

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
            }
        }
    }

    void PlayerDie()
    {
        if (isDead) return;

        isDead = true;

        if (anim != null)
        {
            anim.SetTrigger("Die");
        }

        if (controller != null)
        {
            controller.canControl = false;
        }

        Debug.Log("Player Mati");

        StartCoroutine(RespawnRoutine());
    }

    IEnumerator RespawnRoutine()
    {
        if (fader != null)
            yield return StartCoroutine(fader.FadeOutWithDeathText());

        yield return new WaitForSeconds(1.5f);

        currentHealth = maxHealth;

        if (spawnPoint != null)
        {
            CharacterController cc = GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            transform.position = spawnPoint.position;

            if (cc != null) cc.enabled = true;
        }

        isDead = false;

        if (controller != null)
            controller.canControl = true;

        if (anim != null)
        {
            anim.ResetTrigger("Die");
            anim.Play("Locomotion");
        }

        if (fader != null)
            yield return StartCoroutine(fader.FadeIn());
    }
}