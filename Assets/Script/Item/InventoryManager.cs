using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    // UBAH: Sekarang menggunakan List of InventorySlot, bukan ItemData
    public List<InventorySlot> Slots = new List<InventorySlot>();

    [Header("UI References")]
    public Transform ItemContent;
    public GameObject InventoryItem;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        FindUIReferencesInNewScene();
        RefreshUI();
    }

    private void FindUIReferencesInNewScene()
    {
        GameObject contentObj = GameObject.Find("ItemContent");
        ItemContent = contentObj != null ? contentObj.transform : null;
    }

    // UBAH: Fungsi Add sekarang menerima jumlah (amount) dan mengecek tumpukan
    public void Add(ItemData itemToAdd, int amountToAdd = 1)
    {
        if (itemToAdd == null) return;

        foreach (var slot in Slots)
        {
            // PERBAIKAN: Gunakan .name
            if (slot.item != null && slot.item.name == itemToAdd.name)
            {
                slot.amount += amountToAdd;
                RefreshUI();
                return;
            }
        }

        Slots.Add(new InventorySlot(itemToAdd, amountToAdd));
        RefreshUI();
    }

    public void Remove(ItemData itemToRemove, int amountToRemove = 1)
    {
        if (itemToRemove == null) return;

        for (int i = Slots.Count - 1; i >= 0; i--)
        {
            // PERBAIKAN: Gunakan .name
            if (Slots[i].item != null && Slots[i].item.name == itemToRemove.name)
            {
                Slots[i].amount -= amountToRemove;

                if (Slots[i].amount <= 0)
                {
                    Slots.RemoveAt(i);
                }
                break;
            }
        }
        RefreshUI();
    }

    public void RefreshUI()
    {
        if (ItemContent == null || InventoryItem == null) return;

        for (int i = ItemContent.childCount - 1; i >= 0; i--)
        {
            Transform child = ItemContent.GetChild(i);
            child.SetParent(null);
            Destroy(child.gameObject);
        }

        foreach (var slot in Slots)
        {
            GameObject obj = Instantiate(InventoryItem, ItemContent);

            // UBAH: Kirim 'slot' secara utuh, BUKAN 'slot.item'
            obj.GetComponent<InventoryItemUI>()?.Setup(slot);
        }
    }
}