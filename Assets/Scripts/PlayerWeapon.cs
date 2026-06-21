using UnityEngine;

public class PlayerWeapon : MonoBehaviour
{
    [SerializeField] private Projectile projectilePrefab; // 발사체
    [SerializeField] private Transform firePoint; // 발사 위치
    [SerializeField] private float shotsPerSecond = 5f; // 1초에 몇 발 쏠 건지
    
    private float nextFireTime = 2f;
    
    private void Update()
    {
        Fire();
    }

    private void Fire()
    {
        if (Time.time < nextFireTime) return; // 다음 발사 가능 시점이 되기 전까지 발사 제한
        
        nextFireTime = Time.time + 1f / Mathf.Max(0.01f, shotsPerSecond); // 다음 발사 시점 계산
        // sfx
        AudioManager.Instance.PlayShoot();
        Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
    }
}
