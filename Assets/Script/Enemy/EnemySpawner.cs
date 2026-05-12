using UnityEngine;
using UnityEngine.AI;

public class EnemySpawner : MonoBehaviour
{
    [Header("Pool Musuh")]
    // Menggunakan Array agar bisa menampung banyak prefab sekaligus
    public GameObject[] enemyPrefabs;

    [Header("Spawn Settings")]
    public int minSpawn = 1;
    public int maxSpawn = 5;
    public float spawnRadius = 5f;

    void Start()
    {
        SpawnEnemies();
    }

    void SpawnEnemies()
    {
        // 1. Tentukan jumlah musuh yang akan muncul kali ini
        int jumlah = Random.Range(minSpawn, maxSpawn + 1);

        for (int i = 0; i < jumlah; i++)
        {
            // 2. Ambil posisi random di dalam radius
            Vector3 randomPos = transform.position + new Vector3(
                Random.Range(-spawnRadius, spawnRadius),
                0,
                Random.Range(-spawnRadius, spawnRadius)
            );

            NavMeshHit hit;

            // 3. Pastikan posisi random tersebut berada di atas NavMesh
            if (NavMesh.SamplePosition(randomPos, out hit, 5f, NavMesh.AllAreas))
            {
                // 4. Pilih satu musuh secara acak dari Array enemyPrefabs
                // Random.Range untuk int akan mengambil angka dari 0 sampai (Length - 1)
                int randomIndex = Random.Range(0, enemyPrefabs.Length);
                GameObject selectedPrefab = enemyPrefabs[randomIndex];

                // 5. Munculkan musuh yang terpilih
                if (selectedPrefab != null)
                {
                    Instantiate(selectedPrefab, hit.position, Quaternion.identity);
                }
            }
        }
    }

    // Visualisasi radius spawner di Editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);
    }
}