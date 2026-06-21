using UnityEngine;

[DisallowMultipleComponent]
public sealed class CameraTargetLag : MonoBehaviour
{
    [Header("Base Follow")]
    [SerializeField] private Vector3 baseLocalOffset = Vector3.zero;

    [Header("Start Inertia")]
    [SerializeField] private bool useScreenPlaneOnly = true;
    [SerializeField, Min(0f)] private float inertiaStrength = 0.018f;
    [SerializeField, Min(0f)] private float maxInertiaOffset = 0.32f;
    [SerializeField, Min(0.01f)] private float returnSmoothTime = 0.22f;
    [SerializeField, Min(0f)] private float minVelocityChange = 0.5f;

    [Header("Rotation")]
    [SerializeField] private bool followParentRotation = true;

    private Vector3 runtimeLocalOffset;
    private Vector3 shakeOffset;
    private Vector3 inertiaLocalOffset;
    private Vector3 inertiaReturnVelocity;
    private Vector3 previousParentPosition;
    private Vector3 previousLocalVelocity;
    private bool initialized;

    // public Vector3 BaseLocalOffset
    // {
    //     get => baseLocalOffset;
    //     set => baseLocalOffset = value;
    // }
    //
    // public Vector3 RuntimeLocalOffset
    // {
    //     get => runtimeLocalOffset;
    //     set => runtimeLocalOffset = value;
    // }
    //
    // public Vector3 ShakeOffset
    // {
    //     get => shakeOffset;
    //     set => shakeOffset = value;
    // }

    private void OnEnable()
    {
        SnapToDesiredPose();
    }

    private void LateUpdate()
    {
        // if (transform.parent == null)
        // {
        //     return;
        // }

        if (!Application.isPlaying)
        {
            inertiaLocalOffset = Vector3.zero;
            SetTargetPose();
            CacheParentMotionState(Vector3.zero);
            initialized = true;
            return;
        }

        if (!initialized)
        {
            SnapToDesiredPose();
        }

        UpdateInertiaOffset();
        SetTargetPose();
    }

    public void SnapToDesiredPose()
    {
        // if (transform.parent == null)
        // {
        //     initialized = true;
        //     return;
        // }

        inertiaLocalOffset = Vector3.zero;
        inertiaReturnVelocity = Vector3.zero;
        SetTargetPose();
        CacheParentMotionState(Vector3.zero);
        initialized = true;
    }

    private void UpdateInertiaOffset()
    {
        float deltaTime = Time.deltaTime;
        if (deltaTime <= Mathf.Epsilon)
        {
            return;
        }

        Vector3 worldVelocity = (transform.parent.position - previousParentPosition) / deltaTime;
        Vector3 localVelocity = transform.parent.InverseTransformVector(worldVelocity);
        if (useScreenPlaneOnly)
        {
            localVelocity.z = 0f;
        }

        Vector3 velocityChange = localVelocity - previousLocalVelocity;
        if (velocityChange.magnitude >= minVelocityChange)
        {
            inertiaLocalOffset -= velocityChange * inertiaStrength;
            inertiaLocalOffset = Vector3.ClampMagnitude(inertiaLocalOffset, maxInertiaOffset);
        }

        inertiaLocalOffset = Vector3.SmoothDamp(
            inertiaLocalOffset,
            Vector3.zero,
            ref inertiaReturnVelocity,
            returnSmoothTime,
            Mathf.Infinity,
            deltaTime);

        CacheParentMotionState(localVelocity);
    }

    private void SetTargetPose()
    {
        Vector3 localOffset = baseLocalOffset + runtimeLocalOffset + shakeOffset + inertiaLocalOffset;
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
