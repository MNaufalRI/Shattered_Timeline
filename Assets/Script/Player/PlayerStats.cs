using StarterAssets;
using System.Collections;
using UnityEngine;
using Eliot.AgentComponents;


public class PlayerStats : MonoBehaviour
{
    private EliotAgent eliotAgent;
    private AgentResources eliotResources;

    // Menggunakan Indexer [] sesuai skrip AgentResources
    public float currentHealth
    {
        get
        {
            if (eliotResources == null) return 100f;
            var res = eliotResources["Health"]; // Memanggil Indexer
            return res != null ? res.currentValue : 100f; // Menggunakan .currentValue
        }
    }

    public float maxHealth
    {
        get
        {
            if (eliotResources == null) return 100f;
            var res = eliotResources["Health"];
            // Di skripmu, nilai awal/maks biasanya adalah initialValue
            return res != null ? res.initialValue : 100f;
        }
    }

    public float currentMana
    {
        get
        {
            if (eliotResources == null) return 50f;
            var res = eliotResources["Mana"];
            return res != null ? res.currentValue : 50f;
        }
    }

    public float maxMana
    {
        get
        {
            if (eliotResources == null) return 50f;
            var res = eliotResources["Mana"];
            return res != null ? res.initialValue : 50f;
        }
    }

    [Header("Status")]
    public bool isInvincible = false;
    public bool isStunned = false;

    [Header("UI")]
    public StatBarUI healthBar;
    public StatBarUI manaBar;

    [Header("Respawn")]
    public Transform spawnPoint;
    public float respawnDelay = 2f;

    [Header("References")]
    [SerializeField] private ScreenFader fader;
    [SerializeField] private Animator heartAnim;

    [Header("Potion Effects")]
    private float potionRegenBonus = 0f;

    private Animator anim;
    private PlayerMovement2 controller;
    private bool isDead = false;
    public bool IsDead() => isDead;
    private float beatTimer = 0f;

    private float healthRegenAccumulator = 0f;
    private float manaRegenAccumulator = 0f;
    private bool isInitialized = false; // Flag untuk menunggu data siap
    private float lastHealth; // Untuk mengecek perubahan (Dirty Flag)
    private float lastMana;

    public float attackDamage
    {
        get
        {
            return eliotAgent != null ? eliotAgent["Attack", 20f] : 20f;
        }
    }

    void Awake()
    {
        eliotAgent = GetComponent<EliotAgent>();
        eliotResources = GetComponent<AgentResources>();
        anim = GetComponentInChildren<Animator>();
        controller = GetComponent<PlayerMovement2>();
    }

    IEnumerator Start()
    {
        isInitialized = false;
        yield return new WaitForSeconds(1f);

        lastHealth = currentHealth;
        lastMana = currentMana;

        UpdateUI();
        isInitialized = true;
    }

    void Update()
    {
        if (isDead || !isInitialized) return;

        UpdateHeartBeat();
        RegenMana();
        RegenHealth();

        // --- TAMBAHKAN INI: CEK KEMATIAN ---
        if (currentHealth <= 0)
        {
            PlayerDie();
            return; // Langsung keluar agar kode di bawahnya tidak jalan
        }

        // Strategi Optimasi Dirty Flag
        if (currentHealth != lastHealth || currentMana != lastMana)
        {
            UpdateUI();
            lastHealth = currentHealth;
            lastMana = currentMana;
        }
    }

    void UpdateUI()
    {
        if (healthBar != null) healthBar.SetValue(currentHealth);
        if (manaBar != null) manaBar.SetValue(currentMana);
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

    public void TakeDamage(float damage)
    {
        if (isDead || isInvincible || eliotResources == null) return;

        eliotResources.Action(new ResourceAction("Health", ResourceAffectionWay.Reduce, Mathf.RoundToInt(damage)));

        if (currentHealth <= 0)
        {
            PlayerDie();
        }
        else if (anim != null)
        {
            anim.ResetTrigger("Hit");
            anim.SetTrigger("Hit");
        }
        UpdateUI();
    }

    public bool UseMana(float amount)
    {
        if (currentMana < amount || eliotResources == null) return false;

        eliotResources.Action(new ResourceAction("Mana", ResourceAffectionWay.Reduce, Mathf.RoundToInt(amount)));
        UpdateUI();
        return true;
    }

    public void RestoreMana(float amount)
    {
        if (eliotResources == null) return;
        // Menggunakan ResourceAffectionWay.Increase
        eliotResources.Action(new ResourceAction("Mana", ResourceAffectionWay.Increase, Mathf.RoundToInt(amount)));
        UpdateUI();
    }

    void RegenMana()
    {
        if (currentMana < maxMana && !isDead && eliotResources != null)
        {
            manaRegenAccumulator += 0.5f * Time.deltaTime;
            if (manaRegenAccumulator >= 1f)
            {
                int addAmount = Mathf.FloorToInt(manaRegenAccumulator);
                eliotResources.Action(new ResourceAction("Mana", ResourceAffectionWay.Increase, addAmount));
                manaRegenAccumulator -= addAmount;
                UpdateUI();
            }
        }
    }

    void RegenHealth()
    {
        if (currentHealth < maxHealth && !isDead && eliotResources != null)
        {
            // Gabungkan regen asli (0.2f) dengan bonus potion
            float totalRegenSpeed = 0.2f + potionRegenBonus;

            healthRegenAccumulator += totalRegenSpeed * Time.deltaTime;

            if (healthRegenAccumulator >= 1f)
            {
                int addAmount = Mathf.FloorToInt(healthRegenAccumulator);
                eliotResources.Action(new ResourceAction("Health", ResourceAffectionWay.Increase, addAmount));
                healthRegenAccumulator -= addAmount;
                UpdateUI();
            }
        }
    }
    public void ApplyPotionEffect(float instantHeal, float regenAmount, float duration)
    {
        if (eliotResources == null || isDead) return;

        // 1. Instant Heal 25 HP
        eliotResources.Action(new ResourceAction("Health", ResourceAffectionWay.Increase, Mathf.RoundToInt(instantHeal)));

        // 2. Jalankan Buff Regen (+1 HP/dtk)
        StartCoroutine(PotionRegenRoutine(regenAmount, duration));
    }

    private IEnumerator PotionRegenRoutine(float amount, float duration)
    {
        potionRegenBonus += amount; // Tambah ke tumpukan
        yield return new WaitForSeconds(duration);
        potionRegenBonus -= amount; // Kurangi dari tumpukan setelah durasi habis

        if (potionRegenBonus < 0) potionRegenBonus = 0; // Safety check
    }

    void PlayerDie()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log("<color=black><b>PLAYER MATI!</b></color>");

        if (anim != null) anim.SetTrigger("Die");
        if (controller != null) controller.canMove = false;

        // Hentikan regenerasi agar tidak menambah darah saat animasi mati
        healthRegenAccumulator = 0;
        manaRegenAccumulator = 0;

        StartCoroutine(RespawnRoutine());
        UpdateUI();
    }

    IEnumerator RespawnRoutine()
    {
        if (fader != null) yield return StartCoroutine(fader.FadeOutWithDeathText());
        yield return new WaitForSeconds(respawnDelay);

        // 1. Reset HP dan Mana
        if (eliotResources != null)
        {
            eliotResources.ReplenishResource("Health", Mathf.RoundToInt(maxHealth));
            eliotResources.ReplenishResource("Mana", Mathf.RoundToInt(maxMana));
        }

        // 2. Teleportasi ke Spawn Point
        if (spawnPoint != null)
        {
            CharacterController cc = GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false; // Matikan CC agar teleportasi lancar
            transform.position = spawnPoint.position;
            if (cc != null) cc.enabled = true;
        }

        // 3. Reset Status Kematian
        isDead = false;

        // 4. HIDUPKAN KEMBALI KONTROLLER (PENTING!)
        if (controller != null)
        {
            controller.enabled = true; // Aktifkan skripnya kembali
            controller.canMove = true; // Beri izin untuk bergerak
        }

        // 5. Kembalikan Animasi ke Normal
        if (anim != null)
        {
            anim.ResetTrigger("Die");
            anim.Play("Locomotion"); // Pastikan nama state animasi jalanmu benar
        }

        UpdateUI();

        if (fader != null) yield return StartCoroutine(fader.FadeIn());
    }

    public void ApplyStun(float duration)
    {
        if (isDead || isInvincible) return;
        StartCoroutine(StunRoutine(duration));
    }

    private IEnumerator StunRoutine(float duration)
    {
        isStunned = true;
        if (controller != null) controller.canMove = false;
        yield return new WaitForSeconds(duration);
        isStunned = false;
        if (!isDead && controller != null) controller.canMove = true;
    }
}