using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSettings : MonoBehaviour
{
    void Awake()
    {
        Debug.unityLogger.logEnabled = Application.isEditor;
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            SaveSystem.SaveInventory(InventoryManager.Instance.Items);
        }

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
            ItemData item = Resources.Load<ItemData>("Items/" + n);
            if (item != null) InventoryManager.Instance.Items.Add(item);
        }
        InventoryManager.Instance.RefreshUI();
    }

    public async void TravelToBoss(string sceneName)
    {
        SaveSystem.SaveInventory(InventoryManager.Instance.Items);
        Debug.Log("Auto-save completed before transition.");
        if (SceneFader.Instance != null)
        {
            await SceneFader.Instance.FadeOut();
        }

        SceneManager.LoadScene(sceneName);
    }
}