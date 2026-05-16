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
    [Tooltip("Tarik Canvas atau Panel Utama yang berisi semua UI game")]
    public GameObject mainGameUI;

    [Tooltip("Tarik Panel Quest spesifik yang ingin dimunculkan")]
    public GameObject questPanel;

    [Header("Tutorial Settings")]
    [Tooltip("Tarik Panel UI utama tutorialmu ke sini")]
    public GameObject tutorialPanel;

    [Tooltip("Masukkan semua halaman/slide tutorial (Panel/Image) ke dalam array ini berurutan")]
    public GameObject[] tutorialSlides;

    private int currentSlideIndex = 0;
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
        if (questPanel != null) questPanel.SetActive(false);
        if (tutorialPanel != null) tutorialPanel.SetActive(false);

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

        // 7. MUNCULKAN QUEST PANEL
        if (questPanel != null) questPanel.SetActive(true);

        // 8. HIDUPKAN KONTROL PLAYER
        player.enabled = true;
        if (movement != null)
        {
            movement.enabled = true;
            movement.canMove = true;
        }


        if (tutorialPanel != null && tutorialSlides.Length > 0)
        {
            tutorialPanel.SetActive(true);
            currentSlideIndex = 0; 
            UpdateSlideVisibility();
        }
        else
        {
            Destroy(gameObject);
        }
    }


    public void NextSlide()
    {
        if (currentSlideIndex < tutorialSlides.Length - 1)
        {
            currentSlideIndex++;
            UpdateSlideVisibility();
        }
    }

    public void PreviousSlide()
    {
        if (currentSlideIndex > 0)
        {
            currentSlideIndex--;
            UpdateSlideVisibility();
        }
    }

    public void CloseTutorial()
    {
        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(false);
        }

        Destroy(gameObject);
    }

    private void UpdateSlideVisibility()
    {
        for (int i = 0; i < tutorialSlides.Length; i++)
        {
            if (tutorialSlides[i] != null)
            {
                tutorialSlides[i].SetActive(i == currentSlideIndex);
            }
        }
    }
}