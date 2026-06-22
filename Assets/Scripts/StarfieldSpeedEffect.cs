using UnityEngine;

[ExecuteAlways]
[DisallowMultipleComponent]
[RequireComponent(typeof(Camera))]
public sealed class StarfieldSpeedEffect : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private ParticleSystem starParticleSystem;

    [Header("Star Shape")]
    [SerializeField, Min(0.001f)] private float minStarSize = 0.012f;
    [SerializeField, Min(0.001f)] private float maxStarSize = 0.03f;
    [SerializeField, Min(1)] private int maxParticles = 420;

    [Header("Visibility Distribution")]
    [SerializeField, Range(0f, 0.2f)] private float viewportPadding = 0.035f;
    [SerializeField] private Vector2 centerDeadZone = new Vector2(0.23f, 0.18f);
    [SerializeField, Range(1f, 3f)] private float edgeBias = 1.45f;
    [SerializeField, Min(0.1f)] private float minSpawnDepth = 7f;
    [SerializeField, Min(0.1f)] private float maxSpawnDepth = 18f;
    [SerializeField, Range(0.2f, 1f)] private float farDepthBias = 0.55f;
    
    [Header("Speed Response")]
    [SerializeField, Min(0.01f)] private float referenceSpeed = 25f;
    [SerializeField, Min(0f)] private float idleFlowSpeed = 2.2f;
    [SerializeField, Min(0f)] private float maxFlowSpeed = 15f;
    [SerializeField, Min(0f)] private float screenDirectionInfluence = 6f;
    [SerializeField, Min(0f)] private float maxScreenFlowSpeed = 4f;
    [SerializeField, Min(0.01f)] private float responseSmoothTime = 0.16f;

    [Header("Spawn Rate")]
    [SerializeField, Min(0f)] private float idleSpawnRate = 55f;
    [SerializeField, Min(0f)] private float maxSpawnRate = 150f;

    [Header("Stretched Billboard")]
    [SerializeField, Min(0f)] private float idleLengthScale = 0.18f;
    [SerializeField, Min(0f)] private float maxLengthScale = 1.25f;
    [SerializeField, Min(0f)] private float idleVelocityScale = 0.08f;
    [SerializeField, Min(0f)] private float maxVelocityScale = 0.42f;

    [Header("External Boost")]
    [SerializeField, Min(1f)] private float boostSpeedMultiplier = 4f;
    [SerializeField, Min(1f)] private float boostSpawnMultiplier = 2.5f;
    [SerializeField, Min(1f)] private float boostLengthMultiplier = 2.5f;
    [SerializeField, Min(1f)] private float boostVelocityScaleMultiplier = 2f;

    private Camera targetCamera;
    private Vector3 previousPlayerPosition;
    private Vector3 previousViewportPosition;
    private Vector3 currentVelocity;
    private Vector3 velocitySmoothRef;
    private float speed01;
    private float rateSmoothRef;
    private float lengthSmoothRef;
    private float velocityScaleSmoothRef;
    private float currentRate;
    private float currentLengthScale;
    private float currentVelocityScale;
    private float emissionAccumulator;
    private float boostAmount;
    private bool hasPreviousPlayerPosition;

    private void Awake()
    {
        targetCamera = GetComponent<Camera>();
        
        CachePlayerState();
        InitializeValues();
        
        ParticleSystem.MainModule main = starParticleSystem.main;
        main.maxParticles = maxParticles;
        
        ApplyParticleMotionSettings();

        starParticleSystem.Clear();
        PrewarmStarfield();
        starParticleSystem.Play();
    }

    private void LateUpdate()
    {
        UpdatePlayerVelocity(); // 플레이어 속도 계산
        UpdateParticleMotion();
        EmitStars(Application.isPlaying ? Time.deltaTime : 0f);
    }

    // 플레이어의 이전 위치와 현재 위치를 비교해서 속도 계산
    // 플레이어가 빠르게 움직일수록 speed01 값이 커짐
    private void UpdatePlayerVelocity()
    {
        float deltaTime = Time.deltaTime;
        if (!hasPreviousPlayerPosition) // 이전 위치가 없으면 종료
        {
            CachePlayerState();
            return;
        }

        // 플레이어 실제 속도 계산
        Vector3 worldVelocity = (player.position - previousPlayerPosition) / deltaTime;
        // 화면 속도 계산
        Vector3 viewportPosition = targetCamera.WorldToViewportPoint(player.position);
        Vector3 viewportVelocity = (viewportPosition - previousViewportPosition) / deltaTime;
        // 플레이어의 월드 이동 속도를 0~1 범위로 정규화
        speed01 = Mathf.Clamp01(worldVelocity.magnitude / referenceSpeed);

        // 플레이어가 화면에서 움직이는 방향의 반대 방향
        Vector3 desiredVelocity = new Vector3(
            Mathf.Clamp(-viewportVelocity.x * screenDirectionInfluence, -maxScreenFlowSpeed, maxScreenFlowSpeed),
            Mathf.Clamp(-viewportVelocity.y * screenDirectionInfluence, -maxScreenFlowSpeed, maxScreenFlowSpeed),
            -Mathf.Lerp(idleFlowSpeed, maxFlowSpeed, speed01) * Mathf.Lerp(1f, boostSpeedMultiplier, boostAmount));

        // 부드럽게 변경
        currentVelocity = Vector3.SmoothDamp(currentVelocity, desiredVelocity, ref velocitySmoothRef, responseSmoothTime, Mathf.Infinity, deltaTime);
        // 현재 상태 저장
        previousPlayerPosition = player.position;
        previousViewportPosition = viewportPosition;
    }

    // 효과의 강도를 계산하여 ParticleSystem 설정에 반영
    private void UpdateParticleMotion()
    {
        float deltaTime = Time.deltaTime;
        // 목표 생성량 계산
        float targetRate = Mathf.Lerp(idleSpawnRate, maxSpawnRate, speed01) * Mathf.Lerp(1f, boostSpawnMultiplier, boostAmount);
        // 목표 길이 계산
        float targetLengthScale = Mathf.Lerp(idleLengthScale, maxLengthScale, speed01) * Mathf.Lerp(1f, boostLengthMultiplier, boostAmount);
        // 목표 Velocity Scale 계산
        float targetVelocityScale = Mathf.Lerp(idleVelocityScale, maxVelocityScale, speed01) * Mathf.Lerp(1f, boostVelocityScaleMultiplier, boostAmount);
        // 부드럽게 변경
        currentRate = Mathf.SmoothDamp(currentRate, targetRate, ref rateSmoothRef, responseSmoothTime, Mathf.Infinity, deltaTime);
        currentLengthScale = Mathf.SmoothDamp(currentLengthScale, targetLengthScale, ref lengthSmoothRef, responseSmoothTime, Mathf.Infinity, deltaTime);
        currentVelocityScale = Mathf.SmoothDamp(currentVelocityScale, targetVelocityScale, ref velocityScaleSmoothRef, responseSmoothTime, Mathf.Infinity, deltaTime);

        ApplyParticleMotionSettings(); // ParticleSystem에 적용
    }

    private void InitializeValues() // 설정 초기화
    {
        currentVelocity = new Vector3(0f, 0f, -idleFlowSpeed);
        currentRate = idleSpawnRate;
        currentLengthScale = idleLengthScale;
        currentVelocityScale = idleVelocityScale;
        emissionAccumulator = 0f;
    }
    
    private void ApplyParticleMotionSettings()
    {
        ParticleSystem.VelocityOverLifetimeModule velocityOverLifetime = starParticleSystem.velocityOverLifetime;
        velocityOverLifetime.x = currentVelocity.x;
        velocityOverLifetime.y = currentVelocity.y;
        velocityOverLifetime.z = currentVelocity.z;

        ParticleSystemRenderer particleRenderer = starParticleSystem.GetComponent<ParticleSystemRenderer>();
        particleRenderer.lengthScale = currentLengthScale;
        particleRenderer.velocityScale = currentVelocityScale;
    }

    // 따라 프레임마다 몇 개 생성할지 계산
    private void EmitStars(float deltaTime)
    {
        if (deltaTime <= 0f) return;

        emissionAccumulator += currentRate * deltaTime; // 누적 생성량 계산
        int emitCount = Mathf.Min(Mathf.FloorToInt(emissionAccumulator), 32); // 생성 개수
        emissionAccumulator -= emitCount;

        for (int i = 0; i < emitCount; i++)
        {
            EmitStar(); // 생성
        }
    }

    private void PrewarmStarfield()
    {
        int prewarmCount = Mathf.Min(maxParticles, Mathf.CeilToInt(idleSpawnRate));
        for (int i = 0; i < prewarmCount; i++)
        {
            EmitStar();
        }
    }

    // 랜덤 위치를 고르고, 그 위치에 파티클을 하나 생성
    private void EmitStar()
    {
        // 화면 안 생성 위치 가져오기
        Vector2 viewportPosition = GetViewportSpawnPosition();
        float depthT = Mathf.Pow(Random.value, farDepthBias);
        float depth = Mathf.Lerp(minSpawnDepth, maxSpawnDepth, depthT);
        // World Position 으로 변환
        Vector3 worldPosition = targetCamera.ViewportToWorldPoint(
            new Vector3(viewportPosition.x, viewportPosition.y, depth));

        ParticleSystem.EmitParams emitParams = new ParticleSystem.EmitParams
        {
            position = starParticleSystem.transform.InverseTransformPoint(worldPosition),
            startSize = Random.Range(minStarSize, maxStarSize) * Mathf.Lerp(0.55f, 1f, depthT),
            applyShapeToPosition = false
        };

        starParticleSystem.Emit(emitParams, 1);
    }

    // 가장자리 쪽에 더 많이 나오도록 위치 계산
    private Vector2 GetViewportSpawnPosition()
    {
        Vector2 viewportPosition = Vector2.zero;
        // 사용 가능한 화면 영역 계산
        float usableHalfWidth = 0.5f - viewportPadding;
        float usableHalfHeight = 0.5f - viewportPadding;

        for (int attempt = 0; attempt < 16; attempt++)
        {
            // 랜덤값 생성
            float x = BiasTowardEdge(Random.Range(-1f, 1f));
            float y = BiasTowardEdge(Random.Range(-1f, 1f));
            viewportPosition = new Vector2(
                0.5f + x * usableHalfWidth,
                0.5f + y * usableHalfHeight);

            // 데드존 계산
            float deadZoneX = centerDeadZone.x > 0f ? (viewportPosition.x - 0.5f) / centerDeadZone.x : float.PositiveInfinity;
            float deadZoneY = centerDeadZone.y > 0f ? (viewportPosition.y - 0.5f) / centerDeadZone.y : float.PositiveInfinity;
            // 화면 중앙은 비워 두어 플레이어 시야가 가려지지 않도록 하기
            if (deadZoneX * deadZoneX + deadZoneY * deadZoneY >= 1f)
            {
                return viewportPosition;
            }
        }

        return new Vector2(viewportPadding, Random.Range(viewportPadding, 1f - viewportPadding));
    }

    private float BiasTowardEdge(float value) // 랜덤값을 화면 가장자리 방향으로 밀어내기
    {
        return Mathf.Sign(value) * Mathf.Pow(Mathf.Abs(value), 1f / edgeBias);
    }

    // 스테이지 매니저에서 호출
    public void SetBoostAmount(float amount)
    {
        boostAmount = Mathf.Clamp01(amount);
    }

    private void CachePlayerState() // 플레이어 위치 캐싱
    {
        previousPlayerPosition = player.position;
        previousViewportPosition = targetCamera.WorldToViewportPoint(player.position);
        hasPreviousPlayerPosition = true;
    }
}
