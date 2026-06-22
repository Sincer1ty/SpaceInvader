using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class StageManager : MonoBehaviour
{
    [Header("Stage")]
    [SerializeField] private EnemySpawner enemySpawner;
    [SerializeField] private StageStartUI stageStartUI;
    [FormerlySerializedAs("stageEnemyCounts")]
    [SerializeField] private int[] stageTargetKillCounts = { 20, 30, 50 };
    [SerializeField] private float spawnInterval = 2f;
    [SerializeField] private float fastRate = 0.3f;

    [Header("Warp Transition")]
    [FormerlySerializedAs("nextStageDelay")]
    [SerializeField, Range(1.2f, 2f)] private float transitionDuration = 1.6f;
    [SerializeField, Range(90f, 110f)] private float targetFov = 100f;
    [SerializeField, Range(0.3f, 0.4f)] private float fadeFinalPortion = 0.35f;
    [SerializeField, Min(0f)] private float fadeResetDuration = 0.25f;

    [Header("Transition References")]
    [SerializeField] private CinemachineCamera virtualCamera;
    [SerializeField] private Camera gameplayCamera;
    private StarfieldSpeedEffect starfieldSpeedEffect;
    private PlaneController player;
    [SerializeField] private CanvasGroup transitionFade;

    private int currentStageIndex;
    private int targetKillCount;
    private int currentKillCount;
    private float originalFov = 60f;
    private bool stageCleared;
    private bool isTransitioning;

    public static bool gameCleared;

    public int CurrentStageNumber => gameCleared ? stageTargetKillCounts.Length : currentStageIndex + 1;
    
    private void Awake()
    {
        starfieldSpeedEffect = gameplayCamera.GetComponent<StarfieldSpeedEffect>();
        player = FindFirstObjectByType<PlaneController>();
        originalFov = virtualCamera.Lens.FieldOfView;
        
        // 시각 효과를 초기 상태로 만들기
        starfieldSpeedEffect?.SetBoostAmount(0f);
        SetCameraFov(originalFov);
        transitionFade.alpha = 0f;
    }

    private void Start()
    {
        GameEvent.EnemyKilled += OnEnemyKilled;
        GameEvent.PlayerDead += OnPlayerDead;
        GameEvent.OnHpChanged += stageStartUI.UpdateHearts;

        StartStage(0); // 1 스테이지 시작
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        
        GameEvent.EnemyKilled -= OnEnemyKilled;
        GameEvent.PlayerDead -= OnPlayerDead;
        GameEvent.OnHpChanged -= stageStartUI.UpdateHearts;
    }

    private void OnPlayerDead()
    {
        gameCleared = false;
        SceneManager.LoadScene("EndScene");
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
    }

    private void OnEnemyKilled() // 적이 죽을 때마다 호출됨
    {
        if (gameCleared || stageCleared || isTransitioning) return;

        currentKillCount++;
        stageStartUI.UpdateKillCount(currentKillCount, targetKillCount);

        // 목표 처치 수 달성시 스테이지 종료
        if (currentKillCount >= targetKillCount)
            ClearStage();
    }

    private void ClearStage()
    {
        if (stageCleared || isTransitioning) return;

        stageCleared = true;
        isTransitioning = true;
        player?.SetTransitionState(true); // 입력을 잠그기
        enemySpawner?.ClearEnemies(); // 남아있는 적 제거
        StartCoroutine(RunStageTransition()); // 전환 연출 시작
    }

    private IEnumerator RunStageTransition() // 전환 연출
    {
        AudioManager.Instance?.PlayBoost(); // 효과음

        float elapsed = 0f;
        float fadeStart = 1f - fadeFinalPortion;
        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            float t = transitionDuration <= 0f ? 1f : Mathf.Clamp01(elapsed / transitionDuration);
            
            SetCameraFov(Mathf.Lerp(originalFov, targetFov, t)); // 카메라 FOV 증가
            starfieldSpeedEffect?.SetBoostAmount(t); // 주변 effect 가속
            
            // 화면 페이드 아웃
            float fadeT = Mathf.InverseLerp(fadeStart, 1f, t);
            transitionFade.alpha = Mathf.SmoothStep(0f, 1f, fadeT);

            yield return null;
        }

        // 목표값으로 보정
        SetCameraFov(targetFov);
        starfieldSpeedEffect?.SetBoostAmount(1f);
        transitionFade.alpha = 1f;
        
        if (currentStageIndex + 1 >= stageTargetKillCounts.Length)
        {
            ClearGame();
            yield break;
        }
        
        // 원래 상태 복구
        SetCameraFov(originalFov);
        starfieldSpeedEffect?.SetBoostAmount(0f);
        yield return FadeTo(0f, fadeResetDuration); // 페이드 인

        player?.SetTransitionState(false); // 플레이어 입력 복구
        isTransitioning = false;
        StartStage(currentStageIndex + 1); // 다음 스테이지 시작
    }

    private IEnumerator FadeTo(float targetAlpha, float duration)
    {
        float startAlpha = transitionFade.alpha;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = duration <= 0f ? 1f : Mathf.Clamp01(elapsed / duration);
            transitionFade.alpha = Mathf.Lerp(startAlpha, targetAlpha, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }
        
        transitionFade.alpha = targetAlpha;
    }

    private void ClearGame()
    {
        stageCleared = true;
        gameCleared = true;
        currentKillCount = targetKillCount;
        enemySpawner?.ClearEnemies();
        SceneManager.LoadScene("EndScene"); // 엔딩 씬으로 이동
    }

    private void SetCameraFov(float fov)
    {
        LensSettings lens = virtualCamera.Lens;
        lens.FieldOfView = fov;
        virtualCamera.Lens = lens;
        
        gameplayCamera.fieldOfView = fov;
    }
}
