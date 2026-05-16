using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using StarterAssets;

public class PauseMenuManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject pauseMenuPanel; 
    [Tooltip("Panel yang isinya tombol Continue, Settings, Quit")]
    public GameObject mainButtonsPanel;
    [Tooltip("Panel yang isinya slider Master, Music, SFX")]
    public GameObject settingsPanel;

    [Header("Audio")]
    public AudioMixer audioMixer;

    [Header("Player Input Component")]
    public StarterAssetsInputs starterAssetsInputs;

    private bool isPaused = false;

    void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    public void PauseGame()
    {
        pauseMenuPanel.SetActive(true);
        mainButtonsPanel.SetActive(true);
        settingsPanel.SetActive(false);

        Time.timeScale = 0f;
        isPaused = true;

        if (starterAssetsInputs != null)
        {
            starterAssetsInputs.cursorInputForLook = false;
        }
    }

    public void ResumeGame()
    {
        pauseMenuPanel.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
        if (starterAssetsInputs != null)
        {
            starterAssetsInputs.cursorInputForLook = true;
        }
    }
    public void OpenSettings()
    {
        mainButtonsPanel.SetActive(false);
        settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        settingsPanel.SetActive(false);
        mainButtonsPanel.SetActive(true);
    }

    public void SetMasterVolume(float sliderValue)
    {
        audioMixer.SetFloat("MasterVolume", Mathf.Log10(sliderValue) * 20f);
    }

    public void SetMusicVolume(float sliderValue)
    {
        audioMixer.SetFloat("MusicVolume", Mathf.Log10(sliderValue) * 20f);
    }

    public void SetSFXVolume(float sliderValue)
    {
        audioMixer.SetFloat("SFXVolume", Mathf.Log10(sliderValue) * 20f);
    }

    public void QuitGame()
    {
        Debug.Log("Keluar dari Game!");
        Application.Quit();
    }
}