using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CraftingManager : MonoBehaviour
{
    public WeaponUpgradeRecipe currentRecipe;
    public DamageDealer playerWeaponDamageDealer;

    public TextMeshProUGUI targetWeaponNameText;
    public TextMeshProUGUI material1Text;
    public TextMeshProUGUI material2Text;
    public TextMeshProUGUI effectsText;
    public Button upgradeButton;

    public Image targetWeaponPreview;
    public GameObject baseAxe3DObject;
    public GameObject iceAxe3DObject;

    private void OnEnable()
    {
        UpdateCraftingUI();
    }

    private void Update()
    {
        UpdateCraftingUI();
    }

    public void UpdateCraftingUI()
    {
        if (currentRecipe == null) return;
        if (InventoryManager.Instance == null) return;

        if (effectsText != null)
        {
            effectsText.text = currentRecipe.upgradeDescription;
        }

        int currentMat1 = CountItem(currentRecipe.material1);
        int currentMat2 = CountItem(currentRecipe.material2);

        bool isHoldingBase = false;
        bool isHoldingIce = false;

        if (PlayerWeaponManager.Instance != null)
        {
            isHoldingBase = PlayerWeaponManager.Instance.IsHoldingWeapon(currentRecipe.baseWeapon);
            isHoldingIce = PlayerWeaponManager.Instance.IsHoldingWeapon(currentRecipe.upgradedWeapon);
        }

        bool hasUpgradedWeapon = isHoldingIce;

        string mat1Color = currentMat1 >= currentRecipe.material1Amount ? "green" : "red";
        string mat2Color = currentMat2 >= currentRecipe.material2Amount ? "green" : "red";

        if (material1Text != null)
            material1Text.text = $"{currentRecipe.material1.itemName} : <color={mat1Color}>{currentMat1} / {currentRecipe.material1Amount}</color>";
        if (material2Text != null)
            material2Text.text = $"{currentRecipe.material2.itemName} : <color={mat2Color}>{currentMat2} / {currentRecipe.material2Amount}</color>";

        if (hasUpgradedWeapon)
        {
            if (targetWeaponNameText != null) targetWeaponNameText.text = currentRecipe.upgradedWeapon.itemName;

            if (upgradeButton != null)
            {
                upgradeButton.interactable = false;
                upgradeButton.GetComponentInChildren<TextMeshProUGUI>().text = "Max Level";
            }

            if (baseAxe3DObject != null) baseAxe3DObject.SetActive(false);
            if (iceAxe3DObject != null) iceAxe3DObject.SetActive(true);
            if (targetWeaponPreview != null) targetWeaponPreview.color = Color.white;
        }
        else
        {
            if (baseAxe3DObject != null) baseAxe3DObject.SetActive(true);

            if (isHoldingBase)
            {
                if (targetWeaponNameText != null) targetWeaponNameText.text = "???";
                if (iceAxe3DObject != null) iceAxe3DObject.SetActive(true);
                if (targetWeaponPreview != null) targetWeaponPreview.color = new Color(0, 0, 0, 0.6f);
            }
            else
            {
                if (targetWeaponNameText != null) targetWeaponNameText.text = "???";
                if (iceAxe3DObject != null) iceAxe3DObject.SetActive(false);
            }

            bool canCraft = isHoldingBase && currentMat1 >= currentRecipe.material1Amount && currentMat2 >= currentRecipe.material2Amount;
            if (upgradeButton != null)
            {
                upgradeButton.interactable = canCraft;
                upgradeButton.GetComponentInChildren<TextMeshProUGUI>().text = "Upgrade";
            }
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

        if (playerWeaponDamageDealer != null)
        {
            playerWeaponDamageDealer.ApplyUpgradedWeaponStats(currentRecipe.newHitVFX);
        }

        if (PlayerWeaponManager.Instance != null)
        {
            PlayerWeaponManager.Instance.EquipUpgradedWeapon();
        }

        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.RefreshUI();
        }

        UpdateCraftingUI();
    }

    private int CountItem(ItemData itemToFind)
    {
        if (itemToFind == null || InventoryManager.Instance == null) return 0;

        int totalCount = 0;

        foreach (var slot in InventoryManager.Instance.Slots)
        {
            if (slot.item != null)
            {
                if (slot.item.name == itemToFind.name)
                {
                    totalCount += slot.amount;
                }
            }
        }

        return totalCount;
    }

    private void RemoveItemFromInventory(ItemData itemToRemove, int amountToRemove)
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.Remove(itemToRemove, amountToRemove);
        }
    }
}