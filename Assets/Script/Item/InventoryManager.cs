using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    public List<ItemData> Items = new List<ItemData>();

    public Transform ItemContent;
    public GameObject InventoryItem;

    void Awake()
    {
        Instance = this;
    }

    public void Add(ItemData item)
    {
        Items.Add(item);
        RefreshUI();
    }

    public void Remove(ItemData item)
    {
        Items.Remove(item);
        RefreshUI();
    }

    public void RefreshUI()
    {
        if (ItemContent == null || InventoryItem == null)
        {
            Debug.LogError("ItemContent / InventoryItem belum di assign!");
            return;
        }

        foreach (Transform child in ItemContent)
        {
            Destroy(child.gameObject);
        }

        foreach (var item in Items)
        {
            GameObject obj = Instantiate(InventoryItem, ItemContent);

            InventoryItemUI ui = obj.GetComponent<InventoryItemUI>();

            if (ui == null)
            {
                Debug.LogError("InventoryItemUI tidak ada di prefab!");
                return;
            }

            ui.Setup(item);
        }
    }
}