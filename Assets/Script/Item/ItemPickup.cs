using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    public ItemData itemData;
    public int amount = 1;

    [Header("Pickup Settings")]
    public float pickupRadius = 2f;
    public float pickupDelay = 1f;

    private float spawnTime;
    private Transform player;

    void Start()
    {
        spawnTime = Time.time;

        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null)
            player = p.transform;
    }

    void Update()
    {
        if (player == null) return;

        // delay setelah spawn
        if (Time.time < spawnTime + pickupDelay)
            return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= pickupRadius)
        {
            Pickup();
        }
    }

    void Pickup()
    {
        if (itemData == null)
        {
            Debug.LogError("ItemData kosong!");
            return;
        }

        if (InventoryManager.Instance == null)
        {
            Debug.LogError("InventoryManager tidak ada!");
            return;
        }

        InventoryManager.Instance.Add(itemData);

        Debug.Log("Auto pickup: " + itemData.itemName);

        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickupRadius);
    }
}