using System.Collections;
using TMPro;
using UnityEngine;

public class StageStartUI : MonoBehaviour
{
    private CanvasGroup canvasGroup;
    
    [SerializeField] private TextMeshProUGUI stageText;
    [SerializeField] private float fadeDuration = 0.35f;
    [SerializeField] private float visibleDuration = 2f;
    
    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        canvasGroup.alpha = 0f;
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
        float startAlpha = canvasGroup.alpha;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = fadeDuration <= 0f ? 1f : elapsed / fadeDuration;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
            yield return null;
        }

        canvasGroup.alpha = targetAlpha;
    }
}
