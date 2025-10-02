using state;
using UnityEngine;

public class EnemySpawnController : MonoBehaviour
{
    [Header("Spawn Settings")] public GameObject enemyPrefab; // The enemy prefab to spawn
    public Transform biasPoint; // The point enemies will be biased toward
    public float spawnInterval = 2f; // Time between spawns
    public Vector3 spawnAreaSize = new(10, 0, 10); // Size of spawn area
    [Header("Kill Zone Settings")] public Vector3 killZoneSize = new(20, 20, 0); // size of the bounding box


    private float spawnTimer;

    void Start()
    {
        spawnTimer = spawnInterval; // initialize timer
    }

    void Update()
    {
        if (!GameStateManager.Instance.IsPlaying)
        {
            return;
        }

        spawnTimer -= Time.deltaTime;
        if (spawnTimer <= 0f)
        {
            SpawnEnemy();
            spawnTimer = spawnInterval; // reset timer
        }
    }

    void SpawnEnemy()
    {
        // Pick a random position inside the spawn area (centered on spawner)
        Vector3 randomOffset = new Vector3(
            Random.Range(-spawnAreaSize.x / 2f, spawnAreaSize.x / 2f),
            Random.Range(-spawnAreaSize.y / 2f, spawnAreaSize.y / 2f),
            0f // Keep Z = 0 for 2D movement
        );

        Vector3 spawnPos = transform.position + randomOffset;

        GameObject enemyInstance = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);

        // Set the bias point, random type, and kill zone reference
        EnemyController enemyController = enemyInstance.GetComponent<EnemyController>();
        if (enemyController)
        {
            enemyController.targetPoint = biasPoint;

            // Assign a random enemy type
            EnemyController.Type[] types = (EnemyController.Type[])System.Enum.GetValues(typeof(EnemyController.Type));
            enemyController.enemyType = types[Random.Range(0, types.Length)];

            // Assign the spawn controller so the enemy can check the kill zone
            enemyController.spawnController = this;
        }
    }


    public bool IsInsideKillZone(Vector3 position)
    {
        Vector3 min = transform.position - killZoneSize / 2f;
        Vector3 max = transform.position + killZoneSize / 2f;
        return position.x >= min.x && position.x <= max.x &&
               position.y >= min.y && position.y <= max.y;
    }


    // Visualize spawn area in editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, spawnAreaSize);
        // Kill zone in red
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, killZoneSize);
    }
}