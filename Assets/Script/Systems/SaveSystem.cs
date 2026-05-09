using UnityEngine;
using System.IO;
using System.Collections.Generic;

public static class SaveSystem
{
    private static string path = Application.persistentDataPath + "/inventory_save.json";

    public static void SaveInventory(List<ItemData> items)
    {
        InventoryData data = new InventoryData();
        foreach (var item in items) data.itemNames.Add(item.name);

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