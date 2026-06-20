using UnityEngine;

public class PlayerWeapon : MonoBehaviour
{
    [SerializeField] private Projectile projectilePrefab; // 발사체
    [SerializeField] private Transform firePoint; // 발사 위치
    [SerializeField] private float fireRate = 5f; // 1초에 몇 발 쏠 건지
    
    [SerializeField] private float nextFireTime = 2f;
    
    private void Update()
    {
        Fire();
    }

    private void Fire()
    {
        if (Time.time < nextFireTime) return;

        nextFireTime = Time.time + 1f / Mathf.Max(0.01f, fireRate);
        Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
    }
}
