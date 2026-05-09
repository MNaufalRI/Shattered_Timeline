using UnityEngine;

public class BossGateTrigger : MonoBehaviour
{
    [Header("Pengaturan Tujuan")]
    [Tooltip("Ketik nama Scene Boss yang ada di Build Settings")]
    public string bossSceneName = "BossScene"; // Ubah defaultnya sesuai nama scenemu

    // Fungsi ini terpanggil otomatis saat ada Collider yang menyentuh zona Is Trigger
    private void OnTriggerEnter(Collider other)
    {
        // Pastikan HANYA Player yang bisa memicu perpindahan scene (bukan musuh atau proyektil)
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player menyentuh gerbang! Memulai transisi ke: " + bossSceneName);

            // Mencari skrip GameSettings di scene saat ini
            GameSettings gameSettings = FindObjectOfType<GameSettings>();

            if (gameSettings != null)
            {
                // Memanggil fungsi TravelToBoss yang berisi Auto-Save dan Fade Out
                gameSettings.TravelToBoss(bossSceneName);
            }
            else
            {
                Debug.LogError("Gagal: Skrip GameSettings tidak ditemukan di scene ini!");
            }
        }
    }
}