using UnityEngine;

public class EnemyController : MonoBehaviour
{
    private Transform target;
    private Vector3 personalTargetOffset;
    [SerializeField] private float targetOffsetFadeDistance = 20f;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 30f;
    [SerializeField] private float stopDistance = 1f;
    [SerializeField] private float targetOffsetRadius = 4f;
    
    private void Update()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, target.position); // player 까지의 거리
        float offsetWeight = Mathf.Clamp01((distanceToPlayer - stopDistance) / targetOffsetFadeDistance); // 오프셋 적용 비율
        Vector3 targetPosition = target.position + personalTargetOffset * offsetWeight;
        Vector3 directionToTarget = targetPosition - transform.position; // 타겟까지의 거리
        if (directionToTarget.magnitude <= stopDistance)
        {
            gameObject.SetActive(false);
            return;
        }
        
        Vector3 desiredDirection = directionToTarget.normalized;
        Move(desiredDirection);
    }

    public void Initialize(Transform player)
    {
        target = player.transform;
        SetData(moveSpeed, new Vector2(0.9f, 1.1f), 
            new Vector2(1f, targetOffsetRadius),
            new Vector2(10f, 20f));
    }
    
    public void SetData(float baseMoveSpeed, Vector2 speedMultiplierRange, 
        Vector2 targetOffsetRadiusRange, Vector2 targetOffsetFadeDistanceRange)
    {
        moveSpeed = Mathf.Max(0f, baseMoveSpeed * Random.Range(speedMultiplierRange.x, speedMultiplierRange.y));
        targetOffsetRadius = Random.Range(targetOffsetRadiusRange.x, targetOffsetRadiusRange.y);
        personalTargetOffset = new Vector3(Random.Range(-targetOffsetRadius, targetOffsetRadius), 
            Random.Range(-targetOffsetRadius, targetOffsetRadius), 0f);
    }

    private void Move(Vector3 moveDirection)
    {
        transform.position += moveDirection * moveSpeed * Time.deltaTime;
        transform.rotation = Quaternion.LookRotation(moveDirection);
    }
}
