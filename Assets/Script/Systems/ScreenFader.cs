using UnityEngine;
using System.Collections;

public class ScreenFader : MonoBehaviour
{
    public CanvasGroup canvasGroup;
    public GameObject deathText;

    public float fadeSpeed = 2f;

    void Start()
    {
        canvasGroup.alpha = 0;
        canvasGroup.blocksRaycasts = false;

        if (deathText != null)
            deathText.SetActive(false);
    }

    public IEnumerator FadeOutWithDeathText()
    {
        canvasGroup.blocksRaycasts = true;

        float alpha = canvasGroup.alpha;

        while (alpha < 1f)
        {
            alpha += Time.deltaTime * fadeSpeed;
            canvasGroup.alpha = alpha;
            yield return null;
        }

        canvasGroup.alpha = 1f;

        if (deathText != null)
            deathText.SetActive(true);
    }

    public IEnumerator FadeIn()
    {
        if (deathText != null)
            deathText.SetActive(false);

        float alpha = canvasGroup.alpha;

        while (alpha > 0f)
        {
            alpha -= Time.deltaTime * fadeSpeed;
            canvasGroup.alpha = alpha;
            yield return null;
        }

        canvasGroup.alpha = 0;
        canvasGroup.blocksRaycasts = false;
    }
}