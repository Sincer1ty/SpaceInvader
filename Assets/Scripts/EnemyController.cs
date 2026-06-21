using UnityEngine;

public class EnemyController : MonoBehaviour
{
    private Transform target;
    private Vector3 localTargetOffset;
    private Vector3 currentMoveDirection;
    private bool isDead;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 30f;
    [SerializeField] private float turnSpeed = 6f;
    [SerializeField] private float trackingStrength = 1f;
    [SerializeField] private Vector2 speedRange = new Vector2(0.9f, 1.1f);
    [SerializeField] private float stopDistance = 1f;
    [SerializeField] private Vector2 sideOffsetRange = new Vector2(3f, 10f);
    [SerializeField] private Vector2 verticalOffsetRange = new Vector2(-2f, 5f);
    [SerializeField] private Vector2 forwardOffsetRange = new Vector2(-2f, 4f);

    [Header("Approach Boost")]
    [SerializeField] private float farApproachDistance = 180f;
    [SerializeField] private float normalApproachDistance = 70f;
    [SerializeField] private float farSpeedMultiplier = 4f;

    [Header("Combat")]
    [SerializeField] private float maxHp = 1f;
    [SerializeField] private int collisionDamage = 1; // player 에게 입히는 데미지
    
    private float currentHp;

    private void OnValidate()
    {
        farApproachDistance = Mathf.Max(0.1f, farApproachDistance);
        normalApproachDistance = Mathf.Clamp(normalApproachDistance, 0.1f, farApproachDistance);
        farSpeedMultiplier = Mathf.Max(1f, farSpeedMultiplier);
    }
    
    private void Update()
    {
        if (target == null || isDead) return;
        
        Vector3 targetPosition = GetTargetPosition();
        Vector3 directionToTarget = targetPosition - transform.position; // 타겟까지의 거리
        if (directionToTarget.magnitude <= stopDistance)
        {
            HitPlayer();
            return;
        }
        
        Vector3 desiredDirection = directionToTarget.normalized;
        MoveToward(desiredDirection);
    }

    public void Initialize(Transform player)
    {
        currentHp = maxHp;
        isDead = false;
        target = player;
        SetData(moveSpeed, turnSpeed, trackingStrength, speedRange,
            sideOffsetRange, verticalOffsetRange, forwardOffsetRange);
    }
    
    public void SetData(float baseMoveSpeed, float newTurnSpeed, float newTrackingStrength,
        Vector2 speedMultiplierRange, Vector2 newSideOffsetRange, Vector2 newVerticalOffsetRange,
        Vector2 newForwardOffsetRange)
    {
        moveSpeed = baseMoveSpeed * Random.Range(speedMultiplierRange.x, speedMultiplierRange.y);
        turnSpeed = Mathf.Max(0.01f, newTurnSpeed);
        trackingStrength = Mathf.Max(0.01f, newTrackingStrength);
        sideOffsetRange = newSideOffsetRange;
        verticalOffsetRange = newVerticalOffsetRange;
        forwardOffsetRange = newForwardOffsetRange;
        localTargetOffset = CreateLocalTargetOffset();
    }

    private void MoveToward(Vector3 desiredDirection)
    {
        float turnAmount = turnSpeed * trackingStrength * Time.deltaTime;
        currentMoveDirection = Vector3.Slerp(currentMoveDirection, desiredDirection, turnAmount).normalized;
        transform.position += currentMoveDirection * GetCurrentMoveSpeed() * Time.deltaTime;
        transform.rotation = Quaternion.LookRotation(currentMoveDirection);
    }

    private float GetCurrentMoveSpeed()
    {
        if (target == null) return moveSpeed;

        float distanceToPlayer = Vector3.Distance(transform.position, target.position);
        float boostWeight = Mathf.InverseLerp(normalApproachDistance, farApproachDistance, distanceToPlayer);
        float speedMultiplier = Mathf.Lerp(1f, farSpeedMultiplier, boostWeight);
        return moveSpeed * speedMultiplier;
    }

    private Vector3 GetTargetPosition()
    {
        return target.position
               + target.right * localTargetOffset.x
               + target.up * localTargetOffset.y
               + target.forward * localTargetOffset.z;
    }

    private Vector3 CreateLocalTargetOffset()
    {
        float side = Random.Range(sideOffsetRange.x, sideOffsetRange.y);
        if (Random.value < 0.5f) side *= -1f;

        float vertical = Random.Range(verticalOffsetRange.x, verticalOffsetRange.y);
        float forward = Random.Range(forwardOffsetRange.x, forwardOffsetRange.y);
        return new Vector3(side, vertical, forward);
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;
        
        currentHp -= damage;

        if (currentHp <= 0f)
        {
            GameEvent.EnemyKilled?.Invoke();
            Die();
        }
    }
    
    private void HitPlayer()
    {
        PlaneController player = target.GetComponent<PlaneController>();
        if (player != null)
            player.TakeDamage(collisionDamage);

        Die();
        GameEvent.HitPlayer?.Invoke();
    }

    private void Die()
    {
        if (isDead) return;

        isDead = true;
        Destroy(gameObject);
    }
}
