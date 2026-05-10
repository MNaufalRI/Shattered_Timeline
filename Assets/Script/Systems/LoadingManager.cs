using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using DG.Tweening;

public class LoadingManager : MonoBehaviour
{
    public static LoadingManager Instance;

    [Header("UI References")]
    [SerializeField] private GameObject loadingScreen;
    // Slider dihapus untuk mencegah error NULL target
    [SerializeField] private TextMeshProUGUI progressText;
    [SerializeField] private CanvasGroup canvasGroup;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void LoadScene(string sceneName)
    {
        // Bersihkan semua tween aktif agar tidak memory leak
        DOTween.KillAll();
        StartCoroutine(LoadAsynchronously(sceneName));
    }

    IEnumerator LoadAsynchronously(string sceneName)
    {
        if (loadingScreen == null || canvasGroup == null)
        {
            SceneManager.LoadScene(sceneName);
            yield break;
        }

        loadingScreen.SetActive(true);
        canvasGroup.DOKill();
        canvasGroup.alpha = 0;
        canvasGroup.DOFade(1, 0.5f).SetUpdate(true);

        yield return new WaitForSeconds(0.5f);

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        operation.allowSceneActivation = false;

        while (!operation.isDone)
        {
            // Tetap hitung progress untuk update teks persentase
            float progress = Mathf.Clamp01(operation.progress / 0.9f);

            if (progressText != null)
            {
                progressText.text = (progress * 100f).ToString("F0") + "%";
            }

            if (operation.progress >= 0.9f)
            {
                if (progressText != null) progressText.text = "Press Any Key to Continue";

                if (Input.anyKeyDown)
                {
                    yield return new WaitForSeconds(0.2f);
                    operation.allowSceneActivation = true;
                }
            }

            yield return null;
        }

        if (canvasGroup != null)
        {
            canvasGroup.DOKill();
            canvasGroup.DOFade(0, 0.5f).SetUpdate(true).OnComplete(() => {
                loadingScreen.SetActive(false);
            });
        }
    }
}