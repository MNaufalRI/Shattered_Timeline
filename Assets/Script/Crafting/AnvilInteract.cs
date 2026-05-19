using UnityEngine;

public class AnvilInteract : MonoBehaviour
{
    [Header("UI Referensi")]
    public GameObject craftingUIPanel; // Tarik UI Crafting Menu ke sini

    private bool isPlayerNear = false;

    void Update()
    {
        // Jika player di dekat Anvil dan menekan tombol F
        if (isPlayerNear && Input.GetKeyDown(KeyCode.F))
        {
            ToggleCraftingUI();
        }
    }

    private void ToggleCraftingUI()
    {
        // 1. Balikkan kondisi aktif panel UI (buka/tutup)
        bool akanTerbuka = !craftingUIPanel.activeSelf;
        craftingUIPanel.SetActive(akanTerbuka);

        // 2. JIKA PANELNYA TERBUKA, PAKSA REFRESH ANGKA!
        if (akanTerbuka)
        {
            // Cari komponen CraftingManager di panel tersebut, lalu panggil fungsinya
            CraftingManager craftingScript = craftingUIPanel.GetComponent<CraftingManager>();

            if (craftingScript != null)
            {
                craftingScript.UpdateCraftingUI();
                Debug.Log("<color=lime>[ANVIL]</color> Sukses memicu hitung ulang material saat menu dibuka!");
            }
            else
            {
                Debug.LogError("[ANVIL] Waduh, script CraftingManager tidak ditemukan di object " + craftingUIPanel.name);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = true;
            Debug.Log("Tekan 'F' untuk membuka Anvil.");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = false;

            // Tutup otomatis jika player menjauh
            craftingUIPanel.SetActive(false);
        }
    }
}