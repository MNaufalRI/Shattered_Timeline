using UnityEngine;

public class InventoryUIRegistrar : MonoBehaviour
{
    [Header("Masukkan objek ItemContent yang sedang Non-Aktif ke sini")]
    public Transform targetItemContent;

    void Start()
    {
        // Pastikan InventoryManager abadi kita sudah ada
        if (InventoryManager.Instance != null && targetItemContent != null)
        {
            // Berikan alamat UI baru ke InventoryManager
            InventoryManager.Instance.ItemContent = targetItemContent;

            // Perbarui gambar item di tas
            InventoryManager.Instance.RefreshUI();

            Debug.Log("<color=yellow>UI Inventory berhasil didaftarkan secara manual!</color>");
        }
    }
}