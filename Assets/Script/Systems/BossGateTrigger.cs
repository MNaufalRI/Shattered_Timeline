using UnityEngine;

public class BossGateTrigger : MonoBehaviour
{
    public string bossSceneName = "BossScene";
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
                    LoadBossScene(other.gameObject);
                }
                else
                {
                    Debug.Log("<color=red>Pintu Boss masih terkunci! Selesaikan quest sebelumnya.</color>");
                }
            }
            else
            {
                Debug.LogWarning("QuestManager tidak ditemukan di Inspector! Pintu otomatis terbuka.");
                LoadBossScene(other.gameObject);
            }
        }
    }

    private void LoadBossScene(GameObject player)
    {
        PlayerLevel playerLevel = player.GetComponent<PlayerLevel>();
        PlayerStats playerStats = player.GetComponent<PlayerStats>();

        if (InventoryManager.Instance != null && playerLevel != null && playerStats != null)
        {
            SaveSystem.SaveAllData(
                InventoryManager.Instance.Slots,
                playerLevel.currentLevel,
                playerLevel.currentExp,
                playerLevel.expToNextLevel,
                playerStats.maxHealth,
                playerStats.maxMana,
                playerStats.attackDamage
            );
        }

        if (PlayerWeaponManager.Instance != null)
        {
            int isUpgraded = PlayerWeaponManager.Instance.IsWeaponMaxLevel() ? 1 : 0;
            PlayerPrefs.SetInt("SavedWeaponUpgraded", isUpgraded);
        }

        PlayerPrefs.SetInt("LoadFromTransition", 1);
        PlayerPrefs.Save();

        if (LoadingManager.Instance != null)
        {
            LoadingManager.Instance.LoadScene(bossSceneName);
        }
    }
}