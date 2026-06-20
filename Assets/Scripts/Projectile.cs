using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float speed = 80f;
    [SerializeField] private float lifeTime = 3f;
    
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
        Destroy(gameObject);
    }
}
