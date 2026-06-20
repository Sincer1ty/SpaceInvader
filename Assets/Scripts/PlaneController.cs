using UnityEngine;
using UnityEngine.InputSystem;

public class PlaneController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float forwardSpeed = 8f;
    
    [Header("Health")]
    [SerializeField] private float maxHp = 5f;

    private Vector2 moveInput;
    private Vector3 moveDirection;
    private float currentHp;
    private float nextDamageTime;

    public float CurrentHp => currentHp;
    public float MaxHp => maxHp;
    
    private void Awake()
    {
        currentHp = maxHp;
    }
    
    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
        moveDirection = new Vector3(moveInput.x, moveInput.y, 0f);
    }
    
    void Update()
    {
        Vector3 movement = transform.forward * forwardSpeed;
        movement += moveDirection * moveSpeed;

        transform.position += movement * Time.deltaTime;
    }

    public void TakeDamage(float damage)
    {
        if (currentHp <= 0f) return;

        currentHp = Mathf.Max(0f, currentHp - Mathf.Max(0f, damage));
        
        Debug.Log($"Player HP: {currentHp}/{maxHp}", this);

        if (currentHp <= 0f)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("Player destroyed.", this);
        gameObject.SetActive(false);
    }
}
