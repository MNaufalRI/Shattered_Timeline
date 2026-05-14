using UnityEngine;
using TMPro; // Wajib untuk TextMeshPro
using Unity.Cinemachine; // Wajib untuk kamera Unity 6
using System.Collections;

public class QuestManager : MonoBehaviour
{
    // Struktur status quest kita
    public enum QuestState
    {
        Tutorial,
        MainQuest1_Kill,
        MainQuest1_GoToArea,
        MainQuest2_KillAll,
        MainQuest2_CheckSound
    }

    [Header("Quest Status")]
    public QuestState currentQuest = QuestState.Tutorial;

    [Header("UI Components")]
    public TextMeshProUGUI questTitleText;
    public TextMeshProUGUI questObjectiveText;

    [Header("Kill Targets")]
    public int tutorialTarget = 1;
    public int mainQuest1Target = 10;
    public int mainQuest2Target = 15; // Anggap ada 15 zombie di area kedua
    private int currentKills = 0;

    [Header("Barrier & Spawnpoint (Area 1)")]
    public Collider solidWallCollider;
    public Collider triggerZoneCollider; // Trigger auto-spawnpoint

    [Header("Boss Cutscene (Area 2)")]
    public CinemachineCamera bossDoorCamera;
    public AudioSource roarAudioSource;

    [Header("UI Management")]
    [Tooltip("Tarik Canvas atau Panel Utama yang berisi semua UI game (HP, Mana, Peta, dll)")]
    public GameObject mainGameUI;

    [Tooltip("Tarik Panel Quest spesifik yang ingin dimunculkan setelah/saat cutscene")]
    public GameObject questPanel;

    private void OnEnable()
    {
        // Mendengarkan event musuh mati dari EnemySimple.cs
        EnemySimple.OnEnemyKilled += HandleEnemyKilled;
    }

    private void OnDisable()
    {
        EnemySimple.OnEnemyKilled -= HandleEnemyKilled;
    }

    private void Start()
    {
        UpdateQuestUI();
    }

    // Fungsi ini otomatis dipanggil setiap ada musuh yang memanggil OnEnemyKilled?.Invoke()
    private void HandleEnemyKilled()
    {
        currentKills++;
        CheckQuestProgress();
        UpdateQuestUI();
    }

    private void CheckQuestProgress()
    {
        switch (currentQuest)
        {
            case QuestState.Tutorial:
                if (currentKills >= tutorialTarget)
                {
                    // Lanjut ke Main Quest 1
                    currentQuest = QuestState.MainQuest1_Kill;
                    currentKills = 0; // Reset hitungan kill untuk quest baru
                }
                break;

            case QuestState.MainQuest1_Kill:
                if (currentKills >= mainQuest1Target)
                {
                    // Buka jalan dan set trigger spawnpoint
                    if (solidWallCollider != null) solidWallCollider.enabled = false;
                    if (triggerZoneCollider != null) triggerZoneCollider.enabled = true;

                    currentQuest = QuestState.MainQuest1_GoToArea;
                    currentKills = 0;
                }
                break;

            case QuestState.MainQuest2_KillAll:
                if (currentKills >= mainQuest2Target)
                {
                    // Mainkan Cutscene Boss
                    StartCoroutine(PlayBossTeaserCutscene());
                }
                break;
        }
    }

    private void UpdateQuestUI()
    {
        switch (currentQuest)
        {
            case QuestState.Tutorial:
                questTitleText.text = "Tutorial";
                questObjectiveText.text = $"- Kill Zombie: {currentKills} / {tutorialTarget}";
                break;

            case QuestState.MainQuest1_Kill:
                questTitleText.text = "Clear The Path";
                questObjectiveText.text = $"- Eliminate Zombies : {currentKills} / {mainQuest1Target}";
                break;

            case QuestState.MainQuest1_GoToArea:
                questTitleText.text = "Clear The Path";
                questObjectiveText.text = "- Go to the next area";
                break;

            case QuestState.MainQuest2_KillAll:
                questTitleText.text = "Secure The Zone";
                questObjectiveText.text = $"- Purge the area : {currentKills} / {mainQuest2Target}";
                break;

            case QuestState.MainQuest2_CheckSound:
                questTitleText.text = "Unknown Threat";
                questObjectiveText.text = "- Investigate the terrifying roar";
                break;
        }
    }

    // Fungsi untuk dipanggil dari Trigger Spawnpoint saat player masuk area baru
    public void AdvanceToMainQuest2()
    {
        if (currentQuest == QuestState.MainQuest1_GoToArea)
        {
            currentQuest = QuestState.MainQuest2_KillAll;
            currentKills = 0;
            UpdateQuestUI();
            Debug.Log("Masuk area baru, mulai Main Quest 2!");
        }
    }

    private IEnumerator PlayBossTeaserCutscene()
    {
        if (mainGameUI != null) mainGameUI.SetActive(false);
        // Pastikan Quest Panel juga mati jika sebelumnya sempat menyala
        if (questPanel != null) questPanel.SetActive(false);
        // 1. Cari referensi PlayerStats di awal cutscene
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        PlayerStats stats = null;

        if (playerObj != null)
        {
            stats = playerObj.GetComponent<PlayerStats>();
        }

        // 2. Aktifkan status kebal (Immune)
        if (stats != null)
        {
            stats.isInvincible = true;
            Debug.Log("<color=green>Player is now Invincible during cutscene.</color>");
        }

        currentQuest = QuestState.MainQuest2_CheckSound;
        UpdateQuestUI();

        // 3. Pindahkan kamera ke pintu bos/area spesifik
        if (bossDoorCamera != null) bossDoorCamera.Priority = 20;

        // 4. Mainkan suara auman bos
        if (roarAudioSource != null) roarAudioSource.Play();

        // Tunggu durasi cutscene (misal 3 detik)
        yield return new WaitForSeconds(3f);

        // 5. Kembalikan kamera ke player
        if (bossDoorCamera != null) bossDoorCamera.Priority = 1;

        yield return new WaitForSeconds(2f);
        if (mainGameUI != null) mainGameUI.SetActive(true);

        // 7. MUNCULKAN QUEST PANEL (Khusus untuk tutorial ini)
        if (questPanel != null) questPanel.SetActive(true);

        // 6. Matikan kembali status kebal setelah cutscene selesai
        if (stats != null)
        {
            stats.isInvincible = false;
            Debug.Log("<color=white>Player is no longer Invincible.</color>");
        }
    }
}