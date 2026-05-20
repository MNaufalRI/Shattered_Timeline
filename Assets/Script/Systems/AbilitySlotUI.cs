using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AbilitySlotUI : MonoBehaviour
{
    [Header("Settings")]
    public int skillIndex;
    public Color lowManaColor = new Color(1, 0, 0, 0.5f);
    public Color normalColor = Color.white;

    [Header("UI Elements")]
    public Image iconImage;
    public Image cooldownOverlay;
    public TextMeshProUGUI cooldownText;

    private PlayerControl playerControl;
    private PlayerStats playerStats;

    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerControl = player.GetComponent<PlayerControl>();
            playerStats = player.GetComponent<PlayerStats>();
        }
    }

    void Update()
    {
        if (playerControl == null || playerStats == null) return;

        // Khusus Skill 3: Cek apakah sudah di-unlock
        if (skillIndex == 3)
        {
            bool isUnlocked = false;
            if (PlayerWeaponManager.Instance != null)
            {
                isUnlocked = PlayerWeaponManager.Instance.IsWeaponMaxLevel();
            }

            iconImage.enabled = isUnlocked;

            // Jika belum di-unlock, matikan semua elemen cooldown dan berhenti di sini
            if (!isUnlocked)
            {
                cooldownOverlay.enabled = false;
                cooldownText.enabled = false;
                return;
            }
        }

        // Hanya jalankan pembaruan jika senjata sudah di-unlock (atau jika skill 1 & 2)
        UpdateCooldownUI();
        UpdateManaAvailability();
    }

    void UpdateCooldownUI()
    {
        float currentTimer = 0f;
        float maxCooldown = 1f;

        // Ambil data SESUAI dengan index masing-masing
        switch (skillIndex)
        {
            case 1:
                currentTimer = playerControl.Skill1Timer;
                maxCooldown = playerControl.skill1Cooldown;
                break;
            case 2:
                currentTimer = playerControl.Skill2Timer;
                maxCooldown = playerControl.skill2Cooldown;
                break;
            case 3:
                currentTimer = playerControl.Skill3Timer;
                maxCooldown = playerControl.skill3Cooldown;
                break;
        }

        // Terapkan visual
        if (currentTimer > 0)
        {
            cooldownOverlay.enabled = true;
            cooldownText.enabled = true;

            cooldownOverlay.fillAmount = currentTimer / maxCooldown;
            cooldownText.text = Mathf.Ceil(currentTimer).ToString();
        }
        else
        {
            cooldownOverlay.enabled = false;
            cooldownText.enabled = false;
        }
    }

    void UpdateManaAvailability()
    {
        float manaCost = 0f;

        switch (skillIndex)
        {
            case 1: manaCost = playerControl.skill1ManaCost; break;
            case 2: manaCost = playerControl.skill2ManaCost; break;
            case 3: manaCost = playerControl.skill3ManaCost; break;
        }

        if (playerStats.currentMana < manaCost)
        {
            iconImage.color = lowManaColor;
        }
        else
        {
            iconImage.color = normalColor;
        }
    }
}