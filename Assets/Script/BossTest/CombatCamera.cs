using UnityEngine;

/// <summary>
/// 플레이어 follow + 보스 거리 기반 줌.
/// 특수 패턴은 PunchZoomOffset / HoldZoomOffset으로만 살짝 보정.
/// </summary>
[RequireComponent(typeof(Camera))]
public class CombatCamera : MonoBehaviour
{
    public static CombatCamera Instance { get; private set; }

    [Header("Follow")]
    [SerializeField] float followSmooth = 6f;
    [SerializeField] float lookAhead = 1.2f;
    [SerializeField] float lookAheadSmooth = 4f;
    [SerializeField] float bossBias = 0.18f;
    [SerializeField] float bossBiasMaxDist = 14f;
    [SerializeField] Vector3 offset = new Vector3(0f, 0f, -10f);

    [Header("Distance Zoom")]
    [SerializeField] float closeSize = 7.2f;
    [SerializeField] float farSize = 12.5f;
    [SerializeField] float closeDistance = 3.5f;
    [SerializeField] float farDistance = 16f;
    [SerializeField] float sizeSmooth = 4f;

    Camera cam;
    Transform player;
    Transform boss;

    Vector3 lookVel;
    Vector3 currentLookAhead;
    Vector3 shakeOffset;
    Vector3 kickOffset;
    float shakeTimer;
    float shakeAmp;
    float shakeFreq = 28f;
    float punchOffset;
    float punchTimer;
    float holdOffset;
    float holdTimer;
    float punchKickDecay = 8f;
    float shakeDurationMax = 0.01f;
    float rotationShake; // Z tilt in degrees
    bool bossFocusCinematic;
    float bossFocusWeight;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
        cam = GetComponent<Camera>();
        cam.orthographic = true;
        if (cam.orthographicSize < 0.1f)
            cam.orthographicSize = (closeSize + farSize) * 0.5f;
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    void LateUpdate()
    {
        ResolveTargets();
        if (player == null && boss == null) return;

        float focusSpeed = bossFocusCinematic ? 2.8f : 3.5f;
        bossFocusWeight = Mathf.MoveTowards(
            bossFocusWeight,
            bossFocusCinematic && boss != null ? 1f : 0f,
            Time.deltaTime * focusSpeed);

        Vector3 target = player != null ? player.position : boss.position;

        if (!bossFocusCinematic && player != null)
        {
            Vector3 desiredLook = Vector3.zero;
            Vector3 mouse = Input.mousePosition;
            mouse.z = Mathf.Abs(transform.position.z);
            Vector3 world = cam.ScreenToWorldPoint(mouse);
            world.z = 0f;
            Vector3 toMouse = world - player.position;
            toMouse.z = 0f;
            if (toMouse.sqrMagnitude > 0.01f)
                desiredLook = toMouse.normalized * lookAhead;

            currentLookAhead = Vector3.SmoothDamp(currentLookAhead, desiredLook, ref lookVel, 1f / Mathf.Max(0.01f, lookAheadSmooth));
            target += currentLookAhead;
        }
        else
        {
            currentLookAhead = Vector3.Lerp(currentLookAhead, Vector3.zero, 1f - Mathf.Exp(-8f * Time.deltaTime));
        }

        float bossDist = 8f;
        if (boss != null)
        {
            Vector3 anchor = player != null ? player.position : boss.position;
            Vector3 toBoss = boss.position - anchor;
            toBoss.z = 0f;
            bossDist = toBoss.magnitude;

            if (bossFocusWeight > 0.001f)
                target = Vector3.Lerp(target, boss.position, bossFocusWeight);
            else if (player != null)
            {
                float w = bossBias * (1f - Mathf.Clamp01(bossDist / bossBiasMaxDist));
                target = Vector3.Lerp(target, boss.position, w);
            }
            else
                target = boss.position;
        }

        UpdateShakeAndKick();

        float follow = bossFocusCinematic ? followSmooth * 1.35f : followSmooth;
        Vector3 desiredPos = new Vector3(target.x, target.y, 0f) + offset + shakeOffset + kickOffset;
        float t = 1f - Mathf.Exp(-follow * Time.deltaTime);
        transform.position = Vector3.Lerp(transform.position, desiredPos, t);
        transform.rotation = Quaternion.Euler(0f, 0f, rotationShake);

        float zoomT = Mathf.InverseLerp(closeDistance, farDistance, bossDist);
        float desiredSize = Mathf.Lerp(closeSize, farSize, zoomT);
        if (bossFocusCinematic)
            desiredSize = Mathf.Lerp(desiredSize, Mathf.Clamp(desiredSize * 0.92f, closeSize, farSize), bossFocusWeight);

        if (holdTimer > 0f)
        {
            holdTimer -= Time.deltaTime;
            desiredSize += holdOffset;
            if (holdTimer <= 0f) holdOffset = 0f;
        }

        if (punchTimer > 0f)
        {
            punchTimer -= Time.deltaTime;
            float life = Mathf.Clamp01(punchTimer);
            desiredSize += punchOffset * Mathf.Max(life, 0.35f);
            if (punchTimer <= 0f) punchOffset = 0f;
        }

        desiredSize = Mathf.Clamp(desiredSize, 5.5f, 16f);
        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, desiredSize, 1f - Mathf.Exp(-sizeSmooth * Time.deltaTime));
    }

    void ResolveTargets()
    {
        if (player == null && PlayerController.Instance != null)
            player = PlayerController.Instance.transform;

        if (boss == null)
        {
            var go = GameObject.Find("Boss");
            if (go != null) boss = go.transform;
        }
    }

    void UpdateShakeAndKick()
    {
        if (shakeTimer > 0f)
        {
            shakeTimer -= Time.deltaTime;
            float life = shakeDurationMax > 0.001f ? Mathf.Clamp01(shakeTimer / shakeDurationMax) : 0f;
            // 초반 세고 끝에서 훅 떨어지게
            float falloff = life * life * (3f - 2f * life); // smoothstep reverse-ish
            falloff = Mathf.Pow(life, 0.55f);

            // Perlin만 쓰면 슴슴해서 노이즈+랜덤 킥 혼합
            float ax = (Mathf.PerlinNoise(Time.time * shakeFreq, 11.7f) * 2f - 1f);
            float ay = (Mathf.PerlinNoise(22.3f, Time.time * shakeFreq) * 2f - 1f);
            ax = Mathf.Lerp(ax, Random.Range(-1f, 1f), 0.55f);
            ay = Mathf.Lerp(ay, Random.Range(-1f, 1f), 0.55f);
            shakeOffset = new Vector3(ax, ay, 0f) * shakeAmp * falloff;
            rotationShake = ax * shakeAmp * 1.8f * falloff;
        }
        else
        {
            shakeOffset = Vector3.Lerp(shakeOffset, Vector3.zero, 1f - Mathf.Exp(-22f * Time.deltaTime));
            rotationShake = Mathf.Lerp(rotationShake, 0f, 1f - Mathf.Exp(-18f * Time.deltaTime));
        }

        kickOffset = Vector3.Lerp(kickOffset, Vector3.zero, 1f - Mathf.Exp(-punchKickDecay * Time.deltaTime));
        if (shakeTimer <= 0f && kickOffset.sqrMagnitude < 0.0001f)
            punchKickDecay = 8f;
    }

    public void Shake(float amplitude, float duration = 0.25f, float frequency = 28f)
    {
        // ortho 크기에 비례해서 체감 맞춤
        float scale = cam != null ? Mathf.Clamp(cam.orthographicSize / 9f, 0.75f, 1.4f) : 1f;
        float amp = amplitude * scale;
        shakeAmp = Mathf.Max(amp, shakeTimer > 0f ? shakeAmp * 0.45f + amp * 0.7f : amp);
        shakeTimer = Mathf.Max(shakeTimer, duration);
        shakeDurationMax = Mathf.Max(shakeDurationMax, duration);
        if (shakeTimer <= duration) shakeDurationMax = duration;
        shakeFreq = frequency;
    }

    public void Kick(Vector3 worldDir, float strength = 0.4f)
    {
        worldDir.z = 0f;
        if (worldDir.sqrMagnitude < 0.0001f) return;
        float scale = cam != null ? Mathf.Clamp(cam.orthographicSize / 9f, 0.75f, 1.4f) : 1f;
        kickOffset += worldDir.normalized * (strength * scale);
    }

    /// <summary>거리 줌 위에 가산. 음수=줌인, 양수=줌아웃.</summary>
    public void PunchZoomOffset(float deltaSize, float duration = 0.3f)
    {
        punchOffset = deltaSize;
        punchTimer = duration;
    }

    public void HoldZoomOffset(float deltaSize, float duration = 999f)
    {
        holdOffset = deltaSize;
        holdTimer = duration;
    }

    // 구 API no-op (절대 줌값 쓰지 않음 — 거리 줌 사용)
    public void HoldZoom(float _ignoredAbsolute, float duration = 999f) { }
    public void PunchZoom(float _ignoredAbsolute, float duration = 0.35f) { }
    public void ClearHoldZoom()
    {
        holdTimer = 0f;
        holdOffset = 0f;
    }

    public void Impact(float shakeAmplitude = 0.35f, float shakeDur = 0.22f, float zoomDelta = 0f, float zoomDur = 0.2f)
    {
        Shake(shakeAmplitude, shakeDur);
        if (Mathf.Abs(zoomDelta) > 0.01f)
            PunchZoomOffset(zoomDelta, zoomDur);
    }

    /// <summary>사망 연출 등: 카메라를 보스로 고정.</summary>
    public void SetBossFocus(bool enabled, Transform bossOverride = null)
    {
        bossFocusCinematic = enabled;
        if (bossOverride != null)
            boss = bossOverride;
        else if (enabled)
            ResolveTargets();
    }

    public void HitReaction(Vector3 fromWorld, float strength = 1f)
    {
        // 피격은 확실히 때리게
        float amp = Mathf.Max(1.15f, strength * 1.35f);
        Shake(amp, 0.42f, 48f);
        punchKickDecay = 5.5f;

        Vector3 dir = Random.insideUnitCircle;
        if (player != null)
        {
            Vector3 from = fromWorld;
            dir = player.position - from;
            dir.z = 0f;
        }
        if (dir.sqrMagnitude < 0.001f) dir = Random.insideUnitCircle;
        Kick(dir.normalized, amp * 0.95f);
        PunchZoomOffset(-0.95f, 0.22f);

        // 잔여 트래마 한 번 더
        shakeAmp = Mathf.Max(shakeAmp, amp);
        shakeTimer = Mathf.Max(shakeTimer, 0.42f);
        shakeDurationMax = 0.42f;
    }
}
