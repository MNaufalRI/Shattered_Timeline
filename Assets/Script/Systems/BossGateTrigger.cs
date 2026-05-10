using UnityEngine;

public class BossGateTrigger : MonoBehaviour
{
    [Header("Pengaturan Tujuan")]
    [Tooltip("Ketik nama Scene Boss yang ada di Build Settings")]
    public string bossSceneName = "BossScene"; // Ubah defaultnya sesuai nama scenemu

    // Fungsi ini terpanggil otomatis saat ada Collider yang menyentuh zona Is Trigger
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Langsung panggil LoadingManager yang sudah jadi Singleton
            if (LoadingManager.Instance != null)
            {
                LoadingManager.Instance.LoadScene(bossSceneName); //
            }
            else
            {
                Debug.LogError("LoadingManager tidak ditemukan! Pastikan sudah ada di scene awal.");
            }
        }
    }
}