using System;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
public class BarrageState : BaseState
{
    public BarragePattern pattern;
    public int shotCount;
    public float windup;
    public float interval;
    public float recover;
    public float damage;
    public float projectileSpeed;
    public float spreadAngle;
    public int ringCount;
    public int waveCount;

    BarragePattern active;
    float timer;
    int fired;
    int wavesDone;
    bool started;
    float spiralAng;
    Color shotColor;

    public BarrageState(EnemyController c, StateMachine s, BarrageStateSo d) : base(c, s)
    {
        pattern = d.pattern;
        shotCount = Mathf.Max(1, d.shotCount);
        windup = d.windup;
        interval = d.interval;
        recover = d.recover;
        damage = d.damage;
        projectileSpeed = d.projectileSpeed;
        spreadAngle = d.spreadAngle;
        ringCount = Mathf.Max(6, d.ringCount);
        waveCount = Mathf.Max(1, d.waveCount);
    }

    public override void Enter()
    {
        active = pattern == BarragePattern.Random
            ? (BarragePattern)Random.Range(1, 6)
            : pattern;

        timer = 0f;
        fired = 0;
        wavesDone = 0;
        started = false;
        spiralAng = Random.Range(0f, 360f);

        string label = active switch
        {
            BarragePattern.Ring => "RING BARRAGE",
            BarragePattern.Spiral => "SPIRAL BARRAGE",
            BarragePattern.Cross => "CROSS BARRAGE",
            BarragePattern.Bloom => "BLOOM BARRAGE",
            _ => "FAN BARRAGE"
        };
        BossCombatHud.Instance?.SetStateLabel(label);
        Debug.Log($"[Boss] {label}");

        shotColor = active switch
        {
            BarragePattern.Ring => new Color(1.2f, 0.4f, 2f),
            BarragePattern.Spiral => new Color(2.2f, 1.2f, 0.3f),
            BarragePattern.Cross => new Color(2f, 0.3f, 0.55f),
            BarragePattern.Bloom => new Color(1.6f, 0.55f, 1.8f),
            _ => new Color(2.4f, 0.7f, 0.15f)
        };

        enemyController.FacePlayer();
        BossVfx.AttachTelegraph(enemyController.transform, shotColor, 8f);
        BossVfx.SpawnPulseRing(enemyController.transform.position, shotColor, 0.7f, 2.4f, 0.28f);
        CombatCamera.Instance?.Shake(0.12f, 0.2f);
    }

    public override void Update()
    {
        timer += Time.deltaTime;
        enemyController.FacePlayer();

        if (!started)
        {
            if (timer < windup) return;
            started = true;
            timer = 0f;
            FireWave();
            return;
        }

        int totalWaves = active == BarragePattern.Fan ? shotCount : waveCount;
        // Fan still fires one "wave" of shots sequentially via FireWave using fan logic per interval
        if (active == BarragePattern.Fan)
        {
            if (fired < shotCount)
            {
                if (timer >= interval)
                {
                    timer = 0f;
                    FireFanShot();
                }
                return;
            }
        }
        else if (active == BarragePattern.Spiral)
        {
            int spiralShots = shotCount + ringCount;
            if (fired < spiralShots)
            {
                if (timer >= interval * 0.55f)
                {
                    timer = 0f;
                    FireSpiralShot();
                }
                return;
            }
        }
        else
        {
            if (wavesDone < totalWaves)
            {
                if (timer >= interval * 1.6f)
                {
                    timer = 0f;
                    FireWave();
                }
                return;
            }
        }

        if (timer >= recover)
            enemyController.Navigator.GoToNextNode();
    }

    void FireWave()
    {
        wavesDone++;
        switch (active)
        {
            case BarragePattern.Ring:
                FireRing(ringCount, 0f);
                BossVfx.SpawnPulseRing(enemyController.transform.position, shotColor, 0.5f, 2.8f, 0.22f);
                break;
            case BarragePattern.Cross:
                FireCross();
                break;
            case BarragePattern.Bloom:
                FireRing(ringCount, wavesDone * 12f);
                FireFanBurst(5, spreadAngle + 10f);
                BossVfx.SpawnSparkBurst(enemyController.transform.position, shotColor, 12, 6f);
                break;
            case BarragePattern.Spiral:
                FireSpiralShot();
                break;
            default:
                FireFanShot();
                break;
        }
        CombatCamera.Instance?.Shake(0.08f, 0.1f, 30f);
    }

    void FireFanShot()
    {
        float t = shotCount == 1 ? 0.5f : fired / (float)(shotCount - 1);
        float ang = Mathf.Lerp(-spreadAngle, spreadAngle, t);
        // 약간의 파동으로 지그재그 팬
        ang += Mathf.Sin(fired * 1.2f) * 6f;
        SpawnShot(RotateTowardPlayer(ang), 0.3f, shotColor);
        fired++;
        BossVfx.SpawnPulseRing(enemyController.transform.position, new Color(shotColor.r, shotColor.g, shotColor.b, 0.45f), 0.3f, 1.1f, 0.12f);
    }

    void FireFanBurst(int count, float spread)
    {
        for (int i = 0; i < count; i++)
        {
            float t = count == 1 ? 0.5f : i / (float)(count - 1);
            float ang = Mathf.Lerp(-spread, spread, t);
            SpawnShot(RotateTowardPlayer(ang), 0.26f, shotColor);
        }
    }

    void FireRing(int count, float offsetDeg)
    {
        Vector3 origin = enemyController.transform.position;
        for (int i = 0; i < count; i++)
        {
            float ang = offsetDeg + (360f / count) * i;
            float rad = ang * Mathf.Deg2Rad;
            Vector3 dir = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f);
            SpawnShot(dir, 0.28f, Color.Lerp(shotColor, new Color(2f, 1.2f, 0.4f), i / (float)count));
        }
        fired += count;
    }

    void FireSpiralShot()
    {
        float rad = spiralAng * Mathf.Deg2Rad;
        Vector3 dir = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f);
        // 플레이어 쪽으로도 살짝 편향
        if (enemyController.PlayerTransform != null)
        {
            Vector3 toP = (enemyController.PlayerTransform.position - enemyController.transform.position);
            toP.z = 0f;
            if (toP.sqrMagnitude > 0.01f)
                dir = (dir + toP.normalized * 0.35f).normalized;
        }
        SpawnShot(dir, 0.26f + (fired % 3) * 0.04f, shotColor);
        spiralAng += 28f + Random.Range(-4f, 8f);
        fired++;
        if (fired % 4 == 0)
            BossVfx.SpawnPulseRing(enemyController.transform.position, shotColor, 0.4f, 1.6f, 0.15f);
    }

    void FireCross()
    {
        Vector3 toPlayer = GetPlayerDir();
        Vector3 perp = new Vector3(-toPlayer.y, toPlayer.x, 0f);
        float[] offsets = { -spreadAngle, -spreadAngle * 0.4f, 0f, spreadAngle * 0.4f, spreadAngle };
        foreach (float o in offsets)
        {
            SpawnShot(Quaternion.Euler(0f, 0f, o) * toPlayer, 0.28f, shotColor);
            SpawnShot(Quaternion.Euler(0f, 0f, o) * perp, 0.24f, Color.Lerp(shotColor, Color.white, 0.25f));
            SpawnShot(Quaternion.Euler(0f, 0f, o) * -perp, 0.24f, Color.Lerp(shotColor, Color.white, 0.25f));
        }
        fired += offsets.Length * 3;
        BossVfx.SpawnSparkBurst(enemyController.transform.position, shotColor, 16, 7f);
    }

    Vector3 GetPlayerDir()
    {
        if (enemyController.PlayerTransform == null)
            return enemyController.FacingDirection2D();
        Vector3 d = enemyController.PlayerTransform.position - enemyController.transform.position;
        d.z = 0f;
        return d.sqrMagnitude < 0.001f ? enemyController.FacingDirection2D() : d.normalized;
    }

    Vector3 RotateTowardPlayer(float ang)
    {
        return Quaternion.Euler(0f, 0f, ang) * GetPlayerDir();
    }

    void SpawnShot(Vector3 dir, float scale, Color color)
    {
        if (dir.sqrMagnitude < 0.0001f) dir = Vector3.right;
        dir.Normalize();

        var go = new GameObject("BossBarrageShot");
        go.transform.position = enemyController.transform.position + dir * 0.45f;
        go.transform.localScale = Vector3.one * scale;
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = BossTestSprites.Circle;
        sr.color = color;
        sr.sortingOrder = 10;
        var col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        var rb = go.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        float spd = projectileSpeed * Random.Range(0.92f, 1.08f);
        go.AddComponent<BossProjectile>().Init(dir, spd, damage);
    }

    public override void Exit()
    {
        BossVfx.ClearTelegraph(enemyController.transform);
    }
}
