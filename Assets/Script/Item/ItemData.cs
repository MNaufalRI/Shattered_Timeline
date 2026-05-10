using UnityEngine;

public enum ItemType
{
    Material,
    Consumable
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
}