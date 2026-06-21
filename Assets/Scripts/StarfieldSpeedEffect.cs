using UnityEngine;

[ExecuteAlways]
[DisallowMultipleComponent]
[RequireComponent(typeof(Camera))]
public sealed class StarfieldSpeedEffect : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private ParticleSystem starParticleSystem;
    [SerializeField] private bool autoFindPlayer = true;

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
    private bool hasPreviousPlayerPosition;

    private void Awake()
    {
        targetCamera = GetComponent<Camera>();
        ResolvePlayer();
    }

    private void OnEnable()
    {
        ResolvePlayer();
        CachePlayerState();
        InitializeRuntimeValues();
        ConfigureSpawnDistribution();
        ApplyDynamicParticleSettings();

        if (Application.isPlaying)
        {
            starParticleSystem.Clear();
            PrewarmStarfield();
            starParticleSystem.Play();
        }
    }

    private void OnValidate()
    {
        referenceSpeed = Mathf.Max(0.01f, referenceSpeed);
        responseSmoothTime = Mathf.Max(0.01f, responseSmoothTime);
        maxParticles = Mathf.Max(1, maxParticles);
        minStarSize = Mathf.Max(0.001f, minStarSize);
        maxStarSize = Mathf.Max(minStarSize, maxStarSize);
        centerDeadZone.x = Mathf.Clamp(centerDeadZone.x, 0f, 0.45f);
        centerDeadZone.y = Mathf.Clamp(centerDeadZone.y, 0f, 0.45f);
        minSpawnDepth = Mathf.Max(0.1f, minSpawnDepth);
        maxSpawnDepth = Mathf.Max(minSpawnDepth, maxSpawnDepth);
        idleSpawnRate = Mathf.Max(0f, idleSpawnRate);
        maxSpawnRate = Mathf.Max(idleSpawnRate, maxSpawnRate);
        idleLengthScale = Mathf.Max(0f, idleLengthScale);
        maxLengthScale = Mathf.Max(idleLengthScale, maxLengthScale);
        idleVelocityScale = Mathf.Max(0f, idleVelocityScale);
        maxVelocityScale = Mathf.Max(idleVelocityScale, maxVelocityScale);

        if (starParticleSystem != null)
        {
            ConfigureSpawnDistribution();
            ApplyDynamicParticleSettings();
        }
    }

    private void LateUpdate()
    {
        UpdatePlayerVelocity();
        UpdateParticleMotion();
        EmitStars(Application.isPlaying ? Time.deltaTime : 0f);
    }

    private void UpdatePlayerVelocity()
    {
        if (player == null || targetCamera == null)
        {
            currentVelocity = Vector3.SmoothDamp(currentVelocity, new Vector3(0f, 0f, -idleFlowSpeed), ref velocitySmoothRef, responseSmoothTime);
            speed01 = Mathf.SmoothStep(speed01, 0f, Time.deltaTime / responseSmoothTime);
            return;
        }

        float deltaTime = Application.isPlaying ? Time.deltaTime : 1f / 60f;
        if (deltaTime <= Mathf.Epsilon || !hasPreviousPlayerPosition)
        {
            CachePlayerState();
            return;
        }

        Vector3 worldVelocity = (player.position - previousPlayerPosition) / deltaTime;
        Vector3 viewportPosition = targetCamera.WorldToViewportPoint(player.position);
        Vector3 viewportVelocity = (viewportPosition - previousViewportPosition) / deltaTime;
        speed01 = Mathf.Clamp01(worldVelocity.magnitude / referenceSpeed);

        Vector3 desiredVelocity = new Vector3(
            Mathf.Clamp(-viewportVelocity.x * screenDirectionInfluence, -maxScreenFlowSpeed, maxScreenFlowSpeed),
            Mathf.Clamp(-viewportVelocity.y * screenDirectionInfluence, -maxScreenFlowSpeed, maxScreenFlowSpeed),
            -Mathf.Lerp(idleFlowSpeed, maxFlowSpeed, speed01));

        currentVelocity = Vector3.SmoothDamp(currentVelocity, desiredVelocity, ref velocitySmoothRef, responseSmoothTime, Mathf.Infinity, deltaTime);
        previousPlayerPosition = player.position;
        previousViewportPosition = viewportPosition;
    }

    private void UpdateParticleMotion()
    {
        float deltaTime = Application.isPlaying ? Time.deltaTime : 1f / 60f;
        currentRate = Mathf.SmoothDamp(currentRate, Mathf.Lerp(idleSpawnRate, maxSpawnRate, speed01), ref rateSmoothRef, responseSmoothTime, Mathf.Infinity, deltaTime);
        currentLengthScale = Mathf.SmoothDamp(currentLengthScale, Mathf.Lerp(idleLengthScale, maxLengthScale, speed01), ref lengthSmoothRef, responseSmoothTime, Mathf.Infinity, deltaTime);
        currentVelocityScale = Mathf.SmoothDamp(currentVelocityScale, Mathf.Lerp(idleVelocityScale, maxVelocityScale, speed01), ref velocityScaleSmoothRef, responseSmoothTime, Mathf.Infinity, deltaTime);

        ApplyDynamicParticleSettings();
    }

    private void InitializeRuntimeValues()
    {
        currentVelocity = new Vector3(0f, 0f, -idleFlowSpeed);
        currentRate = idleSpawnRate;
        currentLengthScale = idleLengthScale;
        currentVelocityScale = idleVelocityScale;
        emissionAccumulator = 0f;
    }

    private void ConfigureSpawnDistribution()
    {
        ParticleSystem.MainModule main = starParticleSystem.main;
        main.maxParticles = maxParticles;
        main.prewarm = false;
        main.playOnAwake = false;

        ParticleSystem.ShapeModule shape = starParticleSystem.shape;
        shape.enabled = false;

        ParticleSystem.EmissionModule emission = starParticleSystem.emission;
        emission.enabled = false;
        emission.rateOverTime = 0f;
    }
    
    private void ApplyDynamicParticleSettings()
    {
        ParticleSystem.EmissionModule emission = starParticleSystem.emission;
        emission.enabled = false;
        emission.rateOverTime = 0f;

        ParticleSystem.VelocityOverLifetimeModule velocityOverLifetime = starParticleSystem.velocityOverLifetime;
        velocityOverLifetime.x = currentVelocity.x;
        velocityOverLifetime.y = currentVelocity.y;
        velocityOverLifetime.z = currentVelocity.z;

        ParticleSystemRenderer particleRenderer = starParticleSystem.GetComponent<ParticleSystemRenderer>();
        particleRenderer.lengthScale = currentLengthScale;
        particleRenderer.velocityScale = currentVelocityScale;
    }

    private void EmitStars(float deltaTime)
    {
        if (!Application.isPlaying || deltaTime <= 0f)
        {
            return;
        }

        emissionAccumulator += currentRate * deltaTime;
        int emitCount = Mathf.Min(Mathf.FloorToInt(emissionAccumulator), 32);
        emissionAccumulator -= emitCount;

        for (int i = 0; i < emitCount; i++)
        {
            EmitStar();
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

    private void EmitStar()
    {
        Vector2 viewportPosition = GetViewportSpawnPosition();
        float depthT = Mathf.Pow(Random.value, farDepthBias);
        float depth = Mathf.Lerp(minSpawnDepth, maxSpawnDepth, depthT);
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

    private Vector2 GetViewportSpawnPosition()
    {
        Vector2 viewportPosition = Vector2.zero;
        float usableHalfWidth = 0.5f - viewportPadding;
        float usableHalfHeight = 0.5f - viewportPadding;

        for (int attempt = 0; attempt < 16; attempt++)
        {
            float x = BiasTowardEdge(Random.Range(-1f, 1f));
            float y = BiasTowardEdge(Random.Range(-1f, 1f));
            viewportPosition = new Vector2(
                0.5f + x * usableHalfWidth,
                0.5f + y * usableHalfHeight);

            float deadZoneX = centerDeadZone.x > 0f ? (viewportPosition.x - 0.5f) / centerDeadZone.x : float.PositiveInfinity;
            float deadZoneY = centerDeadZone.y > 0f ? (viewportPosition.y - 0.5f) / centerDeadZone.y : float.PositiveInfinity;
            if (deadZoneX * deadZoneX + deadZoneY * deadZoneY >= 1f)
            {
                return viewportPosition;
            }
        }

        return new Vector2(viewportPadding, Random.Range(viewportPadding, 1f - viewportPadding));
    }

    private float BiasTowardEdge(float value)
    {
        return Mathf.Sign(value) * Mathf.Pow(Mathf.Abs(value), 1f / edgeBias);
    }

    private void ResolvePlayer()
    {
        if (player == null && autoFindPlayer)
        {
            PlaneController planeController = FindFirstObjectByType<PlaneController>();
            if (planeController != null)
            {
                player = planeController.transform;
            }
        }
    }

    private void CachePlayerState()
    {
        if (player == null || targetCamera == null)
        {
            hasPreviousPlayerPosition = false;
            return;
        }

        previousPlayerPosition = player.position;
        previousViewportPosition = targetCamera.WorldToViewportPoint(player.position);
        hasPreviousPlayerPosition = true;
    }
}
