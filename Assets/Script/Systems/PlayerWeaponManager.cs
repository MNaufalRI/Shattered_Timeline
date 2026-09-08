using UnityEngine;

public class PlayerWeaponManager : MonoBehaviour
{
    public static PlayerWeaponManager Instance;

    public ItemData baseWeapon;
    public ItemData upgradedWeapon;

    public GameObject baseWeaponObject;
    public GameObject upgradedWeaponObject;

    public GameObject skill3IconUI;

    private ItemData currentWeapon;

    private void Awake()
    {
        Instance = this;
    }

    public void EquipBaseWeapon()
    {
        currentWeapon = baseWeapon;

        if (baseWeaponObject != null) baseWeaponObject.SetActive(true);
        if (upgradedWeaponObject != null) upgradedWeaponObject.SetActive(false);
        if (skill3IconUI != null) skill3IconUI.SetActive(false);

        if (baseWeaponObject != null)
        {
            DamageDealer baseDealer = baseWeaponObject.GetComponentInChildren<DamageDealer>();
            PlayerControl playerCtrl = GetComponent<PlayerControl>();

            if (playerCtrl != null && baseDealer != null)
            {
                playerCtrl.UpdateWeaponDamageDealer(baseDealer);
            }
        }
    }

    public void EquipUpgradedWeapon()
    {
        currentWeapon = upgradedWeapon;

        if (baseWeaponObject != null) baseWeaponObject.SetActive(false);
        if (upgradedWeaponObject != null) upgradedWeaponObject.SetActive(true);
        if (skill3IconUI != null) skill3IconUI.SetActive(true);

        if (upgradedWeaponObject != null)
        {
            DamageDealer newIceAxeDealer = upgradedWeaponObject.GetComponentInChildren<DamageDealer>();
            PlayerControl playerCtrl = GetComponent<PlayerControl>();

            if (playerCtrl != null && newIceAxeDealer != null)
            {
                playerCtrl.UpdateWeaponDamageDealer(newIceAxeDealer);
            }
        }
    }

    public bool IsHoldingWeapon(ItemData weaponToCheck)
    {
        return currentWeapon == weaponToCheck;
    }

    public bool IsWeaponMaxLevel()
    {
        return currentWeapon == upgradedWeapon;
    }
}