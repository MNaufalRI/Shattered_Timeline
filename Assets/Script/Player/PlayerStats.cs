using StarterAssets;
using System.Collections;
using UnityEngine;



public class PlayerStats : MonoBehaviour
{
    [Header("Health")]
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("Mana")]
    public float maxMana = 50f;
    public float currentMana;

    public bool isInvincible = false;

    [Header("Attack Stats")]
    public float attackDamage = 20f;

    [Header("UI")]
    public StatBarUI healthBar;
    public StatBarUI manaBar;

    [Header("Respawn")]
    public Transform spawnPoint;
    public float respawnDelay = 2f;

    [Header("Status Effects")]
    public bool isStunned = false;

    [SerializeField] ScreenFader fader;
    [SerializeField] private Animator heartAnim;

    private Animator anim;
    private bool isDead = false;
    public bool IsDead() => isDead;
    private float beatTimer = 0f;

    private PlayerMovement2 controller;

    void Awake()
    {
        currentHealth = maxHealth;
        currentMana = maxMana;

        anim = GetComponentInChildren<Animator>();
        controller = GetComponent<PlayerMovement2>();
    }

    void Start()
    {
        UpdateUI();

    }

    void Update()
    {
        UpdateHeartBeat();
        RegenMana();
        RegenHealth();
    }

    void UpdateUI()
    {
        if (healthBar != null)
            healthBar.SetValue(currentHealth);

        if (manaBar != null)
            manaBar.SetValue(currentMana);
    }

    void UpdateHeartBeat()
    {
        if (heartAnim == null) return;

        float hpPercent = currentHealth / maxHealth;

        float interval = Mathf.Lerp(0.2f, 6f, hpPercent);

        beatTimer += Time.deltaTime;

        if (beatTimer >= interval)
        {
            heartAnim.SetTrigger("Beat");
            beatTimer = 0f;
        }
    }

    // ================= DAMAGE =================

    public void TakeDamage(float damage)
    {
        Debug.Log("KENA DAMAGE: " + damage);
        Debug.Log("HP sekarang: " + currentHealth);

        if (isDead || isInvincible) return;

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        UpdateUI();

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

    // ================= MANA =================

    public bool UseMana(float amount)
    {
        if (currentMana < amount)
            return false;

        currentMana -= amount;
        currentMana = Mathf.Clamp(currentMana, 0, maxMana);

        UpdateUI();
        return true;
    }

    public void RestoreMana(float amount)
    {
        currentMana += amount;
        currentMana = Mathf.Clamp(currentMana, 0, maxMana);

        UpdateUI();
    }

    void RegenMana()
    {
        if (currentMana < maxMana && !isDead)
        {
            currentMana += 0.5f * Time.deltaTime;
            currentMana = Mathf.Clamp(currentMana, 0, maxMana);
            UpdateUI();
        }
    }

    void RegenHealth()
    {
        if (currentHealth < maxHealth && !isDead)
        {
            currentHealth += 0.2f * Time.deltaTime;
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
            UpdateUI();
        }
    }

    // ================= DEATH =================

    void PlayerDie()
    {
        if (isDead) return;

        isDead = true;

        if (anim != null)
            anim.SetTrigger("Die");

        if (controller != null)
            controller.canMove = false;

        Debug.Log("Player Mati");

        StartCoroutine(RespawnRoutine());
    }

    IEnumerator RespawnRoutine()
    {
        if (fader != null)
            yield return StartCoroutine(fader.FadeOutWithDeathText());

        yield return new WaitForSeconds(1.5f);

        currentHealth = maxHealth;
        currentMana = maxMana;

        UpdateUI();

        if (spawnPoint != null)
        {
            CharacterController cc = GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            transform.position = spawnPoint.position;

            if (cc != null) cc.enabled = true;
        }

        isDead = false;

        if (controller != null)
            controller.canMove = true;

        if (anim != null)
        {
            anim.ResetTrigger("Die");
            anim.Play("Locomotion");
        }

        if (fader != null)
            yield return StartCoroutine(fader.FadeIn());
    }

    // ================= STUN =================

    public void ApplyStun(float duration)
    {
        // Jangan di-stun kalau sudah mati atau sedang invincible
        if (isDead || isInvincible) return;

        StartCoroutine(StunRoutine(duration));
    }

    private IEnumerator StunRoutine(float duration)
    {
        isStunned = true;

        // Matikan pergerakan
        if (controller != null) controller.canMove = false;

        // Opsional: Jika kamu punya animasi Stun pusing-pusing, panggil di sini
        // if (anim != null) anim.SetTrigger("Stun");

        Debug.Log("<color=cyan>Player terkena STUN selama " + duration + " detik!</color>");

        yield return new WaitForSeconds(duration);

        isStunned = false;

        // Nyalakan pergerakan kembali (tapi pastikan player tidak mati saat sedang di-stun)
        if (!isDead && controller != null) controller.canMove = true;

        Debug.Log("<color=cyan>Efek STUN selesai!</color>");
    }
}