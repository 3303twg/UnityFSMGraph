using System;
using UnityEngine;

[Serializable]
public class VacuumState : BaseState
{
    public float duration;
    public float pullStrength;
    public float pulseInterval;

    float timer;
    float pulse;
    float streakTimer;
    GameObject aura;

    public VacuumState(EnemyController c, StateMachine s, VacuumStateSo d) : base(c, s)
    {
        duration = d.duration;
        pullStrength = d.pullStrength;
        pulseInterval = d.pulseInterval;
    }

    public override void Enter()
    {
        timer = 0f;
        pulse = 0f;
        streakTimer = 0f;
        BossCombatHud.Instance?.SetStateLabel("VACUUM");
        BossVfx.AttachTelegraph(enemyController.transform, new Color(0.3f, 1.2f, 1.6f), 10f);
        CombatCamera.Instance?.Shake(0.1f, duration * 0.85f, 16f);
        // 특수: 살짝 당겨지는 느낌만 (거리줌은 유지)
        CombatCamera.Instance?.HoldZoomOffset(-0.35f, duration);

        EnsureAura();
        Vector3 center = enemyController.transform.position;
        BossVfx.SpawnSuctionBurst(center, new Color(0.4f, 1.3f, 1.8f, 0.9f), 22, 6f);
        BossVfx.SpawnPullStreaks(center, new Color(0.35f, 1.1f, 1.6f), 12, 5.5f);
        BossVfx.SpawnPulseRing(center, new Color(0.3f, 1f, 1.4f, 0.7f), 5f, 0.6f, 0.45f);
        Debug.Log("[Boss] Vacuum");
    }

    public override void Update()
    {
        timer += Time.deltaTime;
        pulse += Time.deltaTime;
        streakTimer += Time.deltaTime;

        Vector3 center = enemyController.transform.position;

        if (PlayerController.Instance != null)
        {
            var p = PlayerController.Instance.transform;
            Vector3 to = center - p.position;
            to.z = 0f;
            float dist = to.magnitude;
            if (dist > 0.2f)
            {
                to /= dist;
                // 거리 멀수록 살짝 더 강하게 끌어 시인성↑
                float mul = Mathf.Lerp(0.85f, 1.35f, Mathf.Clamp01(dist / 10f));
                p.position += to * pullStrength * mul * Time.deltaTime;
            }
        }

        if (aura != null)
        {
            float pulseScale = 4.2f + Mathf.Sin(Time.time * 8f) * 0.35f;
            aura.transform.localScale = Vector3.one * pulseScale;
            var sr = aura.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                var c = sr.color;
                c.a = 0.12f + Mathf.Abs(Mathf.Sin(Time.time * 6f)) * 0.12f;
                sr.color = c;
            }
        }

        // 지속적으로 바깥→안쪽 링 (흡입)
        if (pulse >= Mathf.Max(0.12f, pulseInterval * 0.55f))
        {
            pulse = 0f;
            BossVfx.SpawnPulseRing(center, new Color(0.35f, 1.2f, 1.7f, 0.65f), 4.8f, 0.5f, 0.4f);
            BossVfx.SpawnSuctionBurst(center, new Color(0.5f, 1.4f, 1.9f), 14, 5.5f);
            CombatCamera.Instance?.Kick(
                center - (PlayerController.Instance != null
                    ? PlayerController.Instance.transform.position
                    : center),
                0.08f);
        }

        if (streakTimer >= 0.28f)
        {
            streakTimer = 0f;
            BossVfx.SpawnPullStreaks(center, new Color(0.4f, 1.15f, 1.7f), 8, 5f);
        }

        if (timer >= duration)
            enemyController.Navigator.GoToNextNode();
    }

    void EnsureAura()
    {
        if (aura != null) return;
        aura = new GameObject("VacuumAura");
        aura.transform.SetParent(enemyController.transform, false);
        aura.transform.localPosition = Vector3.zero;
        aura.transform.localScale = Vector3.one * 4.2f;
        var sr = aura.AddComponent<SpriteRenderer>();
        sr.sprite = BossTestSprites.Circle;
        sr.color = new Color(0.2f, 0.7f, 1.2f, 0.18f);
        sr.sortingOrder = 3;
    }

    public override void Exit()
    {
        BossVfx.ClearTelegraph(enemyController.transform);
        CombatCamera.Instance?.ClearHoldZoom();
        if (aura != null)
        {
            UnityEngine.Object.Destroy(aura);
            aura = null;
        }
    }
}
