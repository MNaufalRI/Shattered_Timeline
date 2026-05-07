using UnityEngine;

public class GatherableItem : Interactable
{
    [Header("Item Data")]
    public ItemData itemData;
    public int amount = 1;

    [Header("Gathering Settings")]
    public string itemName = "Tanaman Herbal";

    private bool isGathered = false;

    void Start()
    {
        promptMessage = "untuk mengumpulkan " + itemName;
    }

    public override void Interact()
    {
        if (isGathered) return;

        isGathered = true;

        if (InventoryManager.Instance != null && itemData != null)
        {
            InventoryManager.Instance.Add(itemData);
        }

        Debug.Log("Item diambil: " + itemName);

        Destroy(gameObject);
    }
}