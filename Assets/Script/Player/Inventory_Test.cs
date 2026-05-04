using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryTest : MonoBehaviour
{
    public InventorySys inventory;
    public ItemData testItem;

    void Update()
    {
        if (Keyboard.current.tKey.wasPressedThisFrame)
        {
            inventory.AddItem(testItem, 1);
            Debug.Log("Item ditambahkan");
        }
    }
}