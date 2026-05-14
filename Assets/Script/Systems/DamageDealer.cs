using System.Collections.Generic;
using UnityEngine;

public class DamageDealer : MonoBehaviour
{
    bool canDealDamage;
    List<GameObject> hasDealtDamage = new List<GameObject>();

    [Header("Raycast Settings")]
    [SerializeField] float weaponLength = 1.5f;
    [SerializeField] LayerMask enemyLayer;

    [Header("Hit VFX (Saat Kena Musuh)")]
    [SerializeField] private GameObject hitVFXPrefab;
    [SerializeField] private float hitVFXDestroyTime = 1f;

    [Header("Weapon Trail")]
    [SerializeField] private TrailRenderer weaponTrail;

    private float currentDamage;

    void Start()
    {
        canDealDamage = false;
        if (weaponTrail != null) weaponTrail.emitting = false;
    }

    void Update()
    {
        if (canDealDamage)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, -transform.up, out hit, weaponLength, enemyLayer))
            {
                GameObject targetObj = hit.transform.gameObject;

                if (!hasDealtDamage.Contains(targetObj))
                {
                    IDamageable damageable = targetObj.GetComponent<IDamageable>();

                    if (damageable != null)
                    {
                        damageable.TakeDamage(currentDamage);
                        SpawnHitVFX(hit.point, hit.normal);
                        hasDealtDamage.Add(targetObj);
                        Debug.Log($"<color=cyan>Hit: {targetObj.name}</color>");
                    }
                }
            }
        }
    }

    private void SpawnHitVFX(Vector3 point, Vector3 normal)
    {
        if (hitVFXPrefab != null)
        {
            GameObject vfx = Instantiate(hitVFXPrefab, point, Quaternion.LookRotation(normal));
            Destroy(vfx, hitVFXDestroyTime);
        }
    }

    // Fungsi ini dipanggil untuk menyalakan mode serang pedang
    public void StartDealDamage(float finalDamage)
    {
        canDealDamage = true;
        currentDamage = finalDamage;
        hasDealtDamage.Clear();

        if (weaponTrail != null)
        {
            weaponTrail.Clear();
            weaponTrail.emitting = true;
        }
    }

    // Fungsi ini dipanggil untuk mematikan mode serang
    public void EndDealDamage()
    {
        canDealDamage = false;
        if (weaponTrail != null) weaponTrail.emitting = false;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position - transform.up * weaponLength);
    }
}