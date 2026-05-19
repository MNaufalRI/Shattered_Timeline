using UnityEngine;

public class PlayerWeaponManager : MonoBehaviour
{
    public static PlayerWeaponManager Instance;

    [Header("Model Senjata di Tangan Karakter")]
    public GameObject baseAxe3DModel;
    public GameObject iceAxe3DModel;

    [Header("Item Data Referensi")]
    [Tooltip("Tarik file ItemData Kapak Dasar dari folder Resources ke sini")]
    public ItemData baseAxeData;
    [Tooltip("Tarik file ItemData Kapak Es dari folder Resources ke sini")]
    public ItemData iceAxeData;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    // FUNGSI BARU: Untuk mengecek apakah player sedang memakai senjata tertentu
    public bool IsHoldingWeapon(ItemData weaponToCheck)
    {
        if (weaponToCheck == null) return false;

        // Jika resep mencari Kapak Dasar, dan Kapak Dasar di tangan sedang aktif (true)
        if (baseAxeData != null && weaponToCheck.name == baseAxeData.name)
        {
            return baseAxe3DModel != null && baseAxe3DModel.activeSelf;
        }

        // Jika resep mencari Kapak Es (misal untuk upgrade ke level selanjutnya nanti)
        if (iceAxeData != null && weaponToCheck.name == iceAxeData.name)
        {
            return iceAxe3DModel != null && iceAxe3DModel.activeSelf;
        }

        return false;
    }

    public void EquipUpgradedWeapon()
    {
        // 1. Matikan kapak lama di tangan
        if (baseAxe3DModel != null) baseAxe3DModel.SetActive(false);

        // 2. Nyalakan kapak es di tangan
        if (iceAxe3DModel != null)
        {
            iceAxe3DModel.SetActive(true);

            // --- PERBAIKAN DI SINI: Gunakan GetComponentInChildren ---
            // Unity akan otomatis mengubek-ubek isi child dari Ice Axe sampai ketemu DamageDealer
            DamageDealer newIceAxeDealer = iceAxe3DModel.GetComponentInChildren<DamageDealer>();

            // 3. Kirimkan komponen tersebut ke PlayerControl
            PlayerControl playerCtrl = GetComponent<PlayerControl>();

            if (playerCtrl != null && newIceAxeDealer != null)
            {
                playerCtrl.UpdateWeaponDamageDealer(newIceAxeDealer);
                Debug.Log($"<color=lime>[SUCCESS]</color> Berhasil menemukan DamageDealer di child milik {newIceAxeDealer.gameObject.name}!");
            }
            else
            {
                if (playerCtrl == null) Debug.LogError("PlayerCtrl tidak ditemukan di tubuh Player!");
                if (newIceAxeDealer == null) Debug.LogError("Gagal menemukan DamageDealer di dalam child Ice Axe! Periksa kembali apakah scriptnya sudah terpasang.");
            }
        }
    }
}