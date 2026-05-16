using StarterAssets;
using System.Collections;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Basic Stats")]
    [SerializeField] private float _maxHealth = 100f;
    [SerializeField] private float _currentHealth = 100f;
    [SerializeField] private float _maxMana = 50f;
    [SerializeField] private float _currentMana = 50f;
    [SerializeField] private float _baseDamage = 20f;

    public float currentHealth => _currentHealth;
    public float maxHealth => _maxHealth;
    public float currentMana => _currentMana;
    public float maxMana => _maxMana;
    public float attackDamage => _baseDamage;

    [Header("Status")]
    public bool isInvincible = false;
    public bool isStunned = false;

    [Header("UI")]
    public StatBarUI healthBar;
    public StatBarUI manaBar;

    [Header("Respawn Settings")]
    public float respawnDelay = 2f;
    public Vector3 currentSpawnPoint;

    [Header("References")]
    [SerializeField] private ScreenFader fader;
    [SerializeField] private Animator heartAnim;

    [Header("Potion Effects")]
    private float potionRegenBonus = 0f;

    [Header("VFX")]
    public GameObject potionVFXPrefab;
    public GameObject DebuffVFXPrefab;

    public Transform vfxSpawnPoint;

    private Animator anim;
    private PlayerMovement2 controller;
    public bool isDead { get; private set; } = false;
    public bool IsDead() => isDead;
    private float beatTimer = 0f;

    private float healthRegenAccumulator = 0f;
    private float manaRegenAccumulator = 0f;
    private float lastHealth;
    private float lastMana;

    [Header("Potion Inventory")]
    public int maxPotions = 3;
    public int currentPotions = 3;

    void Awake()
    {
        anim = GetComponentInChildren<Animator>();
        controller = GetComponent<PlayerMovement2>();
    }

    void Start()
    {
        _currentHealth = _maxHealth;
        _currentMana = _maxMana;
        lastHealth = _currentHealth;
        lastMana = _currentMana;

        currentPotions = maxPotions;

        PlayerControl pControl = GetComponent<PlayerControl>();
        if (pControl != null)
        {
            pControl.RefreshPotionUI(currentPotions);
        }

        UpdateUI();
    }

    void Update()
    {
        if (isDead) return;

        UpdateHeartBeat();
        RegenMana();
        RegenHealth();

        if (_currentHealth <= 0)
        {
            PlayerDie();
            return;
        }

        if (_currentHealth != lastHealth || _currentMana != lastMana)
        {
            UpdateUI();
            lastHealth = _currentHealth;
            lastMana = _currentMana;
        }
    }

    void UpdateUI()
    {
        if (healthBar != null) healthBar.SetValue(_currentHealth);
        if (manaBar != null) manaBar.SetValue(_currentMana);
    }

    void UpdateHeartBeat()
    {
        if (heartAnim == null) return;
        float hpPercent = _currentHealth / _maxHealth;
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
        if (isDead || isInvincible) return;

        _currentHealth -= damage;
        _currentHealth = Mathf.Clamp(_currentHealth, 0, _maxHealth);

        if (_currentHealth <= 0)
        {
            PlayerDie();
        }
        else if (anim != null && !isStunned)
        {
            anim.ResetTrigger("Hit");
            anim.SetTrigger("Hit");
        }
        UpdateUI();
    }

    public bool UseMana(float amount)
    {
        if (_currentMana < amount) return false;
        _currentMana -= amount;
        UpdateUI();
        return true;
    }

    public void RestoreMana(float amount)
    {
        _currentMana = Mathf.Min(_currentMana + amount, _maxMana);
        UpdateUI();
    }

    void RegenMana()
    {
        if (_currentMana < _maxMana && !isDead)
        {
            manaRegenAccumulator += 0.5f * Time.deltaTime;
            if (manaRegenAccumulator >= 1f)
            {
                float addAmount = Mathf.Floor(manaRegenAccumulator);
                _currentMana = Mathf.Min(_currentMana + addAmount, _maxMana);
                manaRegenAccumulator -= addAmount;
                UpdateUI();
            }
        }
    }

    void RegenHealth()
    {
        if (_currentHealth < _maxHealth && !isDead)
        {
            float totalRegenSpeed = 0.2f + potionRegenBonus;
            healthRegenAccumulator += totalRegenSpeed * Time.deltaTime;

            if (healthRegenAccumulator >= 1f)
            {
                float addAmount = Mathf.Floor(healthRegenAccumulator);
                _currentHealth = Mathf.Min(_currentHealth + addAmount, _maxHealth);
                healthRegenAccumulator -= addAmount;
                UpdateUI();
            }
        }
    }

    public void ApplyPotionEffect(float instantHeal, float regenAmount, float duration)
    {
        if (isDead) return;
        _currentHealth = Mathf.Min(_currentHealth + instantHeal, _maxHealth);

        if (potionVFXPrefab != null)
        {
            Vector3 spawnPos = vfxSpawnPoint != null ? vfxSpawnPoint.position : transform.position;
            Quaternion spawnRot = vfxSpawnPoint != null ? vfxSpawnPoint.rotation : Quaternion.identity;
            GameObject vfxInstance = Instantiate(potionVFXPrefab, spawnPos, spawnRot);
            vfxInstance.transform.SetParent(vfxSpawnPoint != null ? vfxSpawnPoint : transform);
            Destroy(vfxInstance, duration > 0 ? duration : 3f);
        }
        StartCoroutine(PotionRegenRoutine(regenAmount, duration));
    }

    private IEnumerator PotionRegenRoutine(float amount, float duration)
    {
        potionRegenBonus += amount;
        yield return new WaitForSeconds(duration);
        potionRegenBonus -= amount;
        if (potionRegenBonus < 0) potionRegenBonus = 0;
    }

    void PlayerDie()
    {
        if (isDead) return;
        isDead = true;

        if (anim != null) anim.SetTrigger("Die");
        if (controller != null) controller.canMove = false;

        healthRegenAccumulator = 0;
        manaRegenAccumulator = 0;

        ResetBossArena();

        StartCoroutine(RespawnRoutine());
        UpdateUI();
    }

    IEnumerator RespawnRoutine()
    {
        if (fader != null) yield return StartCoroutine(fader.FadeOutWithDeathText());
        yield return new WaitForSeconds(respawnDelay);

        _currentHealth = _maxHealth;
        _currentMana = _maxMana;
        currentPotions = maxPotions;
        PlayerControl pControl = GetComponent<PlayerControl>();
        if (pControl != null)
        {
            pControl.RefreshPotionUI(currentPotions);
        }

        ClearDebuffs();

        CharacterController cc = GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;
        transform.position = currentSpawnPoint;
        if (cc != null) cc.enabled = true;

        isDead = false;
        if (controller != null)
        {
            controller.enabled = true;
            controller.canMove = true;
        }

        if (anim != null)
        {
            anim.ResetTrigger("Die");
            anim.Play("Locomotion");
        }

        UpdateUI();
        if (fader != null) yield return StartCoroutine(fader.FadeIn());
        StartCoroutine(SpawnImmunityRoutine(3f));
    }

    public void ApplyStun(float duration)
    {
        if (isDead || isInvincible) return;
        if (DebuffVFXPrefab != null)
        {
            Vector3 spawnPos = transform.position;
            Quaternion spawnRot = Quaternion.identity;

            GameObject vfxInstance = Instantiate(DebuffVFXPrefab, spawnPos, spawnRot);
            vfxInstance.transform.SetParent(this.transform);
            vfxInstance.transform.localPosition = new Vector3(0f, 0.34f, 0f);
            vfxInstance.transform.localRotation = Quaternion.identity;

            Destroy(vfxInstance, duration > 0 ? duration : 3f);
        }
        StartCoroutine(StunRoutine(duration));
    }

    private IEnumerator StunRoutine(float duration)
    {
        isStunned = true;

        var input = GetComponent<StarterAssetsInputs>();
        if (input != null)
        {
            input.move = Vector2.zero;
            input.jump = false;
        }

        if (controller != null) controller.canMove = false;
        if (anim != null) anim.SetBool("isStunned", true);

        yield return new WaitForSeconds(duration);

        if (!isDead)
        {
            isStunned = false;
            if (anim != null) anim.SetBool("isStunned", false);
            if (controller != null) controller.canMove = true;
        }
    }

    public void SetSpawnPoint(Vector3 newPos)
    {
        currentSpawnPoint = newPos;
    }

    private void ClearDebuffs()
    {
        isStunned = false;
        if (anim != null) anim.SetBool("isStunned", false);

        if (controller != null)
        {
            controller.MoveSpeed = 5.0f;   
            controller.SprintSpeed = 10.0f; 
        }
    }

    private IEnumerator SpawnImmunityRoutine(float duration)
    {
        isInvincible = true;
        Debug.Log("<color=cyan>Player is Immune for 3 Seconds!</color>");

        yield return new WaitForSeconds(duration);

        isInvincible = false;
        Debug.Log("<color=cyan>Player Immunity Ended.</color>");
    }

    private void ResetBossArena()
    {

        DragonBoarStats[] bosses = FindObjectsOfType<DragonBoarStats>();
        foreach (var boss in bosses)
        {
            boss.ResetBossState();
        }

        BossArenaTrigger[] triggers = FindObjectsOfType<BossArenaTrigger>();
        foreach (var trigger in triggers)
        {
            trigger.ResetTrigger();
        }
    }
}