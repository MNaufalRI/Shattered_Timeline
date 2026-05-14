using System.Collections.Generic;
using UnityEngine;


public class CharacterVFXSpawner : MonoBehaviour
{
    [System.Serializable]
    public class SlashConfig
    {
        public string attackName;
        public GameObject slashPrefab;
        public Vector3 rotationOffset;
    }
    [Header("Slash VFX Configurations")]
    [Tooltip("Titik referensi tempat VFX muncul (misal: objek kosong di depan dada karakter)")]
    [SerializeField] private Transform slashSpawnPoint;
    [SerializeField] private bool followWeapon = false;

    [SerializeField] private List<SlashConfig> slashConfigs = new List<SlashConfig>();

    // Fungsi ini SEKARANG bisa dipanggil oleh Animation Event
    public void TriggerSlashVFX(int attackIndex)
    {
        if (attackIndex < 0 || attackIndex >= slashConfigs.Count)
        {
            Debug.LogWarning("Index Slash Config tidak valid!");
            return;
        }

        SlashConfig config = slashConfigs[attackIndex];

        if (config.slashPrefab != null && slashSpawnPoint != null)
        {
            // Karena script ini ada di karakter, kita bisa langsung pakai transform.rotation
            Quaternion baseRotation = transform.rotation;

            // Tambahkan rotasi dari konfigurasi inspector
            Quaternion finalRotation = baseRotation * Quaternion.Euler(config.rotationOffset);

            GameObject slash = Instantiate(config.slashPrefab, slashSpawnPoint.position, finalRotation);

            if (followWeapon)
                slash.transform.SetParent(slashSpawnPoint);

            Destroy(slash, 0.5f);
        }
    }
}