using UnityEngine;
using UnityEngine.UI; // Wajib untuk mengakses komponen Image
using TMPro;
using Eliot.AgentComponents;
using DG.Tweening;

public class BossHealthUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject bossUIPanel;

    // GANTI SLIDER MENJADI IMAGE
    [SerializeField] private Image animatedHealthFill;
    [SerializeField] private TextMeshProUGUI bossNameText;

    [Header("Boss Data")]
    public AgentResources bossResources;
    public string bossName = "Gluttony";

    private float currentHealth;
    private float maxHealth;
    private bool isBossActive = false;

    void Start()
    {
        bossUIPanel.SetActive(false);
        if (bossNameText != null) bossNameText.text = bossName;
    }

    public void ActivateBossUI(AgentResources newBossResources)
    {
        bossResources = newBossResources;

        CanvasGroup cg = bossUIPanel.GetComponent<CanvasGroup>();
        if (cg != null)
        {
            cg.alpha = 1f; // Pastikan tidak transparan saat muncul
        }

        if (bossResources != null)
        {
            var hpRes = bossResources["Health"];
            if (hpRes != null)
            {
                maxHealth = hpRes.initialValue;
                currentHealth = hpRes.currentValue;

                // Hitung persentase awal (0.0 sampai 1.0)
                float healthPercentage = currentHealth / maxHealth;
                animatedHealthFill.fillAmount = healthPercentage;
            }
        }

        bossUIPanel.SetActive(true);
        isBossActive = true;
    }

    void Update()
    {
        if (!isBossActive || bossResources == null) return;

        var hpRes = bossResources["Health"];
        if (hpRes != null)
        {
            if (currentHealth != hpRes.currentValue)
            {
                currentHealth = hpRes.currentValue;

                // 1. Hitung persentase darah saat ini
                float targetFill = currentHealth / maxHealth;

                // 2. Animasikan perubahan Fill Amount menggunakan DOTween
                animatedHealthFill.DOFillAmount(targetFill, 0.3f).SetEase(Ease.OutQuad);

                if (currentHealth <= 0)
                {
                    HideBossUI();
                }
            }
        }
    }

    public void HideBossUI()
    {
        isBossActive = false;

        CanvasGroup cg = bossUIPanel.GetComponent<CanvasGroup>();
        if (cg != null)
        {
            cg.DOFade(0, 1f).OnComplete(() => bossUIPanel.SetActive(false));
        }
        else
        {
            bossUIPanel.SetActive(false);
        }
    }
}