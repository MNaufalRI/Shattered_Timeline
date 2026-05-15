using System.Collections;
using UnityEngine;

public class AcidMeteor : MonoBehaviour
{
    [Header("Meteor Timing")]
    [Tooltip("Waktu dari meteor muncul sampai membentur tanah (Damage masuk)")]
    public float timeToHit = 1.5f;
    [Tooltip("Berapa lama objek dibiarkan hidup setelah meledak (untuk menyelesaikan sisa animasi VFX)")]
    public float timeToDestroyAfterHit = 2.0f;

    [Header("Damage Settings")]
    public float explosionRadius = 3f;
    public float explosionDamage = 15f;

    [Header("References")]
    public GameObject acidPuddlePrefab; // Genangan racun

    private bool isExploding = false;

    void Start()
    {
        // Mulai proses jatuhnya meteor
        StartCoroutine(ExplosionRoutine());
    }

    private IEnumerator ExplosionRoutine()
    {
        // 1. Tunggu sampai animasi meteor menyentuh tanah
        yield return new WaitForSeconds(timeToHit);

        // 2. Berikan damage dan munculkan Puddle
        Explode();

        // 3. Biarkan sisa VFX/animasi ledakan berjalan sampai selesai
        yield return new WaitForSeconds(timeToDestroyAfterHit);

        // 4. Hancurkan seluruh objek meteor ini
        Destroy(gameObject);
    }

    void Explode()
    {
        if (isExploding) return;
        isExploding = true;

        // Berikan Damage Area (Hitbox)
        Collider[] hits = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                PlayerStats player = hit.GetComponent<PlayerStats>();
                if (player != null && !player.IsDead()) // Pastikan memanggil IsDead() atau isDead sesuai settingan PlayerStats-mu
                {
                    player.TakeDamage(explosionDamage);
                }
            }
        }

        // Tinggalkan genangan racun di posisi ledakan
        if (acidPuddlePrefab != null)
        {
            // Memutar 90 derajat pada sumbu X agar rata dengan lantai
            Quaternion rotasiFlat = Quaternion.Euler(0f, 0f, 0f);

            Instantiate(acidPuddlePrefab, transform.position, rotasiFlat);
        }
    }

}