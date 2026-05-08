using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AbilitySlotUI : MonoBehaviour
{
    [Header("Settings")]
    public int skillIndex; // 1 untuk Skill 1, 2 untuk Skill 2
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
        // Mencari referensi di player
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

        UpdateCooldownUI();
        UpdateManaAvailability();
    }

    void UpdateCooldownUI()
    {
        float currentTimer = (skillIndex == 1) ? playerControl.Skill1Timer : playerControl.Skill2Timer;
        float maxCooldown = (skillIndex == 1) ? playerControl.skill1Cooldown : playerControl.skill2Cooldown;

        if (currentTimer > 0)
        {
            cooldownOverlay.enabled = true;
            cooldownText.enabled = true;

            // Menghitung persentase fill (0 sampai 1)
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
        float manaCost = (skillIndex == 1) ? playerControl.skill1ManaCost : playerControl.skill2ManaCost;

        // Jika mana tidak cukup, icon sedikit menggelap/merah
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