using UnityEngine;
using Eliot.AgentComponents;

public class BossArenaTrigger : MonoBehaviour
{
    [Header("References")]
    public BossHealthUI bossUIManager;
    public AgentResources bossTarget; // Tarik objek Boss Gluttony ke sini di Inspector

    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !hasTriggered)
        {
            hasTriggered = true; // Agar tidak kepanggil berkali-kali

            // Nyalakan UI Boss!
            if (bossUIManager != null && bossTarget != null)
            {
                bossUIManager.ActivateBossUI(bossTarget);
            }
        }
    }
}