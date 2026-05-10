using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

namespace BlastOffProductions.UI  
{

public class CustomLevelLoader : MonoBehaviour
{
    [Header("Scene Loading Settings")]
    [Tooltip("Name of the scene to load (must be in Build Settings).")]
    public string sceneToLoad = "NextScene";

    [Tooltip("Optional delay before actually starting load (seconds).")]
    public float loadDelayTime = 0f;

    [Header("Loading Screen UI")]
    [Tooltip("Loading screen GameObject to enable while loading.")]
    public GameObject loadingScreen;

    [Tooltip("Slider for showing loading progress (0–1).")]
    public Slider progressBar;

    [Tooltip("Text element for showing progress percentage (optional).")]
    public Text progressText;

    [Header("Extra UI to Disable")]
    [Tooltip("Optional UI object (e.g. Main Menu) to disable once the new scene has loaded.")]
    public GameObject uiToDisableOnLoad;

    [Header("Audio Settings")]
    [Tooltip("Sound to play when the button is clicked.")]
    public AudioClip clickSound;

    [Tooltip("AudioSource used to play the sound.")]
    public AudioSource audioSource;

    public void StartLoadingScene()
    {
        Debug.Log("Button pressed: Starting loading process...");

        // Play the click sound if assigned
        if (audioSource != null && clickSound != null)
        {
            audioSource.PlayOneShot(clickSound);
        }

        StartCoroutine(LoadSceneWithUI());
    }

    private IEnumerator LoadSceneWithUI()
    {
        // Show loading screen immediately
        if (loadingScreen != null)
        {
            loadingScreen.SetActive(true);
            Debug.Log("Loading screen enabled.");
        }

        yield return null; // let UI render one frame

        if (loadDelayTime > 0f)
            yield return new WaitForSeconds(loadDelayTime);

        Debug.Log("Beginning async load of scene: " + sceneToLoad);
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneToLoad);
        operation.allowSceneActivation = true;

        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / 0.9f);

            if (progressBar != null)
                progressBar.value = progress;

            if (progressText != null)
                progressText.text = Mathf.RoundToInt(progress * 100f) + "%";

            yield return null;
        }

        // Disable the loading screen once scene is loaded
        if (loadingScreen != null)
        {
            loadingScreen.SetActive(false);
            Debug.Log("Loading screen disabled after scene load.");
        }

        // Disable extra UI object if assigned
        if (uiToDisableOnLoad != null)
        {
            uiToDisableOnLoad.SetActive(false);
            Debug.Log("Extra UI object disabled after scene load.");
        }
    }
}
}