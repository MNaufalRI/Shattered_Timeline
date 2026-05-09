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
        // Singleton & Persistence
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Add(ItemData item) { Items.Add(item); RefreshUI(); }
    public void Remove(ItemData item) { Items.Remove(item); RefreshUI(); }

    public void RefreshUI()
    {
        // Cari ItemContent baru jika pindah scene (karena yang lama hancur)
        if (ItemContent == null)
        {
            GameObject contentObj = GameObject.Find("ItemContent");
            if (contentObj != null) ItemContent = contentObj.transform;
        }

        if (ItemContent == null || InventoryItem == null) return;

        foreach (Transform child in ItemContent) Destroy(child.gameObject);

        foreach (var item in Items)
        {
            GameObject obj = Instantiate(InventoryItem, ItemContent);
            obj.GetComponent<InventoryItemUI>()?.Setup(item);
        }
    }
}