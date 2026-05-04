using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryItemUI : MonoBehaviour
{
    public TMP_Text itemName;
    public Image itemIcon;

    public void Setup(ItemData item)
    {
        itemName.text = item.itemName;
        itemIcon.sprite = item.icon;
    }
}