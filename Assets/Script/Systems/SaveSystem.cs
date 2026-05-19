using UnityEngine;
using System.IO;
using System.Collections.Generic;

public static class SaveSystem
{
    private static string path = Application.persistentDataPath + "/inventory_save.json";

    // PERBAIKAN: Mengubah List<ItemData> menjadi List<InventorySlot>
    public static void SaveInventory(List<InventorySlot> slots)
    {
        InventoryData data = new InventoryData();

        // Loop untuk membongkar setiap slot di dalam inventory
        foreach (var slot in slots)
        {
            if (slot.item != null)
            {
                // Masukkan nama item sebanyak jumlah (amount) yang ditumpuk di slot tersebut
                for (int i = 0; i < slot.amount; i++)
                {
                    data.itemNames.Add(slot.item.name);
                }
            }
        }

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(path, json);
        Debug.Log("<color=green>Data Tersimpan di:</color> " + path);
    }

    public static List<string> LoadInventoryNames()
    {
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            InventoryData data = JsonUtility.FromJson<InventoryData>(json);
            return data.itemNames;
        }
        return null;
    }

    [System.Serializable]
    private class InventoryData { public List<string> itemNames = new List<string>(); }
}