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
        // 전방 자동 이동 + 플레이어 입력 이동을 합산
        Vector3 movement = transform.forward * forwardSpeed;
        movement += moveDirection * moveSpeed;

        transform.position += movement * Time.deltaTime;
    }

    public void TakeDamage(int damage)
    {
        if (currentHp <= 0f) return; // 이미 사망한 경우 무시

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
        GameEvent.PlayerDead?.Invoke(); // 플레이어 사망 이벤트 발생
    }
}
