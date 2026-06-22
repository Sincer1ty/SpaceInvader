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
    private bool inputLocked;
    private bool invulnerable;

    public bool IsInputLocked => inputLocked;
    
    private void Awake()
    {
        currentHp = maxHp;
    }
    
    public void OnMove(InputValue value) // Input System에서 이동 입력을 받을 때 호출
    {
        if (inputLocked)
        {
            moveInput = Vector2.zero;
            moveDirection = Vector3.zero;
            return;
        }

        moveInput = value.Get<Vector2>();
        moveDirection = new Vector3(moveInput.x, moveInput.y, 0f);
    }
    
    void Update()
    {
        // 전방 이동 + 플레이어 입력 이동 합산
        Vector3 movement = transform.forward * forwardSpeed;
        movement += moveDirection * moveSpeed;

        transform.position += movement * Time.deltaTime;
    }

    public void TakeDamage(int damage)
    {
        if (currentHp <= 0f || invulnerable) return; // 이미 사망했거나 전환 중인 경우 무시

        currentHp = currentHp - damage;
        GameEvent.OnHpChanged?.Invoke(currentHp);
        // Debug.Log($"Player HP: {currentHp}/{maxHp}", this);

        if (currentHp <= 0f)
        {
            Die();
        }
    }

    private void Die()
    {
        GameEvent.PlayerDead?.Invoke(); // 플레이어 사망 이벤트 발생
    }

    // 스테이지 전환 중 입력 잠그기
    public void SetTransitionState(bool active)
    {
        inputLocked = active;
        invulnerable = active;

        if (active)
        {
            moveInput = Vector2.zero;
            moveDirection = Vector3.zero;
        }
    }
}
