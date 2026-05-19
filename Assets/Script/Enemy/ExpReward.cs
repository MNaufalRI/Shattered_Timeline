using UnityEngine;

public class ExpReward : MonoBehaviour
{
    [Header("EXP Settings")]
    public float expAmount = 20f;

    // Dipanggil saat musuh mati
    public void GiveExp()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        PlayerLevel playerLevel = player.GetComponent<PlayerLevel>();
        if (playerLevel != null)
        {
            playerLevel.GainExp(expAmount);
            Debug.Log($"<color=cyan>+{expAmount} EXP diterima!</color>");
        }
    }
}