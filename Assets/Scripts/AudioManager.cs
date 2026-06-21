using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private AudioSource sfxSource; // SFX AudioSource 컴포넌트 
    [SerializeField] private AudioClip explosionClip; // 폭발음

    public static AudioManager Instance { get; private set; } // 싱글톤 인스턴스

    private void Awake()
    {
        if (Instance != null && Instance != this) // 이미 AudioManager가 존재하면 중복 객체 제거
        {
            Destroy(gameObject);
            return;
        }

        Instance = this; // 현재 객체를 싱글톤 인스턴스로 등록
    }
    
    public void PlayExplosion() // 폭발 효과음 재생
    {
        sfxSource.PlayOneShot(explosionClip);
    }
}
