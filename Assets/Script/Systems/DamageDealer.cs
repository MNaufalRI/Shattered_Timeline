using System.Collections.Generic;
using UnityEngine;

public class DamageDealer : MonoBehaviour
{
    bool canDealDamage;
    List<GameObject> hasDealtDamage;

    [SerializeField] float weaponLength = 1.5f;
    [SerializeField] LayerMask enemyLayer; // Lebih baik pakai ini daripada hardcode "1 << 9"

    private float currentDamage;

    void Start()
    {
        canDealDamage = false;
        hasDealtDamage = new List<GameObject>();
    }

    void Update()
    {
        if (canDealDamage)
        {
            RaycastHit hit;
            // Memancarkan raycast mengikuti panjang pedang
            if (Physics.Raycast(transform.position, -transform.up, out hit, weaponLength, enemyLayer))
            {
                if (!hasDealtDamage.Contains(hit.transform.gameObject))
                {
                    // Berikan damage ke musuh (Sesuaikan dengan script kamu)
                    EnemySimple enemySimple = hit.transform.GetComponent<EnemySimple>();
                    if (enemySimple != null) enemySimple.TakeDamage(currentDamage);

                    EnemyBase enemyBase = hit.transform.GetComponent<EnemyBase>();
                    if (enemyBase != null) enemyBase.OnHit();

                    if (enemySimple != null || enemyBase != null)
                    {
                        hasDealtDamage.Add(hit.transform.gameObject);
                        Debug.Log($"Hit musuh dengan damage: {currentDamage}");
                    }
                }
            }
        }
    }

    // Fungsi ini sekarang meminta parameter finalDamage dari Player
    public void StartDealDamage(float finalDamage)
    {
        canDealDamage = true;
        currentDamage = finalDamage;
        hasDealtDamage.Clear(); // Kosongkan daftar agar bisa nge-hit lagi di ayunan berikutnya
    }

    public void EndDealDamage()
    {
        canDealDamage = false;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position - transform.up * weaponLength);
    }
}