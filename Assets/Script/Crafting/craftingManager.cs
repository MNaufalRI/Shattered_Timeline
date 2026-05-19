using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CraftingManager : MonoBehaviour
{
    [Header("Resep Saat Ini")]
    public WeaponUpgradeRecipe currentRecipe;
    public DamageDealer playerWeaponDamageDealer;

    [Header("Referensi UI Teks")]
    public TextMeshProUGUI targetWeaponNameText;
    public TextMeshProUGUI material1Text;
    public TextMeshProUGUI material2Text;
    public TextMeshProUGUI effectsText;
    public Button upgradeButton;

    [Header("Referensi UI Kamera & 3D")]
    [Tooltip("Masukkan RawImage yang menampilkan Render Texture Ice Axe")]
    public RawImage targetWeaponPreview;
    public GameObject baseAxe3DObject;
    public GameObject iceAxe3DObject;

    private void OnEnable()
    {
        UpdateCraftingUI();
    }

    public void UpdateCraftingUI()
    {
        if (currentRecipe == null) return;

        effectsText.text = currentRecipe.upgradeDescription;

        int currentMat1 = CountItem(currentRecipe.material1);
        int currentMat2 = CountItem(currentRecipe.material2);
        bool hasBaseWeapon = false;
        if (PlayerWeaponManager.Instance != null)
        {
            hasBaseWeapon = PlayerWeaponManager.Instance.IsHoldingWeapon(currentRecipe.baseWeapon);
        }
        bool hasUpgradedWeapon = CountItem(currentRecipe.upgradedWeapon) > 0;
        string mat1Color = currentMat1 >= currentRecipe.material1Amount ? "green" : "red";
        string mat2Color = currentMat2 >= currentRecipe.material2Amount ? "green" : "red";

        material1Text.text = $"{currentRecipe.material1.itemName} : <color={mat1Color}>{currentMat1} / {currentRecipe.material1Amount}</color>";
        material2Text.text = $"{currentRecipe.material2.itemName} : <color={mat2Color}>{currentMat2} / {currentRecipe.material2Amount}</color>";

        if (hasUpgradedWeapon)
        {
            // Jika sudah di-craft sebelumnya
            targetWeaponNameText.text = currentRecipe.upgradedWeapon.itemName;
            upgradeButton.interactable = false; // Matikan tombol
            upgradeButton.GetComponentInChildren<TextMeshProUGUI>().text = "Max Level";

            // Atur tampilan 3D
            if (baseAxe3DObject != null) baseAxe3DObject.SetActive(false);
            if (iceAxe3DObject != null) iceAxe3DObject.SetActive(true);

            // Hilangkan efek siluet di Render Texture
            if (targetWeaponPreview != null) targetWeaponPreview.color = Color.white;
        }
        else
        {
            // Jika belum di-craft (Masih Bayangan)
            targetWeaponNameText.text = "???";

            // Tetap nyalakan Ice Axe di depan kamera
            if (baseAxe3DObject != null) baseAxe3DObject.SetActive(false);
            if (iceAxe3DObject != null) iceAxe3DObject.SetActive(true);

            // Beri warna hitam transparan pada RawImage agar Ice Axe terlihat seperti siluet
            if (targetWeaponPreview != null) targetWeaponPreview.color = new Color(0, 0, 0, 0.9f);

            // Cek apakah bisa di-craft sekarang
            bool canCraft = hasBaseWeapon && currentMat1 >= currentRecipe.material1Amount && currentMat2 >= currentRecipe.material2Amount;
            upgradeButton.interactable = canCraft;
            upgradeButton.GetComponentInChildren<TextMeshProUGUI>().text = "Upgrade";
        }
    }

    public void OnClickUpgradeButton()
    {
        // Pengecekan ulang ganda demi keamanan
        int currentMat1 = CountItem(currentRecipe.material1);
        int currentMat2 = CountItem(currentRecipe.material2);
        bool hasBaseWeapon = false;
        if (PlayerWeaponManager.Instance != null)
        {
            hasBaseWeapon = PlayerWeaponManager.Instance.IsHoldingWeapon(currentRecipe.baseWeapon);
        }

        if (hasBaseWeapon && currentMat1 >= currentRecipe.material1Amount && currentMat2 >= currentRecipe.material2Amount)
        {
            ExecuteUpgrade();
        }
    }

    private void ExecuteUpgrade()
    {
        // 1. Hapus material dan senjata dasar dari inventory
        RemoveItemFromInventory(currentRecipe.material1, currentRecipe.material1Amount);
        RemoveItemFromInventory(currentRecipe.material2, currentRecipe.material2Amount);

        // 2. Tambahkan senjata baru ke inventory
        InventoryManager.Instance.Add(currentRecipe.upgradedWeapon);

        // 3. Terapkan efek damage baru ke musuh
        if (playerWeaponDamageDealer != null)
        {
            playerWeaponDamageDealer.ApplyUpgradedWeaponStats(currentRecipe.newHitVFX);
        }

        // 4. ---> TAMBAHAN BARU: Ganti visual senjata di tangan karakter! <---
        if (PlayerWeaponManager.Instance != null)
        {
            PlayerWeaponManager.Instance.EquipUpgradedWeapon();
        }

        // 5. Refresh UI Anvil agar berubah jadi "Max Level"
        InventoryManager.Instance.RefreshUI();
        UpdateCraftingUI();

        Debug.Log("<color=cyan>Upgrade Berhasil! Senjata baru telah terbuka.</color>");
    }

    // --- Fungsi Bantuan ---
    // --- Fungsi Bantuan ---
    private int CountItem(ItemData itemToFind)
    {
        if (itemToFind == null) return 0;

        int totalCount = 0;
        Debug.Log($"[CRAFTING] Mencari material bernama: '{itemToFind.name}'");

        foreach (var slot in InventoryManager.Instance.Slots)
        {
            if (slot.item != null)
            {
                Debug.Log($"[CRAFTING] Di tas ada item bernama: '{slot.item.name}' dengan jumlah {slot.amount}");

                if (slot.item.name == itemToFind.name)
                {
                    totalCount += slot.amount;
                }
            }
        }

        Debug.Log($"[CRAFTING] Total ditemukan untuk '{itemToFind.name}': {totalCount}");
        return totalCount;
    }

    private void RemoveItemFromInventory(ItemData itemToRemove, int amountToRemove)
    {
        // Langsung panggil fungsi Remove dari InventoryManager yang sudah kita perbarui
        InventoryManager.Instance.Remove(itemToRemove, amountToRemove);
    }
}