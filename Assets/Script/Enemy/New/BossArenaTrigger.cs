using UnityEngine;

public class BossArenaTrigger : MonoBehaviour
{
    [Header("References")]
    public BossHealthUI bossUIManager;

    // SEKARANG: Mengacu langsung ke script utama Boss (GluttonyBase)
    public DragonBoarStats bossTarget;

    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        // Pastikan tag Player sudah diatur pada objek pemain
        if (other.CompareTag("Player") && !hasTriggered)
        {
            if (bossUIManager != null && bossTarget != null)
            {
                hasTriggered = true; // Kunci agar tidak terpanggil berkali-kali

                // Kirim data Gluttony ke UI Manager
                bossUIManager.ActivateBossUI(bossTarget);

                Debug.Log($"<color=orange>Boss Battle Started: {bossTarget.name}</color>");
            }
        }
    }
}