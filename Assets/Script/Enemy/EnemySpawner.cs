using UnityEngine;
using UnityEngine.AI;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public int minSpawn = 1;
    public int maxSpawn = 5;
    public float spawnRadius = 5f;

    void Start()
    {
        SpawnEnemies();
    }

    void SpawnEnemies()
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
                Instantiate(enemyPrefab, hit.position, Quaternion.identity);
            }
        }
    }
}