using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSettings : MonoBehaviour
{
    void Update()
    {
        // F1: Save Manual
        if (Input.GetKeyDown(KeyCode.F1))
        {
            SaveSystem.SaveInventory(InventoryManager.Instance.Items);
        }

        // F2: Load Manual
        if (Input.GetKeyDown(KeyCode.F2))
        {
            LoadInventoryData();
        }
    }

    public void LoadInventoryData()
    {
        var names = SaveSystem.LoadInventoryNames();
        if (names == null) return;

        InventoryManager.Instance.Items.Clear();
        foreach (string n in names)
        {
            // Pastikan item ada di folder Assets/Resources/Items/
            ItemData item = Resources.Load<ItemData>("Items/" + n);
            if (item != null) InventoryManager.Instance.Items.Add(item);
        }
        InventoryManager.Instance.RefreshUI();
    }

    // Panggil fungsi ini saat Wind menyentuh gerbang Boss
    public async void TravelToBoss(string sceneName)
    {
        // 1. Auto Save sebelum pindah
        SaveSystem.SaveInventory(InventoryManager.Instance.Items);
        Debug.Log("Auto-save completed before transition.");

        // 2. Jalankan efek Fade Out dan tunggu sampai benar-benar hitam
        if (SceneFader.Instance != null)
        {
            await SceneFader.Instance.FadeOut();
        }

        // 3. Pindah Scene setelah layar hitam pekat
        SceneManager.LoadScene(sceneName);
    }
}