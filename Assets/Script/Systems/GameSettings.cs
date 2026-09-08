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
        if (PlayerPrefs.GetInt("LoadFromTransition", 0) == 1)
        {
            PlayerPrefs.SetInt("LoadFromTransition", 0);
            PlayerPrefs.Save();
            LoadAllData();
        }
        else
        {
            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.Slots.Clear();
                InventoryManager.Instance.RefreshUI();
            }

            if (PlayerWeaponManager.Instance != null)
            {
                PlayerWeaponManager.Instance.EquipBaseWeapon();
            }
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            SaveCurrentGame();
        }

        if (Input.GetKeyDown(KeyCode.F2))
        {
            LoadAllData();
        }

        if (Input.GetKeyDown(KeyCode.F3))
        {
            SaveSystem.ResetSave();
            PlayerPrefs.DeleteKey("SavedWeaponUpgraded");
            PlayerPrefs.Save();

            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.Slots.Clear();
                InventoryManager.Instance.RefreshUI();
            }

            PlayerLevel pLevel = FindObjectOfType<PlayerLevel>();
            if (pLevel != null) pLevel.ResetToDefault();

            PlayerStats pStats = FindObjectOfType<PlayerStats>();
            if (pStats != null) pStats.ResetToDefault();

            if (PlayerWeaponManager.Instance != null)
            {
                PlayerWeaponManager.Instance.EquipBaseWeapon();
            }
        }
    }

    public void LoadAllData()
    {
        SaveSystem.GameData data = SaveSystem.LoadData();
        if (data == null) return;

        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.Slots.Clear();
            foreach (string n in data.itemNames)
            {
                ItemData item = Resources.Load<ItemData>("Items/" + n);
                if (item != null)
                {
                    InventoryManager.Instance.Add(item, 1);
                }
            }
            InventoryManager.Instance.RefreshUI();
        }

        PlayerLevel pLevel = FindObjectOfType<PlayerLevel>();
        if (pLevel != null)
        {
            pLevel.LoadSavedData(data.level, data.currentExp, data.maxExp);
        }

        PlayerStats pStats = FindObjectOfType<PlayerStats>();
        if (pStats != null)
        {
            pStats.LoadSavedData(data.maxHealth, data.maxMana, data.attackDamage);
        }

        if (PlayerWeaponManager.Instance != null)
        {
            if (PlayerPrefs.GetInt("SavedWeaponUpgraded", 0) == 1)
            {
                PlayerWeaponManager.Instance.EquipUpgradedWeapon();
            }
            else
            {
                PlayerWeaponManager.Instance.EquipBaseWeapon();
            }
        }
    }

    public async void TravelToBoss(string sceneName)
    {
        SaveCurrentGame();

        PlayerPrefs.SetInt("LoadFromTransition", 1);
        PlayerPrefs.Save();

        if (SceneFader.Instance != null)
        {
            await SceneFader.Instance.FadeOut();
        }

        SceneManager.LoadScene(sceneName);
    }

    private void SaveCurrentGame()
    {
        PlayerLevel pLevel = FindObjectOfType<PlayerLevel>();
        PlayerStats pStats = FindObjectOfType<PlayerStats>();

        if (pLevel != null && pStats != null && InventoryManager.Instance != null)
        {
            SaveSystem.SaveAllData(
                InventoryManager.Instance.Slots,
                pLevel.currentLevel,
                pLevel.currentExp,
                pLevel.expToNextLevel,
                pStats.maxHealth,
                pStats.maxMana,
                pStats.attackDamage
            );
        }

        if (PlayerWeaponManager.Instance != null)
        {
            int isUpgraded = PlayerWeaponManager.Instance.IsWeaponMaxLevel() ? 1 : 0;
            PlayerPrefs.SetInt("SavedWeaponUpgraded", isUpgraded);
            PlayerPrefs.Save();
        }
    }
}