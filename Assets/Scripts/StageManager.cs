using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class StageManager : MonoBehaviour
{
    [SerializeField] private EnemySpawner enemySpawner;
    [SerializeField] private StageStartUI stageStartUI;
    [FormerlySerializedAs("stageEnemyCounts")]
    [SerializeField] private int[] stageTargetKillCounts = { 20, 30, 50 };
    [SerializeField] private float nextStageDelay = 2f;
    [SerializeField] private float spawnInterval = 2f;
    [SerializeField] private float fastRate = 0.3f; // 스테이지별 spawn 시간이 빨라지는 정도

    private int currentStageIndex;
    private int targetKillCount;
    private int currentKillCount;
    private bool stageCleared;
    public static bool gameCleared;

    public int CurrentStageNumber => gameCleared ? stageTargetKillCounts.Length : currentStageIndex + 1;
    
    private void Start()
    {
        StartStage(0);
        
        GameEvent.EnemyKilled += OnEnemyKilled;
        GameEvent.OnHpChanged += stageStartUI.UpdateHearts;
        GameEvent.PlayerDead += OnPlayerDead;
    }
    
    private void OnDisable()
    {
        StopAllCoroutines();
        
        GameEvent.EnemyKilled -= OnEnemyKilled;
        GameEvent.OnHpChanged -= stageStartUI.UpdateHearts;
        GameEvent.PlayerDead -= OnPlayerDead;
    }

    private void OnPlayerDead()
    {
        gameCleared = false;
        SceneManager.LoadScene("EndScene");
    }

    private void OnValidate() // Inspector 값 수정 시점에 자동으로 호출
    {
        for (int i = 0; i < stageTargetKillCounts.Length; i++)
        {
            stageTargetKillCounts[i] = Mathf.Max(1, stageTargetKillCounts[i]);
        }
    }

    private void StartStage(int stageIndex)
    {
        if (stageIndex >= stageTargetKillCounts.Length)
        {
            ClearGame();
            return;
        }
        
        // 현재 스테이지 상태로 초기화
        currentStageIndex = stageIndex;
        targetKillCount = stageTargetKillCounts[currentStageIndex];
        currentKillCount = 0;
        stageCleared = false;
        gameCleared = false;

        // 스테이지가 진행될수록 적 생성 주기 줄이기
        float interval = Mathf.Max(0.5f, spawnInterval - (CurrentStageNumber - 1) * fastRate);
        enemySpawner.StartSpawning(interval);
        stageStartUI.ShowStage(CurrentStageNumber, targetKillCount);
        stageStartUI.UpdateKillCount(currentKillCount, targetKillCount);
        // Debug.Log($"Stage {CurrentStageNumber} started. Target kills: {targetKillCount}");
    }
    
    private void OnEnemyKilled()
    {
        if (gameCleared || stageCleared) return;

        currentKillCount++;
        stageStartUI.UpdateKillCount(currentKillCount, targetKillCount);

        // 목표 처치 수 달성시 스테이지 종료
        if (currentKillCount >= targetKillCount)
        {
            ClearStage();
        }
    }

    private void ClearStage()
    {
        stageCleared = true;
        enemySpawner.StopSpawning();

        if (currentStageIndex >= stageTargetKillCounts.Length - 1)
        {
            ClearGame();
            return;
        }

        StartCoroutine(StartNextStageAfterDelay());
    }

    private IEnumerator StartNextStageAfterDelay()
    {
        // 다음 스테이지 시작 전 잠시 대기
        yield return new WaitForSeconds(nextStageDelay);
        StartStage(currentStageIndex + 1);
    }

    private void ClearGame()
    {
        stageCleared = true;
        gameCleared = true;
        currentKillCount = targetKillCount;
        enemySpawner.StopSpawning();
        // Debug.Log("Game Clear!");
        // 엔딩 화면으로 이동
        SceneManager.LoadScene("EndScene");
    }
}
