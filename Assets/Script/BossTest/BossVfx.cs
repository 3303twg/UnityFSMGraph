using UnityEngine;

/// <summary>코드 기반 간단 2D VFX (잔상/차징/링).</summary>
public static class BossVfx
{
    public static void SpawnAfterimage(Transform source, Color color, float life = 0.25f)
    {
        if (source == null) return;
        var srcSr = source.GetComponent<SpriteRenderer>();
        if (srcSr == null || srcSr.sprite == null) return;

        var go = new GameObject("Afterimage");
        go.transform.SetPositionAndRotation(source.position, source.rotation);
        go.transform.localScale = source.lossyScale;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = srcSr.sprite;
        sr.color = new Color(color.r, color.g, color.b, 0.55f);
        sr.sortingOrder = srcSr.sortingOrder - 1;

        var fade = go.AddComponent<BossVfxFade>();
        fade.Init(life, true);
    }

    public static void SpawnPulseRing(Vector3 pos, Color color, float fromScale, float toScale, float life)
    {
        var go = new GameObject("PulseRing");
        go.transform.position = pos;
        go.transform.localScale = Vector3.one * fromScale;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = BossTestSprites.Ring;
        sr.color = color;
        sr.sortingOrder = 20;

        var fade = go.AddComponent<BossVfxFade>();
        fade.Init(life, true, fromScale, toScale);
    }

    public static void SpawnSparkBurst(Vector3 pos, Color color, int count = 10, float speed = 6f)
    {
        for (int i = 0; i < count; i++)
        {
            float ang = (360f / count) * i * Mathf.Deg2Rad;
            Vector3 dir = new Vector3(Mathf.Cos(ang), Mathf.Sin(ang), 0f);
            var go = new GameObject("Spark");
            go.transform.position = pos;
            go.transform.localScale = Vector3.one * Random.Range(0.12f, 0.22f);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = BossTestSprites.Circle;
            sr.color = color;
            sr.sortingOrder = 25;

            var move = go.AddComponent<BossVfxSpark>();
            move.Init(dir, speed * Random.Range(0.7f, 1.3f), Random.Range(0.2f, 0.4f));
        }
    }

    public static BossTelegraph AttachTelegraph(Transform parent, Color color, float pulseSpeed = 6f)
    {
        var existing = parent.GetComponentInChildren<BossTelegraph>();
        if (existing != null)
        {
            existing.SetColor(color);
            return existing;
        }

        var go = new GameObject("Telegraph");
        go.transform.SetParent(parent, false);
        go.transform.localPosition = Vector3.zero;
        go.transform.localScale = Vector3.one * 1.35f;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = BossTestSprites.Ring;
        sr.color = color;
        sr.sortingOrder = 8;

        var tel = go.AddComponent<BossTelegraph>();
        tel.Init(sr, pulseSpeed);
        return tel;
    }

    public static void ClearTelegraph(Transform parent)
    {
        if (parent == null) return;
        var tel = parent.GetComponentInChildren<BossTelegraph>();
        if (tel != null)
            Object.Destroy(tel.gameObject);
    }

    public static void HitFlash(SpriteRenderer sr, Color flash, float time = 0.08f)
    {
        if (sr == null) return;
        var fx = sr.GetComponent<BossHitFlash>();
        if (fx == null) fx = sr.gameObject.AddComponent<BossHitFlash>();
        fx.Play(flash, time);
    }

    /// <summary>진공: 바깥→보스 흡입 파티클.</summary>
    public static void SpawnSuctionBurst(Vector3 center, Color color, int count = 16, float radius = 5f)
    {
        for (int i = 0; i < count; i++)
        {
            float ang = Random.Range(0f, Mathf.PI * 2f);
            float r = Random.Range(radius * 0.45f, radius);
            Vector3 spawn = center + new Vector3(Mathf.Cos(ang), Mathf.Sin(ang), 0f) * r;
            var go = new GameObject("Suck");
            go.transform.position = spawn;
            float s = Random.Range(0.1f, 0.22f);
            go.transform.localScale = Vector3.one * s;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = BossTestSprites.Circle;
            sr.color = color;
            sr.sortingOrder = 22;

            go.AddComponent<BossVfxSuck>().Init(center, Random.Range(6f, 12f), Random.Range(0.35f, 0.6f));
        }
    }

    /// <summary>보스→외곽 방향으로 긴 당김 줄무늬(시인성).</summary>
    public static void SpawnPullStreaks(Vector3 center, Color color, int count = 10, float length = 4.5f)
    {
        for (int i = 0; i < count; i++)
        {
            float ang = (360f / count) * i + Random.Range(-8f, 8f);
            float rad = ang * Mathf.Deg2Rad;
            Vector3 dir = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f);

            var go = new GameObject("PullStreak");
            go.transform.position = center + dir * (length * 0.55f);
            go.transform.rotation = Quaternion.Euler(0f, 0f, ang);
            go.transform.localScale = new Vector3(length, Random.Range(0.06f, 0.12f), 1f);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = BossTestSprites.Circle;
            sr.color = new Color(color.r, color.g, color.b, 0.55f);
            sr.sortingOrder = 12;

            go.AddComponent<BossVfxPullStreak>().Init(center, dir, length, Random.Range(0.25f, 0.4f));
        }
    }
}

public class BossVfxFade : MonoBehaviour
{
    SpriteRenderer sr;
    float life;
    float maxLife;
    float fromScale;
    float toScale;
    bool scale;

    public void Init(float life, bool fadeAlpha, float from = 1f, float to = 1f)
    {
        sr = GetComponent<SpriteRenderer>();
        this.life = life;
        maxLife = life;
        fromScale = from;
        toScale = to;
        scale = Mathf.Abs(from - to) > 0.001f;
        if (scale) transform.localScale = Vector3.one * fromScale;
    }

    void Update()
    {
        life -= Time.deltaTime;
        float t = 1f - Mathf.Clamp01(life / maxLife);
        if (sr != null)
        {
            var c = sr.color;
            c.a = Mathf.Lerp(c.a > 0.01f ? c.a : 0.7f, 0f, t);
            // keep relative: start from current a at t=0
            c.a = (1f - t) * 0.7f;
            sr.color = c;
        }
        if (scale)
            transform.localScale = Vector3.one * Mathf.Lerp(fromScale, toScale, t);

        if (life <= 0f)
            Destroy(gameObject);
    }
}

public class BossVfxSpark : MonoBehaviour
{
    Vector3 dir;
    float speed;
    float life;
    SpriteRenderer sr;

    public void Init(Vector3 dir, float speed, float life)
    {
        this.dir = dir;
        this.speed = speed;
        this.life = life;
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        transform.position += dir * speed * Time.deltaTime;
        life -= Time.deltaTime;
        if (sr != null)
        {
            var c = sr.color;
            c.a = Mathf.Clamp01(life * 3f);
            sr.color = c;
        }
        if (life <= 0f) Destroy(gameObject);
    }
}

public class BossTelegraph : MonoBehaviour
{
    SpriteRenderer sr;
    float pulseSpeed;
    Color baseColor;
    Vector3 baseScale;

    public void Init(SpriteRenderer sr, float pulseSpeed)
    {
        this.sr = sr;
        this.pulseSpeed = pulseSpeed;
        baseColor = sr.color;
        baseScale = transform.localScale;
    }

    public void SetColor(Color c)
    {
        baseColor = c;
        if (sr != null) sr.color = c;
    }

    void Update()
    {
        float s = 1f + Mathf.Sin(Time.time * pulseSpeed) * 0.12f;
        transform.localScale = baseScale * s;
        if (sr != null)
        {
            var c = baseColor;
            c.a = 0.35f + Mathf.Abs(Mathf.Sin(Time.time * pulseSpeed)) * 0.4f;
            sr.color = c;
        }
    }
}

public class BossHitFlash : MonoBehaviour
{
    SpriteRenderer sr;
    Color original;
    float left;

    public void Play(Color flash, float time)
    {
        if (sr == null) sr = GetComponent<SpriteRenderer>();
        if (sr == null) return;
        if (left <= 0f) original = sr.color;
        sr.color = flash;
        left = time;
    }

    void Update()
    {
        if (left <= 0f) return;
        left -= Time.deltaTime;
        if (left <= 0f && sr != null)
            sr.color = original;
    }
}

public class BossVfxSuck : MonoBehaviour
{
    Vector3 center;
    float speed;
    float life;
    SpriteRenderer sr;

    public void Init(Vector3 center, float speed, float life)
    {
        this.center = center;
        this.speed = speed;
        this.life = life;
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        Vector3 to = center - transform.position;
        to.z = 0f;
        float dist = to.magnitude;
        if (dist > 0.05f)
            transform.position += to.normalized * speed * Time.deltaTime;

        life -= Time.deltaTime;
        if (sr != null)
        {
            var c = sr.color;
            c.a = Mathf.Clamp01(life * 2.5f);
            sr.color = c;
            float s = Mathf.Lerp(0.05f, transform.localScale.x, Mathf.Clamp01(dist / 3f));
            transform.localScale = Vector3.one * Mathf.Max(0.04f, s);
        }

        if (life <= 0f || dist < 0.15f)
            Destroy(gameObject);
    }
}

public class BossVfxPullStreak : MonoBehaviour
{
    Vector3 center;
    Vector3 dir;
    float length;
    float life;
    float maxLife;
    SpriteRenderer sr;

    public void Init(Vector3 center, Vector3 dir, float length, float life)
    {
        this.center = center;
        this.dir = dir.normalized;
        this.length = length;
        this.life = life;
        maxLife = life;
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        life -= Time.deltaTime;
        float t = 1f - Mathf.Clamp01(life / maxLife);
        // 줄이 보스 쪽으로 빨려 들어가는 느낌
        float along = Mathf.Lerp(length * 0.7f, 0.2f, t);
        transform.position = center + dir * along;
        transform.localScale = new Vector3(Mathf.Lerp(length, length * 0.2f, t), transform.localScale.y, 1f);

        if (sr != null)
        {
            var c = sr.color;
            c.a = (1f - t) * 0.55f;
            sr.color = c;
        }

        if (life <= 0f)
            Destroy(gameObject);
    }
}
