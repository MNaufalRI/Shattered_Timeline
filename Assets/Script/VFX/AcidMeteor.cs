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
        yield return new WaitForSeconds(timeToHit);

        Explode();

        yield return new WaitForSeconds(timeToDestroyAfterHit);

        Destroy(gameObject);
    }

    void Explode()
    {
        if (isExploding) return;
        isExploding = true;

        Collider[] hits = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                PlayerStats player = hit.GetComponent<PlayerStats>();
                if (player != null && !player.IsDead()) 
                {
                    player.TakeDamage(explosionDamage);
                }
            }
        }

        if (acidPuddlePrefab != null)
        {
            Quaternion rotasiFlat = Quaternion.Euler(0f, 0f, 0f);

            Instantiate(acidPuddlePrefab, transform.position, rotasiFlat);
        }
    }

}