using UnityEngine;
using UnityEngine.UI;
using DG.Tweening; // Menggunakan library yang sudah ada di proyekmu
using System.Threading.Tasks;

public class SceneFader : MonoBehaviour
{
    public static SceneFader Instance;
    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private CanvasGroup canvasGroup;
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Agar tirai tetap ada saat pindah scene
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Efek Fade In saat game pertama kali dimulai atau ganti stage
        FadeIn();
    }

    // Tambahkan variabel di atas

    public void FadeIn()
    {
        // Saat layar mulai terang, matikan tembok klik
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
        fadeImage.DOFade(0, fadeDuration).SetEase(Ease.InOutQuad);
    }

    public async Task FadeOut()
    {
        // Saat layar mulai menghitam, nyalakan tembok klik agar player tidak bisa nge-klik tombol lain
        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;
        await fadeImage.DOFade(1, fadeDuration).SetEase(Ease.InOutQuad).AsyncWaitForCompletion();
    }
}