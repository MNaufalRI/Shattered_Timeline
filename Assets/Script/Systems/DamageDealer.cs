using System.Collections.Generic;
using UnityEngine;

public class DamageDealer : MonoBehaviour
{
    bool canDealDamage;
    List<GameObject> hasDealtDamage = new List<GameObject>();

    [Header("Settings")]
    [SerializeField] float weaponLength = 1.5f;
    [SerializeField] LayerMask enemyLayer;

    private float currentDamage;

    void Start() => canDealDamage = false;

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
                    // HANYA SATU BARIS UNTUK SEMUA JENIS MUSUH
                    IDamageable damageable = targetObj.GetComponent<IDamageable>();

                    if (damageable != null)
                    {
                        damageable.TakeDamage(currentDamage);

                        hasDealtDamage.Add(targetObj);
                        Debug.Log($"<color=cyan>Interface Hit: {targetObj.name} | Damage: {currentDamage}</color>");
                    }
                }
            }
        }
    }

    public void StartDealDamage(float finalDamage)
    {
        canDealDamage = true;
        currentDamage = finalDamage;
        hasDealtDamage.Clear();
    }

    public void EndDealDamage() => canDealDamage = false;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position - transform.up * weaponLength);
    }
}