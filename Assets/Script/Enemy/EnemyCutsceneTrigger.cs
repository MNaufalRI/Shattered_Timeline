using UnityEngine;
using Unity.Cinemachine;
using System.Collections;
using StarterAssets;

public class EnemyCutsceneTrigger : MonoBehaviour
{
    [Header("Cinemachine Settings")]
    public CinemachineCamera enemyVcam;
    public float cutsceneDuration = 3f;

    [Header("UI Management")]
    [Tooltip("Tarik Canvas atau Panel Utama yang berisi semua UI game (HP, Mana, Peta, dll)")]
    public GameObject mainGameUI;

    [Tooltip("Tarik Panel Quest spesifik yang ingin dimunculkan setelah/saat cutscene")]
    public GameObject questPanel;

    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !hasTriggered)
        {
            PlayerControl playerScript = other.GetComponent<PlayerControl>();
            if (playerScript != null)
            {
                StartCoroutine(PlayCutscene(playerScript));
            }
        }
    }

    IEnumerator PlayCutscene(PlayerControl player)
    {
        hasTriggered = true;

        // 1. SEMBUNYIKAN SEMUA UI
        if (mainGameUI != null) mainGameUI.SetActive(false);
        // Pastikan Quest Panel juga mati jika sebelumnya sempat menyala
        if (questPanel != null) questPanel.SetActive(false);

        // 2. MATIKAN KONTROL PLAYER
        var movement = player.GetComponent<PlayerMovement2>();
        player.enabled = false;
        if (movement != null)
        {
            movement.canMove = false;
            movement.enabled = false;
        }

        // 3. ANIMASI IDLE
        Animator anim = player.GetComponentInChildren<Animator>();
        if (anim != null)
        {
            anim.SetFloat("Speed", 0f);
            anim.Play("Idle", 0, 0f);
        }

        // 4. AKTIVASI KAMERA CUTSCENE
        if (enemyVcam != null) enemyVcam.Priority = 20;

        // --- DURASI CUTSCENE BERLANGSUNG ---
        yield return new WaitForSeconds(cutsceneDuration);

        // 5. KEMBALIKAN KAMERA KE PLAYER
        if (enemyVcam != null) enemyVcam.Priority = 1;

        // 6. HIDUPKAN KEMBALI UI
        yield return new WaitForSeconds(2f);
        if (mainGameUI != null) mainGameUI.SetActive(true);

        // 7. MUNCULKAN QUEST PANEL (Khusus untuk tutorial ini)
        if (questPanel != null) questPanel.SetActive(true);

        // 8. HIDUPKAN KONTROL PLAYER
        player.enabled = true;
        if (movement != null)
        {
            movement.enabled = true;
            movement.canMove = true;
        }

        Destroy(gameObject);
    }
}