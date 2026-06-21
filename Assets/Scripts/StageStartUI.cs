using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StageStartUI : MonoBehaviour
{
    [SerializeField] private CanvasGroup stagePanel;
    [SerializeField] private TextMeshProUGUI stageText;
    [SerializeField] private float fadeDuration = 0.35f;
    [SerializeField] private float visibleDuration = 2f;
    [SerializeField] private Image[] hearts;
    [SerializeField] private TextMeshProUGUI killText;
    [SerializeField] private TextMeshProUGUI missionText;

    private void Awake()
    {
        stagePanel.alpha = 0f;
    }

    public void ShowStage(int stageNumber, int count)
    {
        StopAllCoroutines(); // 이전에 실행 중인 코루틴 중단
        
        stageText.text = $"STAGE {stageNumber}";
        missionText.text = $"적 {count}명을 죽여라.";
        StartCoroutine(ShowRoutine());
    }

    private IEnumerator ShowRoutine()
    {
        yield return FadeTo(1f); // 페이드 인
        yield return new WaitForSeconds(visibleDuration); // 일정 시간 표시
        yield return FadeTo(0f); // 페이드 아웃
    }

    // CanvasGroup의 알파값을 부드럽게 전환
    private IEnumerator FadeTo(float targetAlpha)
    {
        float startAlpha = stagePanel.alpha;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = fadeDuration <= 0f ? 1f : elapsed / fadeDuration;
            stagePanel.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
            yield return null;
        }

        stagePanel.alpha = targetAlpha;
    }

    public void UpdateHearts(int currentHp) // 현재 체력 수만큼 하트 표시
    {
        for (int i = 0; i < hearts.Length; i++)
        {
            hearts[i].enabled = i < currentHp;
        }
    }
    
    public void UpdateKillCount(int killCount, int targetCount)
    {
        killText.text = $"{killCount}/{targetCount} KILL";
    }
}
