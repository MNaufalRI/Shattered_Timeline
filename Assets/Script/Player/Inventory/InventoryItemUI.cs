using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryItemUI : MonoBehaviour
{
    public TMP_Text itemName;
    public Image itemIcon;
    public TMP_Text itemAmount; // TAMBAHAN: Komponen teks untuk angka stack (misal: x5)

    // UBAH: Sekarang menerima 'InventorySlot', bukan 'ItemData' saja
    public void Setup(InventorySlot slot)
    {
        // Ambil nama dan icon dari dalam slot
        itemName.text = slot.item.itemName;
        itemIcon.sprite = slot.item.icon;

        // Logika penampilan angka stack
        if (slot.amount > 1)
        {
            itemAmount.text = "x" + slot.amount.ToString();
            itemAmount.gameObject.SetActive(true); // Munculkan teks jika item lebih dari 1
        }
        else
        {
            // Opsional: Sembunyikan teks angka jika itemnya cuma ada 1 biji
            itemAmount.gameObject.SetActive(false);

            // Atau kalau mau tetap muncul tulisan x1, ganti jadi:
            // itemAmount.text = "x1";
            // itemAmount.gameObject.SetActive(true);
        }
    }
}