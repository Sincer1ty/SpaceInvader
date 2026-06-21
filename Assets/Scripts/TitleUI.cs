using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleUI : MonoBehaviour
{
    [SerializeField] private string gameSceneName = "GameScene";
    
    public void StartGame()
    {
        SceneManager.LoadScene(gameSceneName); // 게임 씬으로 이동
    }
}
