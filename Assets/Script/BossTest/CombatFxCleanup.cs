using UnityEngine;

/// <summary>전투 잔여 탄막/VFX를 페이드아웃 후 제거.</summary>
public static class CombatFxCleanup
{
    static readonly string[] NamePrefixes =
    {
        "BossBarrageShot",
        "HomingOrb",
        "PlayerBullet",
        "Meteor",
        "BossProjectile",
        "LaserBurn",
        "LaserWarn",
        "VacuumAura",
        "Afterimage",
        "PulseRing",
        "Spark",
        "Suck",
        "PullStreak",
        "Telegraph"
    };

    public static void FadeCombatRemnants(float duration = 0.55f)
    {
        duration = Mathf.Max(0.05f, duration);

        FadeComponents<BossProjectile>(duration);
        FadeComponents<HomingOrb>(duration);
        FadeComponents<PlayerBullet>(duration);
        FadeComponents<BossTelegraph>(duration);
        FadeComponents<BossVfxSuck>(duration);
        FadeComponents<BossVfxPullStreak>(duration);

        // 이름 기반 (레이저 화상흔, 링 등)
        var all = Object.FindObjectsOfType<Transform>();
        for (int i = 0; i < all.Length; i++)
        {
            var t = all[i];
            if (t == null) continue;
            string n = t.name;
            for (int p = 0; p < NamePrefixes.Length; p++)
            {
                if (!n.StartsWith(NamePrefixes[p])) continue;
                // 초신성 자체 연출은 건드리지 않음
                if (n.StartsWith("Nova")) break;
                BeginFade(t.gameObject, duration);
                break;
            }
        }

        // 보스 자식 레이저 라인
        foreach (var lr in Object.FindObjectsOfType<LineRenderer>())
        {
            if (lr == null) continue;
            string n = lr.gameObject.name;
            if (n.Contains("Laser"))
                BeginFade(lr.gameObject, duration);
        }
    }

    static void FadeComponents<T>(float duration) where T : Component
    {
        var list = Object.FindObjectsOfType<T>();
        for (int i = 0; i < list.Length; i++)
        {
            if (list[i] != null)
                BeginFade(list[i].gameObject, duration);
        }
    }

    public static void BeginFade(GameObject go, float duration)
    {
        if (go == null) return;
        // 초신성 연출 오브젝트는 스킵
        if (go.name.StartsWith("Nova")) return;

        // 충돌/이동 즉시 중단
        foreach (var col in go.GetComponentsInChildren<Collider2D>())
            col.enabled = false;

        var rb = go.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.simulated = false;
        }

        var proj = go.GetComponent<BossProjectile>();
        if (proj != null) proj.enabled = false;
        var homing = go.GetComponent<HomingOrb>();
        if (homing != null) homing.enabled = false;
        var bullet = go.GetComponent<PlayerBullet>();
        if (bullet != null) bullet.enabled = false;
        var spark = go.GetComponent<BossVfxSpark>();
        if (spark != null) spark.enabled = false;
        var suck = go.GetComponent<BossVfxSuck>();
        if (suck != null) suck.enabled = false;
        var streak = go.GetComponent<BossVfxPullStreak>();
        if (streak != null) streak.enabled = false;
        var tel = go.GetComponent<BossTelegraph>();
        if (tel != null) tel.enabled = false;
        var existingFade = go.GetComponent<BossVfxFade>();
        if (existingFade != null) existingFade.enabled = false;

        var fader = go.GetComponent<CombatRemnantFader>();
        if (fader == null) fader = go.AddComponent<CombatRemnantFader>();
        fader.Play(duration);
    }
}

public class CombatRemnantFader : MonoBehaviour
{
    float life;
    float maxLife;
    SpriteRenderer[] sprites;
    LineRenderer[] lines;
    Color[] spriteStarts;
    Color[] lineStarts;
    float[] lineStartWidths;

    public void Play(float duration)
    {
        maxLife = Mathf.Max(0.05f, duration);
        life = maxLife;
        sprites = GetComponentsInChildren<SpriteRenderer>(true);
        lines = GetComponentsInChildren<LineRenderer>(true);
        spriteStarts = new Color[sprites.Length];
        for (int i = 0; i < sprites.Length; i++)
            spriteStarts[i] = sprites[i] != null ? sprites[i].color : Color.white;

        lineStarts = new Color[lines.Length];
        lineStartWidths = new float[lines.Length];
        for (int i = 0; i < lines.Length; i++)
        {
            if (lines[i] == null) continue;
            lineStarts[i] = lines[i].startColor;
            lineStartWidths[i] = lines[i].startWidth;
            lines[i].enabled = true;
        }
    }

    void Update()
    {
        life -= Time.deltaTime;
        float t = 1f - Mathf.Clamp01(life / maxLife);
        float a = 1f - t;

        for (int i = 0; i < sprites.Length; i++)
        {
            var sr = sprites[i];
            if (sr == null) continue;
            var c = spriteStarts[i];
            c.a *= a;
            sr.color = c;
        }

        for (int i = 0; i < lines.Length; i++)
        {
            var lr = lines[i];
            if (lr == null) continue;
            var c = lineStarts[i];
            c.a *= a;
            lr.startColor = c;
            lr.endColor = c;
            float w = lineStartWidths[i] * a;
            lr.startWidth = w;
            lr.endWidth = w;
            if (a <= 0.02f) lr.enabled = false;
        }

        if (life <= 0f)
            Destroy(gameObject);
    }
}
