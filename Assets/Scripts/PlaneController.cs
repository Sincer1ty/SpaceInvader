using UnityEngine;
using UnityEngine.InputSystem;

public class PlaneController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float forwardSpeed = 8f;
    
    [Header("Health")]
    [SerializeField] private int maxHp = 5;

    private Vector2 moveInput;
    private Vector3 moveDirection;
    private int currentHp;
    private float nextDamageTime;
    
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

    public void TakeDamage(int damage)
    {
        if (currentHp <= 0f) return;

        currentHp = currentHp - damage;
        GameEvent.OnHpChanged.Invoke(currentHp);
        
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
