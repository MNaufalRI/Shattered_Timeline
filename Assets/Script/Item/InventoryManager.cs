using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; // TAMBAHAN: Untuk mendeteksi pindah scene

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;
    public List<ItemData> Items = new List<ItemData>();

    [Header("UI References")]
    public Transform ItemContent;
    public GameObject InventoryItem;

    void Awake()
    {
        // Singleton & Persistence
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Daftarkan fungsi OnSceneLoaded ke event manager Unity
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnDestroy()
    {
        // Bagus untuk mencegah memory leak saat game dimatikan
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // Fungsi ini OTOMATIS berjalan setiap kali game berhasil memuat scene baru
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        FindUIReferencesInNewScene();
        RefreshUI();
    }

    private void FindUIReferencesInNewScene()
    {
        // Cari ItemContent baru hanya SEKALI saja saat pindah scene
        GameObject contentObj = GameObject.Find("ItemContent");
        if (contentObj != null)
        {
            ItemContent = contentObj.transform;
        }
        else
        {
            // Set ke null jika scene baru tidak punya UI inventory (misal: di Main Menu atau Loading Stage)
            ItemContent = null;
        }
    }

    public void Add(ItemData item) { Items.Add(item); RefreshUI(); }
    public void Remove(ItemData item) { Items.Remove(item); RefreshUI(); }

    public void RefreshUI()
    {
        // Jika di scene saat ini tidak ada UI Inventory, langsung abaikan agar tidak error
        if (ItemContent == null || InventoryItem == null) return;

        // Bersihkan UI lama dengan aman (Lepas parent dulu agar tidak duplikat di frame yang sama)
        for (int i = ItemContent.childCount - 1; i >= 0; i--)
        {
            Transform child = ItemContent.GetChild(i);
            child.SetParent(null); // Putus hubungan instan
            Destroy(child.gameObject); // Hancurkan di akhir frame
        }

        // Spawn ulang slot UI berdasarkan item terbaru
        foreach (var item in Items)
        {
            GameObject obj = Instantiate(InventoryItem, ItemContent);
            obj.GetComponent<InventoryItemUI>()?.Setup(item);
        }
    }
}