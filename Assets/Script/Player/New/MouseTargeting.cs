using UnityEngine;
using UnityEngine.InputSystem;

public class MouseTargeting : MonoBehaviour
{
    [Header("References")]
    public PlayerControl playerControl;
    private Camera mainCamera;

    [Header("Targeting Settings")]
    public LayerMask enemyLayer;
    public float maxTargetingDistance = 100f;

    void Awake()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        // BRUTE-FORCE: Langsung baca klik kiri dari hardware mouse! Dijamin terbaca.
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Debug.Log("TEST: Hardware Mouse Kiri di-klik!");
            TrySelectTarget();
        }
    }

    void TrySelectTarget()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Ray ray = mainCamera.ScreenPointToRay(mousePos);
        RaycastHit hit;

        // Bantuan Visual: Menggambar garis laser merah di tab 'Scene' selama 2 detik
        Debug.DrawRay(ray.origin, ray.direction * maxTargetingDistance, Color.red, 2f);

        if (Physics.Raycast(ray, out hit, maxTargetingDistance, enemyLayer))
        {
            Debug.Log("Kena Musuh: " + hit.transform.name);

            Transform hitEnemy = hit.transform;
            if (playerControl != null)
            {
                playerControl.ChangeTarget(hitEnemy);
            }
        }
        else
        {
            Debug.Log("Meleset: Klikmu tidak mengenai objek ber-Layer musuh.");
        }
    }
}