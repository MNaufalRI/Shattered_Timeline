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
    [SerializeField] private TextMeshProUGUI progressText;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Image Target")]
    [Tooltip("Tarik UI Image (jam/ikon) yang ingin diputar ke sini")]
    [SerializeField] private RectTransform loadingImage;

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

        if (loadingImage != null)
        {
            loadingImage.DOKill();
            loadingImage.localRotation = Quaternion.identity;
        }

        loadingScreen.SetActive(true);
        canvasGroup.DOKill();
        canvasGroup.alpha = 0;
        canvasGroup.DOFade(1, 0.5f).SetUpdate(true);

        yield return new WaitForSeconds(0.5f);

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        operation.allowSceneActivation = false;

        float dotTimer = 0f;
        int dotCount = 0;

        bool isRotating = false;

        while (!operation.isDone)
        {
            if (Input.anyKeyDown || Input.GetMouseButtonDown(0))
            {
                if (loadingImage != null && !isRotating)
                {
                    isRotating = true;
                    loadingImage.DOLocalRotate(new Vector3(0, 0, 180), 1f, RotateMode.LocalAxisAdd)
                                .SetEase(Ease.InOutQuad) 
                                .OnComplete(() => isRotating = false);
                }
            }

            if (operation.progress < 0.9f)
            {
                dotTimer += Time.deltaTime;
                if (dotTimer >= 0.4f) 
                {
                    dotTimer = 0f;
                    dotCount++;
                    if (dotCount > 3) dotCount = 0;
                    string dots = new string('.', dotCount);
                    if (progressText != null) progressText.text = "Loading" + dots;
                }
            }
            else
            {
                if (progressText != null) progressText.text = "Press Any Key to Continue";

                if (Input.anyKeyDown || Input.GetMouseButtonDown(0))
                {
                    yield return new WaitForSeconds(0.1f); 
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