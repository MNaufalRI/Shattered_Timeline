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
    [Tooltip("Masukkan Image yang menampilkan Preview Target Weapon")]
    public Image targetWeaponPreview;
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

        bool isHoldingBase = false;
        if (PlayerWeaponManager.Instance != null)
        {
            isHoldingBase = PlayerWeaponManager.Instance.IsHoldingWeapon(currentRecipe.baseWeapon);
        }

        bool isHoldingIce = false;
        if (PlayerWeaponManager.Instance != null)
        {
            isHoldingIce = PlayerWeaponManager.Instance.IsHoldingWeapon(currentRecipe.upgradedWeapon);
        }

        bool hasUpgradedWeapon = isHoldingIce || CountItem(currentRecipe.upgradedWeapon) > 0;

        string mat1Color = currentMat1 >= currentRecipe.material1Amount ? "green" : "red";
        string mat2Color = currentMat2 >= currentRecipe.material2Amount ? "green" : "red";

        material1Text.text = $"{currentRecipe.material1.itemName} : <color={mat1Color}>{currentMat1} / {currentRecipe.material1Amount}</color>";
        material2Text.text = $"{currentRecipe.material2.itemName} : <color={mat2Color}>{currentMat2} / {currentRecipe.material2Amount}</color>";

        if (baseAxe3DObject != null) baseAxe3DObject.SetActive(true);

        if (hasUpgradedWeapon)
        {
            targetWeaponNameText.text = currentRecipe.upgradedWeapon.itemName;

            upgradeButton.interactable = false;
            upgradeButton.GetComponentInChildren<TextMeshProUGUI>().text = "Max Level";
            if (iceAxe3DObject != null) iceAxe3DObject.SetActive(true);
            if (targetWeaponPreview != null) targetWeaponPreview.color = Color.white;
        }
        else
        {
            if (isHoldingBase)
            {
                targetWeaponNameText.text = "???";

                if (iceAxe3DObject != null) iceAxe3DObject.SetActive(true);

                if (targetWeaponPreview != null) targetWeaponPreview.color = new Color(0, 0, 0, 0.6f);
            }
            else
            {
                targetWeaponNameText.text = "???";
                if (iceAxe3DObject != null) iceAxe3DObject.SetActive(false);
            }

            bool canCraft = isHoldingBase && currentMat1 >= currentRecipe.material1Amount && currentMat2 >= currentRecipe.material2Amount;
            upgradeButton.interactable = canCraft;
            upgradeButton.GetComponentInChildren<TextMeshProUGUI>().text = "Upgrade";
        }
    }

    public void OnClickUpgradeButton()
    {
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
        RemoveItemFromInventory(currentRecipe.material1, currentRecipe.material1Amount);
        RemoveItemFromInventory(currentRecipe.material2, currentRecipe.material2Amount);

        InventoryManager.Instance.Add(currentRecipe.upgradedWeapon);

        if (playerWeaponDamageDealer != null)
        {
            playerWeaponDamageDealer.ApplyUpgradedWeaponStats(currentRecipe.newHitVFX);
        }

        if (PlayerWeaponManager.Instance != null)
        {
            PlayerWeaponManager.Instance.EquipUpgradedWeapon();
        }

        InventoryManager.Instance.RefreshUI();
        UpdateCraftingUI();

        Debug.Log("<color=cyan>Upgrade Berhasil! Senjata baru telah terbuka.</color>");
    }

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
        InventoryManager.Instance.Remove(itemToRemove, amountToRemove);
    }
}