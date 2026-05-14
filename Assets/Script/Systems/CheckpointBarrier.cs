using UnityEngine;

public class CheckpointBarrier : MonoBehaviour
{
    [Header("Syarat Membuka Jalan")]
    public int requiredKills = 10;
    private int currentKills = 0;

    [Header("Komponen Tembok")]
    [Tooltip("BoxCollider yang isTrigger-nya DIMATIKAN (Untuk menabrak player)")]
    public Collider solidWallCollider;

    [Tooltip("BoxCollider yang isTrigger-nya DIHIDUPKAN (Untuk mendeteksi player lewat)")]
    public Collider triggerZoneCollider;

    [Header("Pengaturan Spawn")]
    [Tooltip("Titik/Posisi di mana player akan hidup kembali")]
    public Transform newSpawnPointLocation;

    private bool isOpen = false;
    private bool spawnSet = false;

    // Mendaftarkan event saat tembok ini aktif
    void OnEnable()
    {
        EnemySimple.OnEnemyKilled += CountKill;
    }

    // Mencabut event saat tembok ini hancur/nonaktif agar tidak memori leak
    void OnDisable()
    {
        EnemySimple.OnEnemyKilled -= CountKill;
    }

    void CountKill()
    {
        if (isOpen) return;

        currentKills++;
        Debug.Log($"<color=yellow>Musuh dikalahkan: {currentKills} / {requiredKills}</color>");

        if (currentKills >= requiredKills)
        {
            OpenBarrier();
        }
    }

    void OpenBarrier()
    {
        isOpen = true;

        // Matikan tembok keras agar player bisa lewat
        if (solidWallCollider != null) solidWallCollider.enabled = false;

        // Nyalakan sensor trigger untuk auto-spawnpoint
        if (triggerZoneCollider != null) triggerZoneCollider.enabled = true;

        Debug.Log("<color=green>Jalan Terbuka! Silakan lewat.</color>");
    }

    private void OnTriggerEnter(Collider other)
    {
        // Jika jalan sudah terbuka dan player menyentuh area ini
        if (isOpen && !spawnSet && other.CompareTag("Player"))
        {
            PlayerStats pStats = other.GetComponent<PlayerStats>();
            if (pStats != null)
            {
                // Set posisi spawn point baru
                pStats.SetSpawnPoint(newSpawnPointLocation.position);
                spawnSet = true;
                Debug.Log("<color=cyan>Checkpoint Tersimpan! Auto-Spawnpoint aktif.</color>");
            }
        }
    }
}