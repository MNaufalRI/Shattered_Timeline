using UnityEngine;

public class InventoryTester : MonoBehaviour
{
    [Header("Item yang Mau Ditambahkan")]
    public ItemData itemToTest;
    public int amountToAdd = 5;

    [Header("Tombol Shortcut")]
    [Tooltip("Pencet tombol ini di keyboard saat playtest untuk inject item")]
    public KeyCode customHotKey = KeyCode.H;

    void Update()
    {
        // Jika game sedang berjalan dan kamu menekan tombol yang ditentukan (contoh: H)
        if (Input.GetKeyDown(customHotKey))
        {
            if (itemToTest == null)
            {
                Debug.LogWarning("InventoryTester: Masukkan file ItemData terlebih dahulu di Inspector!");
                return;
            }

            if (InventoryManager.Instance != null)
            {
                // Inject langsung ke sistem slot inventory yang baru
                InventoryManager.Instance.Add(itemToTest, amountToAdd);
                Debug.Log($"<color=yellow>[TEST]</color> Berhasil menambahkan {itemToTest.itemName} x{amountToAdd} secara manual.");
            }
            else
            {
                Debug.LogError("InventoryTester: InventoryManager tidak ditemukan di scene ini!");
            }
        }
    }
}