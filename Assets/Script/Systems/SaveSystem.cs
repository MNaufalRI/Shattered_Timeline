using UnityEngine;
using System.IO;
using System.Collections.Generic;

public static class SaveSystem
{
    private static string path = Application.persistentDataPath + "/gamesave.json";

    public static void SaveAllData(List<InventorySlot> slots, int level, float currentExp, float maxExp, float maxHealth, float maxMana, float attackDamage)
    {
        GameData data = new GameData();

        if (slots != null)
        {
            foreach (var slot in slots)
            {
                if (slot.item != null)
                {
                    for (int i = 0; i < slot.amount; i++)
                    {
                        data.itemNames.Add(slot.item.name);
                    }
                }
            }
        }

        data.level = level;
        data.currentExp = currentExp;
        data.maxExp = maxExp;
        data.maxHealth = maxHealth;
        data.maxMana = maxMana;
        data.attackDamage = attackDamage;

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(path, json);
    }

    public static GameData LoadData()
    {
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            return JsonUtility.FromJson<GameData>(json);
        }
        return null;
    }

    public static List<string> LoadInventoryNames()
    {
        GameData data = LoadData();
        if (data != null)
        {
            return data.itemNames;
        }
        return null;
    }

    public static void ResetSave()
    {
        if (File.Exists(path))
        {
            File.Delete(path);
        }
        PlayerPrefs.DeleteAll();
    }

    [System.Serializable]
    public class GameData
    {
        public List<string> itemNames = new List<string>();
        public int level = 1;
        public float currentExp = 0f;
        public float maxExp = 100f;
        public float maxHealth = 100f;
        public float maxMana = 50f;
        public float attackDamage = 20f;
    }
}