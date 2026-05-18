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
        bool isActive = craftingUIPanel.activeSelf;
        craftingUIPanel.SetActive(!isActive);

        // Opsional: Pause waktu atau lock kursor saat UI terbuka
        Time.timeScale = isActive ? 1f : 0f;
        Cursor.lockState = isActive ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !isActive;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = true;
            Debug.Log("Tekan 'F' untuk membuka Anvil.");
            // Kamu bisa memunculkan UI tulisan "Press F to Craft" di sini
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = false;
            craftingUIPanel.SetActive(false); // Tutup otomatis jika player menjauh
            Time.timeScale = 1f;
        }
    }
}