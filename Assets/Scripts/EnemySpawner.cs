using UnityEngine;
using System.Collections;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private Transform spawnPoint;
    
    [Header("Enemies")]
    [SerializeField] private GameObject[] enemyPrefabs;
    [SerializeField] private bool randomEnemy = true;

    [Header("Spawn Settings")]
    [SerializeField] private float firstSpawnDelay = 1f; //
    [SerializeField] private float spawnInterval = 2f;
    [SerializeField] private float spawnPositionRadius = 4f;

    private int nextEnemyIndex;
    private int remainingSpawnCount;
    private PlaneController player;
    
    private void Start()
    {
        player = FindFirstObjectByType<PlaneController>();
        
        GameEvent.HitPlayer += IncreaseSpawnCount;
    }

    private void IncreaseSpawnCount()
    {
        remainingSpawnCount++;
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        
        GameEvent.HitPlayer -= IncreaseSpawnCount;
    }

    private void OnValidate() // Inspector 값 수정 시점에 자동으로 호출
    {
        firstSpawnDelay = Mathf.Max(0f, firstSpawnDelay);
        spawnInterval = Mathf.Max(0.1f, spawnInterval);
        spawnPositionRadius = Mathf.Max(0f, spawnPositionRadius);
    }

    public void StartSpawning(int spawnCount)
    {
        StopSpawning();
        
        remainingSpawnCount = spawnCount;

        if (remainingSpawnCount <= 0) return;
        
        StartCoroutine(SpawnLoop());
    }

    public void StopSpawning()
    {
        StopAllCoroutines();
    }
    
    private IEnumerator SpawnLoop()
    {
        yield return new WaitForSeconds(firstSpawnDelay);

        while (enabled && remainingSpawnCount > 0) // 활성화되어 있는 동안
        {
            EnemyController enemy = SpawnEnemy();
            if (enemy != null)
            {
                remainingSpawnCount--;
                yield return new WaitForSeconds(spawnInterval);
            }
            else yield return null;
        }
    }

    private EnemyController SpawnEnemy()
    {
        if (enemyPrefabs == null || enemyPrefabs.Length == 0) return null;

        GameObject enemyPrefab = GetEnemyPrefab();
        GameObject enemyObject = Instantiate(enemyPrefab, GetSpawnPosition(), spawnPoint.rotation);
        EnemyController enemy = enemyObject.GetComponent<EnemyController>();
        
        enemy.Initialize(player.transform);
        
        return enemy;
    }

    private GameObject GetEnemyPrefab()
    {
        if (randomEnemy)
        {
            GameObject randomPrefab = null;
            while (randomPrefab == null)
            {
                randomPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
            }

            return randomPrefab;
        }

        GameObject enemyPrefab = null;
        while (enemyPrefab == null)
        {
            enemyPrefab = enemyPrefabs[nextEnemyIndex];
            nextEnemyIndex = (nextEnemyIndex + 1) % enemyPrefabs.Length;
        }

        return enemyPrefab;
    }

    private Vector3 GetSpawnPosition()
    {
        Vector2 offset = Random.insideUnitCircle * spawnPositionRadius;
        return spawnPoint.position + spawnPoint.right * offset.x + spawnPoint.up * offset.y;
    }
}
