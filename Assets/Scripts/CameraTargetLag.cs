using UnityEngine;

[DisallowMultipleComponent]
public sealed class CameraTargetLag : MonoBehaviour // 카메라 지연 팔로우
{
    [Header("Start Inertia")]
    [SerializeField] private bool useScreenPlaneOnly = true;
    [SerializeField, Min(0f)] private float inertiaStrength = 0.0035f;
    [SerializeField, Min(0f)] private float maxInertiaOffset = 0.1f;
    [SerializeField, Min(0.01f)] private float impulseSmoothTime = 0.08f;
    [SerializeField, Min(0.01f)] private float returnSmoothTime = 0.38f;
    [SerializeField, Min(0f)] private float minVelocityChange = 4f;
    [SerializeField, Min(0f)] private float triggerCooldown = 0.16f;

    [Header("Rotation")]
    [SerializeField] private bool followParentRotation = true;

    private Vector3 runtimeLocalOffset;
    private Vector3 shakeOffset;
    private Vector3 inertiaLocalOffset;
    private Vector3 targetInertiaLocalOffset;
    private Vector3 inertiaReturnVelocity;
    private Vector3 inertiaBlendVelocity;
    private Vector3 previousParentPosition;
    private Vector3 previousLocalVelocity;
    private float lastTriggerTime = float.NegativeInfinity;

    private void OnEnable()
    {
        inertiaLocalOffset = Vector3.zero;
        targetInertiaLocalOffset = Vector3.zero;
        inertiaReturnVelocity = Vector3.zero;
        inertiaBlendVelocity = Vector3.zero;
        SetTargetPose();
        CacheParentMotionState(Vector3.zero);
    }

    private void LateUpdate()
    {
        UpdateInertiaOffset();
        SetTargetPose();
    }

    private void UpdateInertiaOffset()
    {
        float deltaTime = Time.deltaTime;
        if (deltaTime <= Mathf.Epsilon)
        {
            return;
        }

        // 부모가 이전 프레임에서 현재 프레임까지 얼마나 움직였는지 계산
        Vector3 worldVelocity = (transform.parent.position - previousParentPosition) / deltaTime;
        // 월드 속도를 부모 로컬 좌표계 속도로 변환
        Vector3 localVelocity = transform.parent.InverseTransformVector(worldVelocity);
        if (useScreenPlaneOnly)
            localVelocity.z = 0f;

        // 이번 프레임 속도와 이전 프레임 속도의 차이
        Vector3 velocityChange = localVelocity - previousLocalVelocity;
        if (ShouldTriggerInertia(localVelocity, velocityChange))
        {
            targetInertiaLocalOffset -= velocityChange * inertiaStrength;
            targetInertiaLocalOffset = Vector3.ClampMagnitude(targetInertiaLocalOffset, maxInertiaOffset);
            lastTriggerTime = Time.time;
        }

        targetInertiaLocalOffset = Vector3.SmoothDamp(
            targetInertiaLocalOffset,
            Vector3.zero,
            ref inertiaReturnVelocity,
            returnSmoothTime,
            Mathf.Infinity,
            deltaTime);

        inertiaLocalOffset = Vector3.SmoothDamp(
            inertiaLocalOffset,
            targetInertiaLocalOffset,
            ref inertiaBlendVelocity,
            impulseSmoothTime,
            Mathf.Infinity,
            deltaTime);

        CacheParentMotionState(localVelocity);
    }

    // 관성 효과를 발생시킬지 결정
    private bool ShouldTriggerInertia(Vector3 localVelocity, Vector3 velocityChange)
    {
        if (Time.time - lastTriggerTime < triggerCooldown)
            return false;

        float currentSpeed = localVelocity.magnitude;
        float previousSpeed = previousLocalVelocity.magnitude;
        // 속도가 작으면 무시
        if (currentSpeed < minVelocityChange || velocityChange.magnitude < minVelocityChange)
            return false;

        bool startedMoving = previousSpeed < minVelocityChange;
        // 기존 이동 방향과 현재 이동 방향이 달라진 경우
        bool changedDirection = previousSpeed >= minVelocityChange
            && Vector3.Dot(localVelocity.normalized, previousLocalVelocity.normalized) < 0.75f;

        return startedMoving || changedDirection;
    }

    private void SetTargetPose()
    {
        Vector3 localOffset = runtimeLocalOffset + shakeOffset + inertiaLocalOffset;
        Vector3 desiredPosition = transform.parent.TransformPoint(localOffset);
        Quaternion desiredRotation = followParentRotation ? transform.parent.rotation : transform.rotation;
        transform.SetPositionAndRotation(desiredPosition, desiredRotation);
    }

    private void CacheParentMotionState(Vector3 localVelocity)
    {
        previousParentPosition = transform.parent.position;
        previousLocalVelocity = localVelocity;
    }
}
