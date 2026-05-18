using UnityEngine;

public class BossGateTrigger : MonoBehaviour
{
    [Header("Pengaturan Tujuan")]
    [Tooltip("Ketik nama Scene Boss yang ada di Build Settings")]
    public string bossSceneName = "BossScene";

    [Header("Referensi Quest")]
    [Tooltip("Tarik GameObject yang memiliki script QuestManager ke sini")]
    public QuestManager questManager;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (questManager != null)
            {
                if (questManager.currentQuest == QuestManager.QuestState.MainQuest2_CheckSound)
                {
                    Debug.Log("<color=green>Akses diizinkan. Memasuki area Boss...</color>");
                    LoadBossScene();
                }
                else
                {
                    Debug.Log("<color=red>Pintu Boss masih terkunci! Selesaikan quest sebelumnya.</color>");

                }
            }
            else
            {
                Debug.LogWarning("QuestManager tidak ditemukan di Inspector! Pintu otomatis terbuka.");

                LoadBossScene();
            }
        }
    }

    private void LoadBossScene()
    {
        // --- TAMBAHAN KODE: Simpan inventory sebelum pindah scene ---
        if (InventoryManager.Instance != null)
        {
            SaveSystem.SaveInventory(InventoryManager.Instance.Items);
            Debug.Log("Inventory berhasil disimpan sebelum pindah ke boss stage.");
        }
        else
        {
            Debug.LogWarning("InventoryManager tidak ditemukan, inventory gagal disimpan.");
        }
        // -------------------------------------------------------------

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