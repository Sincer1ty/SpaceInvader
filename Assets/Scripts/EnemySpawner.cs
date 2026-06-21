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
        
        StartCoroutine(SpawnLoop(interval));
    }

    public void StopSpawning()
    {
        StopAllCoroutines();
    }
    
    private IEnumerator SpawnLoop(float interval)
    {
        yield return new WaitForSeconds(firstSpawnDelay);

        while (enabled) // 활성화되어 있는 동안
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
        GameObject enemyObject = Instantiate(enemyPrefab, GetSpawnPosition(), spawnPoint.rotation);
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

    private Vector3 GetSpawnPosition()
    {
        Vector2 offset = Random.insideUnitCircle * spawnPositionRadius; // 스폰 지점 주변의 랜덤 위치 계산
        return spawnPoint.position + spawnPoint.right * offset.x + spawnPoint.up * offset.y;
    }
}
