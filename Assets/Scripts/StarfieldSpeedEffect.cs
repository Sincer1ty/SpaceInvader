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
    private bool hasPreviousPlayerPosition;

    private void Awake()
    {
        targetCamera = GetComponent<Camera>();
    }

    private void OnEnable()
    {
        CachePlayerState();
        InitializeRuntimeValues();
        ApplyDynamicParticleSettings(currentRate);

        starParticleSystem.Play();
    }

    private void OnValidate()
    {
        referenceSpeed = Mathf.Max(0.01f, referenceSpeed);
        responseSmoothTime = Mathf.Max(0.01f, responseSmoothTime);
        maxParticles = Mathf.Max(1, maxParticles);
        minStarSize = Mathf.Max(0.001f, minStarSize);
        maxStarSize = Mathf.Max(minStarSize, maxStarSize);
        idleSpawnRate = Mathf.Max(0f, idleSpawnRate);
        maxSpawnRate = Mathf.Max(idleSpawnRate, maxSpawnRate);
        idleLengthScale = Mathf.Max(0f, idleLengthScale);
        maxLengthScale = Mathf.Max(idleLengthScale, maxLengthScale);
        idleVelocityScale = Mathf.Max(0f, idleVelocityScale);
        maxVelocityScale = Mathf.Max(idleVelocityScale, maxVelocityScale);

        ApplyDynamicParticleSettings(currentRate);
    }

    private void LateUpdate()
    {
        UpdatePlayerVelocity();
        UpdateParticleMotion();
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

        ApplyDynamicParticleSettings(currentRate);
    }

    private void InitializeRuntimeValues()
    {
        currentVelocity = new Vector3(0f, 0f, -idleFlowSpeed);
        currentRate = idleSpawnRate;
        currentLengthScale = idleLengthScale;
        currentVelocityScale = idleVelocityScale;
    }
    
    private void ApplyDynamicParticleSettings(float spawnRate)
    {
        ParticleSystem.EmissionModule emission = starParticleSystem.emission;
        // emission.enabled = true;
        emission.rateOverTime = spawnRate;

        ParticleSystem.VelocityOverLifetimeModule velocityOverLifetime = starParticleSystem.velocityOverLifetime;
        velocityOverLifetime.x = currentVelocity.x;
        velocityOverLifetime.y = currentVelocity.y;
        velocityOverLifetime.z = currentVelocity.z;

        ParticleSystemRenderer particleRenderer = starParticleSystem.GetComponent<ParticleSystemRenderer>();
        particleRenderer.lengthScale = currentLengthScale;
        particleRenderer.velocityScale = currentVelocityScale;
    }

    private void CachePlayerState()
    {
        previousPlayerPosition = player.position;
        previousViewportPosition = targetCamera.WorldToViewportPoint(player.position);
        hasPreviousPlayerPosition = true;
    }
}
