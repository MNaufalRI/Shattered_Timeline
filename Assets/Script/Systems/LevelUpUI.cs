using UnityEngine;
using DG.Tweening; // Sudah ada di proyekmu
using TMPro;

public class LevelUpUI : MonoBehaviour
{
    public CanvasGroup panel;
    public TextMeshProUGUI levelText;
    public float displayDuration = 2f;

    public void ShowLevelUp(int newLevel)
    {
        levelText.text = $"LEVEL UP!\nLevel {newLevel}";
        panel.alpha = 0f;
        gameObject.SetActive(true);

        panel.DOFade(1f, 0.3f)
             .OnComplete(() =>
             {
                 DOVirtual.DelayedCall(displayDuration, () =>
                 {
                     panel.DOFade(0f, 0.5f)
                          .OnComplete(() => gameObject.SetActive(false));
                 });
             });
    }
}