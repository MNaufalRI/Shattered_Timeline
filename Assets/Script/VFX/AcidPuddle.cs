using UnityEngine;
using StarterAssets;

public class AcidPuddle : MonoBehaviour
{
    [Header("Puddle Settings")]
    public float duration = 5f;
    public float dotDamage = 2f;
    public float tickRate = 0.5f;

    private float timer = 0f;
    private PlayerMovement2 affectedPlayer; // Simpan referensi player yang ada di dalam

    void Start()
    {
        // Hancurkan genangan setelah durasi habis
        Destroy(gameObject, duration);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            affectedPlayer = other.GetComponent<PlayerMovement2>();
            ApplyDebuff();
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            timer += Time.deltaTime;
            if (timer >= tickRate)
            {
                PlayerStats stats = other.GetComponent<PlayerStats>();
                if (stats != null && !stats.isDead)
                {
                    stats.TakeDamage(dotDamage);
                }
                timer = 0f;
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            RemoveDebuff();
            affectedPlayer = null;
        }
    }

    // PENTING: Jika objek hancur tapi player masih di dalam, kembalikan speed-nya!
    void OnDestroy()
    {
        if (affectedPlayer != null)
        {
            RemoveDebuff();
        }
    }

    void ApplyDebuff()
    {
        if (affectedPlayer != null)
        {
            affectedPlayer.MoveSpeed *= 0.5f;
            affectedPlayer.SprintSpeed *= 0.5f;
        }
    }

    void RemoveDebuff()
    {
        if (affectedPlayer != null)
        {
            affectedPlayer.MoveSpeed /= 0.5f;
            affectedPlayer.SprintSpeed /= 0.5f;
            affectedPlayer = null;
        }
    }
}