using TMPro;
using UnityEngine;

public class EndUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI resultText;

    // UI에 표시할 문구
    [SerializeField] private string gameClearText = "GAME CLEAR !";
    [SerializeField] private string gameOverText = "GAME OVER...";
    
    private void Start()
    {
        // 게임 결과에 따라 종료 화면 문구 설정
        if (StageManager.gameCleared)
        {
            resultText.text = gameClearText;
        }
        else
        {
            resultText.text = gameOverText;
        }
    }
}
