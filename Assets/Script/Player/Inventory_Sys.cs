using System.Collections.Generic;
using UnityEngine;

public class InventorySys : MonoBehaviour
{

    public int maxSlot = 20;
    public List<InventorySlot> slots = new List<InventorySlot>();

    [System.Serializable]
    public class InventorySlot
    {
        public ItemData item;
        public int amount;

        public InventorySlot(ItemData item, int amount)
        {
            this.item = item;
            this.amount = amount;
        }
    }

    public bool AddItem(ItemData item, int amount = 1)
    {
        // kalau stackable → cari slot yang sama
        if (item.isStackable)
        {
            foreach (var slot in slots)
            {
                if (slot.item == item && slot.amount < item.maxStack)
                {
                    int spaceLeft = item.maxStack - slot.amount;
                    int addAmount = Mathf.Min(spaceLeft, amount);

                    slot.amount += addAmount;
                    amount -= addAmount;

                    if (amount <= 0)
                        return true;
                }
            }
        }

        // tambah slot baru
        while (amount > 0)
        {
            if (slots.Count >= maxSlot)
            {
                Debug.Log("Inventory Full!");
                return false;
            }

            int addAmount = item.isStackable ? Mathf.Min(item.maxStack, amount) : 1;

            slots.Add(new InventorySlot(item, addAmount));
            amount -= addAmount;
        }

        return true;
    }

    public void RemoveItem(ItemData item, int amount = 1)
    {
        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i].item == item)
            {
                slots[i].amount -= amount;

                if (slots[i].amount <= 0)
                {
                    slots.RemoveAt(i);
                }

                return;
            }
        }
    }
}