using UnityEngine;
using UnityEngine.AI;

public class GatherableSpawner : MonoBehaviour
{
    [Header("Item Settings")]
    public GameObject[] itemPrefabs; 

    [Header("Spawn Settings")]
    public int minSpawn = 3;
    public int maxSpawn = 8;
    public float spawnRadius = 10f;

    void Start()
    {
        SpawnItems();
    }

    void SpawnItems()
    {
        int jumlah = Random.Range(minSpawn, maxSpawn + 1);

        for (int i = 0; i < jumlah; i++)
        {
            Vector3 randomPos = transform.position + new Vector3(
                Random.Range(-spawnRadius, spawnRadius),
                0,
                Random.Range(-spawnRadius, spawnRadius)
            );

            NavMeshHit hit;

            if (NavMesh.SamplePosition(randomPos, out hit, 5f, NavMesh.AllAreas))
            {
                // pilih item random
                GameObject randomItem = itemPrefabs[Random.Range(0, itemPrefabs.Length)];

                Instantiate(randomItem, hit.position, Quaternion.identity);
            }
        }

        Debug.Log("Spawn item: " + jumlah);
    }
}