using TMPro;
using UnityEngine;

public class EndUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI resultText;

    private void Start()
    {
        if (StageManager.gameCleared)
        {
            resultText.text = "GAME CLEAR !";
        }
        else
        {
            resultText.text = "GAME OVER...";
        }
    }
}
