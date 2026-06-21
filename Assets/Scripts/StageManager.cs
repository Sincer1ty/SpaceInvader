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
        GameEvent.OnHpChanged += stageStartUI.BreakHeart;
        GameEvent.PlayerDead += OnPlayerDead;
    }
    
    private void OnDisable()
    {
        StopAllCoroutines();
        
        GameEvent.EnemyKilled -= OnEnemyKilled;
        GameEvent.OnHpChanged -= stageStartUI.BreakHeart;
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

        currentStageIndex = stageIndex;
        targetKillCount = stageTargetKillCounts[currentStageIndex];
        currentKillCount = 0;
        stageCleared = false;
        gameCleared = false;

        enemySpawner.StartSpawning(targetKillCount);
        stageStartUI.ShowStage(CurrentStageNumber);
        stageStartUI.UpdateKillCount(currentKillCount, targetKillCount);
        Debug.Log($"Stage {CurrentStageNumber} started. Target kills: {targetKillCount}", this);
    }
    
    private void OnEnemyKilled()
    {
        if (gameCleared || stageCleared) return;

        currentKillCount++;
        stageStartUI.UpdateKillCount(currentKillCount, targetKillCount);
        Debug.Log($"Stage {CurrentStageNumber} kills: {currentKillCount}/{targetKillCount}", this);

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
        yield return new WaitForSeconds(nextStageDelay);
        StartStage(currentStageIndex + 1);
    }

    private void ClearGame()
    {
        stageCleared = true;
        gameCleared = true;
        currentKillCount = targetKillCount;
        enemySpawner.StopSpawning();
        Debug.Log("Game Clear!", this);
        gameCleared = true;
        SceneManager.LoadScene("EndScene");
    }
}
