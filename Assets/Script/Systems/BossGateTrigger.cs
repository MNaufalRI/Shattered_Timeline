using UnityEngine;

public class BossGateTrigger : MonoBehaviour
{
    [Header("Pengaturan Tujuan")]
    [Tooltip("Ketik nama Scene Boss yang ada di Build Settings")]
    public string bossSceneName = "BossScene"; // Ubah defaultnya sesuai nama scenemu

    [Header("Referensi Quest")]
    [Tooltip("Tarik GameObject yang memiliki script QuestManager ke sini")]
    public QuestManager questManager;

    // Fungsi ini terpanggil otomatis saat ada Collider yang menyentuh zona Is Trigger
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // 1. Cek apakah QuestManager sudah dimasukkan
            if (questManager != null)
            {
                // 2. Cek apakah quest terakhir (Investigate roar) sudah aktif
                if (questManager.currentQuest == QuestManager.QuestState.MainQuest2_CheckSound)
                {
                    Debug.Log("<color=green>Akses diizinkan. Memasuki area Boss...</color>");
                    LoadBossScene();
                }
                else
                {
                    // Player menyentuh trigger tapi quest belum selesai
                    Debug.Log("<color=red>Pintu Boss masih terkunci! Selesaikan quest sebelumnya.</color>");

                    // Opsional: Jika kamu punya UI peringatan, bisa dipanggil di sini
                    // misal: UIManager.Instance.ShowWarning("You must purge the area first!");
                }
            }
            else
            {
                Debug.LogWarning("QuestManager tidak ditemukan di Inspector! Pintu otomatis terbuka.");
                // Fallback: Jika lupa memasukkan QuestManager, pintu tetap bisa dibuka untuk testing
                LoadBossScene();
            }
        }
    }

    // Dipisahkan menjadi fungsi sendiri agar kodenya lebih rapi
    private void LoadBossScene()
    {
        if (LoadingManager.Instance != null)
        {
            LoadingManager.Instance.LoadScene(bossSceneName);
        }
        else
        {
            Debug.LogError("LoadingManager tidak ditemukan! Pastikan sudah ada di scene awal.");
        }
    }
}