using UnityEngine;
using System.Collections;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private Transform spawnPoint;
    
    [Header("Enemies")]
    [SerializeField] private GameObject[] enemyPrefabs;
    [SerializeField] private bool randomEnemy = true;

    [Header("Spawn Settings")]
    [SerializeField] private float firstSpawnDelay = 1f;
    [SerializeField] private float spawnPositionRadius = 4f;

    private int nextEnemyIndex;
    private PlaneController player;
    private bool isSpawning;
    
    private void Start()
    {
        player = FindFirstObjectByType<PlaneController>();
    }

    private void OnDisable()
    {
        StopAllCoroutines(); // 실행 중인 코루틴 정지
    }

    private void OnValidate() // Inspector 값 수정 시점에 자동으로 호출
    {
        firstSpawnDelay = Mathf.Max(0f, firstSpawnDelay);
        spawnPositionRadius = Mathf.Max(0f, spawnPositionRadius);
    }

    public void StartSpawning(float interval)
    {
        StopSpawning(); // 중복 코루틴 실행 방지
        
        isSpawning = true;
        StartCoroutine(SpawnLoop(interval));
    }

    public void StopSpawning()
    {
        isSpawning = false;
        StopAllCoroutines();
    }

    public void ClearEnemies() // 남아있는 모든 EnemyController 제거
    {
        StopSpawning();

        EnemyController[] enemies = FindObjectsByType<EnemyController>(FindObjectsSortMode.None);
        foreach (EnemyController enemy in enemies)
        {
            if (enemy != null)
            {
                enemy.Despawn();
            }
        }
    }
    
    private IEnumerator SpawnLoop(float interval)
    {
        yield return new WaitForSeconds(firstSpawnDelay);

        while (enabled && isSpawning) // 활성화되어 있고 스폰 허용 상태인 동안
        {
            EnemyController enemy = SpawnEnemy();
            if (enemy != null)
            {
                yield return new WaitForSeconds(interval);
            }
            else yield return null;
        }
    }

    private EnemyController SpawnEnemy()
    {
        if (enemyPrefabs == null || enemyPrefabs.Length == 0) return null;

        GameObject enemyPrefab = GetEnemyPrefab();
        GameObject enemyObject = Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);
        EnemyController enemy = enemyObject.GetComponent<EnemyController>();
        
        enemy.Initialize(player.transform); // 플레이어를 추적 대상으로 전달
        
        return enemy;
    }

    private GameObject GetEnemyPrefab()
    {
        if (randomEnemy)
            return enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];

        GameObject enemyPrefab = null;
        while (enemyPrefab == null)
        {
            enemyPrefab = enemyPrefabs[nextEnemyIndex];
            nextEnemyIndex = (nextEnemyIndex + 1) % enemyPrefabs.Length;
        }

        return enemyPrefab;
    }
}
