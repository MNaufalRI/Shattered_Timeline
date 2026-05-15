using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class BossHealthUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject bossUIPanel;
    [SerializeField] private Image animatedHealthFill;
    [SerializeField] private TextMeshProUGUI bossNameText;

    [Header("Boss Reference")]
    public DragonBoarStats bossBase;
    public string bossName = "Gluttony";

    private float lastKnownHealth;
    private bool isBossActive = false;

    void Start()
    {
        bossUIPanel.SetActive(false);
        if (bossNameText != null) bossNameText.text = bossName;
    }

    // Panggil fungsi ini saat boss muncul/mulai bertarung
    public void ActivateBossUI(DragonBoarStats reference)
    {
        bossBase = reference;

        if (bossBase != null)
        {
            // Sinkronisasi data awal
            lastKnownHealth = bossBase.currentHealth;

            // Set Fill Amount awal secara instan tanpa animasi
            float healthPercentage = bossBase.currentHealth / bossBase.maxHealth;
            animatedHealthFill.fillAmount = healthPercentage;

            // Efek muncul (Fade In)
            CanvasGroup cg = bossUIPanel.GetComponent<CanvasGroup>();
            if (cg != null)
            {
                cg.DOKill(); // Hentikan animasi sebelumnya (jika ada)
                cg.alpha = 0f;
                bossUIPanel.SetActive(true);
                cg.DOFade(1, 0.5f);
            }
            else
            {
                bossUIPanel.SetActive(true);
            }

            isBossActive = true;
        }
    }

    void Update()
    {
        if (!isBossActive || bossBase == null) return;

        // Cek apakah darah berubah sejak frame terakhir
        if (lastKnownHealth != bossBase.currentHealth)
        {
            lastKnownHealth = bossBase.currentHealth;

            // 1. Hitung persentase darah (0.0 - 1.0)
            float targetFill = Mathf.Clamp01(bossBase.currentHealth / bossBase.maxHealth);

            // 2. Animasikan perubahan Fill Amount menggunakan DOTween
            animatedHealthFill.DOKill(); // Hentikan animasi darah sebelumnya agar tidak bug
            animatedHealthFill.DOFillAmount(targetFill, 0.3f).SetEase(Ease.OutQuad);

            // 3. Jika darah habis, sembunyikan UI
            if (bossBase.currentHealth <= 0)
            {
                DeactivateBossUI(); // <-- NAMA FUNGSI DIUBAH DI SINI
            }
        }
    }

    // --- NAMA FUNGSI DIUBAH DARI HideBossUI MENJADI DeactivateBossUI ---
    public void DeactivateBossUI()
    {
        if (!isBossActive) return;
        isBossActive = false;

        CanvasGroup cg = bossUIPanel.GetComponent<CanvasGroup>();
        if (cg != null)
        {
            cg.DOKill(); // Hentikan animasi muncul jika masih berjalan
            cg.DOFade(0, 1f).OnComplete(() => bossUIPanel.SetActive(false));
        }
        else
        {
            bossUIPanel.SetActive(false);
        }
    }
}