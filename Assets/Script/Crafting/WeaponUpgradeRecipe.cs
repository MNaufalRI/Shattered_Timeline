using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon Upgrade", menuName = "Crafting/Weapon Upgrade Recipe")]
public class WeaponUpgradeRecipe : ScriptableObject
{
    [Header("Senjata")]
    public ItemData baseWeapon;
    public ItemData upgradedWeapon;

    [Header("Material Syarat")]
    public ItemData material1;
    public int material1Amount;

    public ItemData material2;
    public int material2Amount;

    [Header("Info UI (Efek & Skill)")]
    [TextArea] public string upgradeDescription; // Isi dengan: "Damage +50\nSkill Baru: Frostbite"

    [Header("Data Internal Senjata")]
    public float newDamage;
    public GameObject newHitVFX;
}