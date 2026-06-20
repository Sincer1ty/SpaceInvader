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
    [SerializeField] private int maxAliveEnemies = 10;
    [SerializeField] private float spawnPositionRadius = 4f;

    private int nextEnemyIndex;
    private int aliveEnemyCount;
    private Coroutine spawnRoutine;
    
    private PlaneController player;
    
    private void Start()
    {
        spawnRoutine = StartCoroutine(SpawnLoop());
        player = FindFirstObjectByType<PlaneController>();
    }

    private void OnDisable()
    {
        if (spawnRoutine != null)
        {
            StopCoroutine(spawnRoutine);
            spawnRoutine = null;
        }
    }

    private void OnValidate() // Inspector 값 수정 시점에 자동으로 호출
    {
        firstSpawnDelay = Mathf.Max(0f, firstSpawnDelay);
        spawnInterval = Mathf.Max(0.1f, spawnInterval);
        maxAliveEnemies = Mathf.Max(1, maxAliveEnemies);
        spawnPositionRadius = Mathf.Max(0f, spawnPositionRadius);
    }

    private IEnumerator SpawnLoop()
    {
        yield return new WaitForSeconds(firstSpawnDelay);

        while (enabled) // 활성화되어 있는 동안
        {
            SpawnEnemy();
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    [ContextMenu("Spawn Enemy")]
    public void SpawnEnemy()
    {
        if (!CanSpawn()) return;

        GameObject enemyPrefab = GetEnemyPrefab();
        EnemyController enemy = Instantiate(enemyPrefab, GetSpawnPosition(), spawnPoint.rotation).GetComponent<EnemyController>();
        enemy.Initialize(player.transform);
        
        aliveEnemyCount++;
    }

    private bool CanSpawn()
    {
        if (spawnPoint == null)
        {
            Debug.LogWarning($"{nameof(EnemySpawner)} needs a spawn point.", this);
            return false;
        }
        
        if (enemyPrefabs == null || enemyPrefabs.Length == 0)
        {
            Debug.LogWarning($"{nameof(EnemySpawner)} needs at least one enemy prefab.", this);
            return false;
        }
        
        if (aliveEnemyCount >= maxAliveEnemies) // 최대 값 이상 스폰 막기
        {
            return false;
        }

        return true;
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
