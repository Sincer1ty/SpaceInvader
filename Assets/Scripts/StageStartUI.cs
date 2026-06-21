using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StageStartUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI stageText;
    [SerializeField] private float fadeDuration = 0.35f;
    [SerializeField] private float visibleDuration = 2f;
    [SerializeField] private Image[] hearts;
    [SerializeField] private TextMeshProUGUI killText;

    private void Awake()
    {
        Color color = stageText.color;
        color.a = 0f;
        stageText.color = color;
    }

    public void ShowStage(int stageNumber)
    {
        StopAllCoroutines();
        
        stageText.text = $"STAGE {stageNumber}";
        StartCoroutine(ShowRoutine());
    }

    private IEnumerator ShowRoutine()
    {
        yield return FadeTo(1f);
        yield return new WaitForSeconds(visibleDuration);
        yield return FadeTo(0f);
    }

    private IEnumerator FadeTo(float targetAlpha)
    {
        Color color = stageText.color;
        float startAlpha = color.a;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = fadeDuration <= 0f ? 1f : elapsed / fadeDuration;
            color.a = Mathf.Lerp(startAlpha, targetAlpha, t);
            stageText.color = color;
            yield return null;
        }

        color.a = targetAlpha;
        stageText.color = color;
    }

    public void BreakHeart(int currentHp)
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
