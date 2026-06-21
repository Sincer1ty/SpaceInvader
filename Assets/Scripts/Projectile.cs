using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float speed = 80f;
    [SerializeField] private float lifeTime = 3f;
    [SerializeField] private float damage = 1f; // 적에게 입히는 데미지
    
    private void Update()
    {
        // 발사 방향으로 지속 이동
        transform.position += transform.forward * speed * Time.deltaTime;
    }

    private void OnEnable()
    {
        Destroy(gameObject, lifeTime); // 일정 시간이 지나면 자동 제거
    }

    private void OnTriggerEnter(Collider other)
    {
        // 충돌한 오브젝트에서 적 컴포넌트 찾기
        EnemyController enemy = other.GetComponent<EnemyController>();

        if (enemy == null) return;

        enemy.TakeDamage(damage);
        Destroy(gameObject);
        // Debug.Log("Projectile Hit");
    }
}
