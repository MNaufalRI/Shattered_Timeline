using UnityEngine;

public class QuestAreaTrigger : MonoBehaviour
{
    public QuestManager questManager;
    public Transform newSpawnPointLocation;

    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!hasTriggered && other.CompareTag("Player"))
        {
            // 1. Set Spawnpoint baru di PlayerStats
            PlayerStats pStats = other.GetComponent<PlayerStats>();
            if (pStats != null && newSpawnPointLocation != null)
            {
                pStats.SetSpawnPoint(newSpawnPointLocation.position);
                Debug.Log("<color=cyan>Checkpoint Tersimpan!</color>");
            }

            // 2. Beri tahu Quest Manager bahwa player sudah sampai di area baru
            if (questManager != null)
            {
                questManager.AdvanceToMainQuest2();
            }

            // 3. Matikan trigger ini agar tidak dipanggil berkali-kali
            hasTriggered = true;
            GetComponent<Collider>().enabled = false;
        }
    }
}