using UnityEngine;
using UnityEngine.UI;

public class StatBarUI : MonoBehaviour
{
    public Image fillImage;

    public float maxValue = 100f;

    private float currentValue;
    private float targetValue;

    [Header("Animation")]
    public float smoothSpeed = 5f;

    void Update()
    {
        // Smooth transition
        currentValue = Mathf.Lerp(currentValue, targetValue, Time.deltaTime * smoothSpeed);

        fillImage.fillAmount = currentValue / maxValue;
    }

    public void SetValue(float value)
    {
        targetValue = Mathf.Clamp(value, 0, maxValue);
    }

    public void SetMaxValue(float value)
    {
        maxValue = value;
        targetValue = value;
        currentValue = value;
    }
}