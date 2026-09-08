using UnityEngine;

public class PlayerLevel : MonoBehaviour
{
    [Header("Level Settings")]
    public int currentLevel = 1;
    public int maxLevel = 50;
    public float currentExp = 0f;
    public float expToNextLevel = 100f;

    [Header("EXP Curve (per level butuh berapa EXP)")]
    public float expMultiplierPerLevel = 1.5f;

    [Header("Stat Growth per Level Up")]
    public float healthGainPerLevel = 10f;
    public float manaGainPerLevel = 5f;
    public float damageGainPerLevel = 2f;

    [Header("UI References")]
    public StatBarEXP expBar;
    public TMPro.TextMeshProUGUI levelText;
    public GameObject levelUpVFXPrefab;
    public Transform vfxSpawnPoint;

    private PlayerStats playerStats;

    void Awake()
    {
        playerStats = GetComponent<PlayerStats>();
    }

    void Start()
    {
        if (expBar != null) expBar.SetMaxValue(expToNextLevel);
        if (expBar != null) expBar.SetValue(currentExp);
        UpdateUI();
    }

    public void GainExp(float amount)
    {
        if (currentLevel >= maxLevel) return;

        currentExp += amount;
        if (expBar != null) expBar.SetValue(currentExp);

        while (currentExp >= expToNextLevel && currentLevel < maxLevel)
        {
            currentExp -= expToNextLevel;
            LevelUp();
        }
    }

    private void LevelUp()
    {
        currentLevel++;
        expToNextLevel *= expMultiplierPerLevel;

        if (playerStats != null)
        {
            playerStats.ApplyLevelUpBonus(healthGainPerLevel, manaGainPerLevel, damageGainPerLevel);
        }

        if (expBar != null)
        {
            expBar.SetMaxValue(expToNextLevel);
            expBar.SetValue(currentExp);
        }

        if (levelUpVFXPrefab != null && vfxSpawnPoint != null)
        {
            GameObject vfx = Instantiate(levelUpVFXPrefab, vfxSpawnPoint.position, Quaternion.identity);
            vfx.transform.SetParent(vfxSpawnPoint);
            Destroy(vfx, 3f);
        }

        UpdateUI();
    }

    private void UpdateUI()
    {
        if (levelText != null)
            levelText.text = $"Lv. {currentLevel}";
    }

    public void ResetToDefault()
    {
        currentLevel = 1;
        currentExp = 0f;
        expToNextLevel = 100f;

        if (expBar != null)
        {
            expBar.SetMaxValue(expToNextLevel);
            expBar.SetValue(currentExp);
        }

        UpdateUI();
    }

    public void LoadSavedData(int savedLevel, float savedExp, float savedMaxExp)
    {
        currentLevel = savedLevel;
        currentExp = savedExp;
        expToNextLevel = savedMaxExp;

        if (expBar != null)
        {
            expBar.SetMaxValue(expToNextLevel);
            expBar.SetValue(currentExp);
        }

        UpdateUI();
    }
}