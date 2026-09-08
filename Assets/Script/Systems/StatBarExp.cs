using UnityEngine;
using UnityEngine.UI;

public class StatBarEXP : MonoBehaviour
{
    public Image fillImage;
    public float maxValue = 100f;

    private float currentValue = 0f;
    private float targetValue = 0f;

    [Header("Animation")]
    public float smoothSpeed = 5f;

    void Update()
    {
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
    }
}