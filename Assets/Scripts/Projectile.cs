using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float speed = 80f;
    [SerializeField] private float lifeTime = 3f;
    [SerializeField] private float damage = 1f; // 적에게 입히는 데미지
    
    private Vector3 moveDirection;
    
    private void Start()
    {
        moveDirection = transform.forward;
    }

    private void Update()
    {
        transform.position += moveDirection * speed * Time.deltaTime;
    }

    private void OnEnable()
    {
        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        EnemyController enemy = other.GetComponentInParent<EnemyController>();

        if (enemy == null) return;

        enemy.TakeDamage(damage);
        Destroy(gameObject);
        Debug.Log("Projectile Hit");
    }
}
