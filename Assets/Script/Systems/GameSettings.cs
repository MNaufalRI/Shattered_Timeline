using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSettings : MonoBehaviour
{
    void Awake()
    {
        Debug.unityLogger.logEnabled = Application.isEditor;
    }

    void Start()
    {
        LoadInventoryData();
        Debug.Log("Inventory otomatis dimuat di scene baru.");
    }

    void Update()
    {
        // Tombol F1 untuk Save Manual
        if (Input.GetKeyDown(KeyCode.F1))
        {
            SaveSystem.SaveInventory(InventoryManager.Instance.Slots);
        }

        // Tombol F2 untuk Load Manual
        if (Input.GetKeyDown(KeyCode.F2))
        {
            LoadInventoryData();
        }

        // --- TAMBAHAN BARU: Tombol F3 untuk Hapus Save Data (Reset) ---
        if (Input.GetKeyDown(KeyCode.F3))
        {
            string path = Application.persistentDataPath + "/inventory_save.json";
            if (System.IO.File.Exists(path))
            {
                System.IO.File.Delete(path);
            }
            InventoryManager.Instance.Slots.Clear();
            InventoryManager.Instance.RefreshUI();
            Debug.Log("<color=red>SAVE DATA DIHAPUS! Inventory kembali kosong.</color>");
        }
    }

    public void LoadInventoryData()
    {
        // Mengambil data nama item yang tersimpan
        var names = SaveSystem.LoadInventoryNames();
        if (names == null) return;

        // UBAH: Bersihkan list .Slots yang baru
        InventoryManager.Instance.Slots.Clear();

        foreach (string n in names)
        {
            ItemData item = Resources.Load<ItemData>("Items/" + n);
            if (item != null)
            {
                // UBAH: Masukkan ke sistem baru menggunakan fungsi Add() 
                // agar otomatis mendeteksi tumpukan/stacking
                InventoryManager.Instance.Add(item, 1);
            }
        }
        InventoryManager.Instance.RefreshUI();
    }

    public async void TravelToBoss(string sceneName)
    {
        // UBAH: Menggunakan .Slots
        SaveSystem.SaveInventory(InventoryManager.Instance.Slots);
        Debug.Log("Auto-save completed before transition.");
        if (SceneFader.Instance != null)
        {
            await SceneFader.Instance.FadeOut();
        }

        SceneManager.LoadScene(sceneName);
    }
}