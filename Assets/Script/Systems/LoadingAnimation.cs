using UnityEngine;
using UnityEngine.UI;

public class LoadingIcon : MonoBehaviour
{
    private RectTransform rectComponent;

    [Header("Rotation Settings")]
    public float rotateSpeed = -200f; // Nilai negatif agar searah jarum jam
    public bool isRotating = true;

    void Start()
    {
        rectComponent = GetComponent<RectTransform>();
    }

    void Update()
    {
        if (!isRotating) return;

        // Gunakan unscaledDeltaTime agar rotasi tetap mulus saat loading berat
        float currentSpeed = rotateSpeed * Time.unscaledDeltaTime;

        // Memutar pada sumbu Z (UI biasanya berputar di sumbu ini)
        rectComponent.Rotate(0f, 0f, currentSpeed);
    }

    // Fungsi tambahan agar bisa dimatikan/dinyalakan dari LoadingManager
    public void SetRotating(bool status)
    {
        isRotating = status;
    }
}