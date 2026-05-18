using UnityEngine;

public enum ItemType
{
    Material,
    Consumable,
    Weapon // <--- TAMBAHAN: Tipe baru untuk senjata
}

[CreateAssetMenu(fileName = "New Item", menuName = "Game/Item")]
public class ItemData : ScriptableObject
{
    public string itemName;
    public Sprite icon;
    public ItemType itemType;

    [Header("Stack")]
    public bool isStackable = true;
    public int maxStack = 99;

    [Header("Description")]
    [TextArea]
    public string description;

    // =================================================================
    // --- TAMBAHAN KHUSUS UNTUK SENJATA ---
    // (Kolom di bawah ini cukup diisi jika ItemType diatur ke 'Weapon')
    // =================================================================

    [Header("Weapon Stats")]
    [Tooltip("Nilai damage dasar dari senjata ini")]
    public float weaponDamage;

    [Tooltip("Prefab model 3D senjata yang akan dimunculkan di tangan Player")]
    public GameObject weaponPrefab;

    [Header("Weapon Skills")]
    [Tooltip("Nama skill aktif pertama")]
    public string skill1Name = "Basic Attack";
    [Tooltip("Nama skill aktif kedua")]
    public string skill2Name = "Heavy Smash";
    [Tooltip("Nama skill ketiga (Bisa dikosongkan untuk senjata awal)")]
    public string skill3Name;
}