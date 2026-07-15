using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 보스 사망 초신성: 흡수 → 붕괴 → 폭발.
/// </summary>
public class BossDeathSupernova : MonoBehaviour
{
    public static bool IsPlaying { get; private set; }

    [SerializeField] float gatherTime = 1.35f;
    [SerializeField] float collapseTime = 0.55f;
    [SerializeField] float explodeTime = 1.1f;
    [SerializeField] float afterglowTime = 1.4f;

    EnemyController enemy;
    SpriteRenderer bossSr;
    Color originalColor;
    Vector3 originalScale;
    readonly List<GameObject> orbs = new List<GameObject>();

    public static void Play(EnemyController target)
    {
        if (target == null || IsPlaying) return;
        var fx = target.GetComponent<BossDeathSupernova>();
        if (fx == null) fx = target.gameObject.AddComponent<BossDeathSupernova>();
        fx.Begin(target);
    }

    void Begin(EnemyController target)
    {
        enemy = target;
        bossSr = target.GetComponent<SpriteRenderer>();
        if (bossSr != null) originalColor = bossSr.color;
        originalScale = target.transform.localScale;
        IsPlaying = true;
        StopAllCoroutines();
        StartCoroutine(Sequence());
    }

    IEnumerator Sequence()
    {
        Vector3 center = transform.position;
        center.z = 0f;

        BossCombatHud.Instance?.SetStateLabel("SUPERNOVA");
        CombatCamera.Instance?.ClearHoldZoom();
        CombatCamera.Instance?.HoldZoomOffset(-1.2f, gatherTime + collapseTime);
        CombatCamera.Instance?.Shake(0.25f, gatherTime * 0.9f, 22f);

        // ----- 1) 흡수: 흰빛 구슬들이 바깥에서 뭉침 -----
        SpawnGatherOrbs(center, 48, 7.5f);
        float t = 0f;
        while (t < gatherTime)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / gatherTime);
            float pulse = 1f + Mathf.Sin(t * 18f) * 0.08f * p;
            transform.localScale = originalScale * Mathf.Lerp(1f, 1.35f, p) * pulse;

            if (bossSr != null)
            {
                Color c = Color.Lerp(originalColor, new Color(2.2f, 2.2f, 2.4f), p);
                c.a = 1f;
                bossSr.color = c;
            }

            // 점점 흔들림 강화
            if (Mathf.FloorToInt(t * 8f) != Mathf.FloorToInt((t - Time.deltaTime) * 8f))
            {
                CombatCamera.Instance?.Shake(0.2f + p * 0.55f, 0.12f, 28f + p * 20f);
                BossVfx.SpawnPulseRing(center, new Color(1.4f, 1.4f, 1.6f, 0.35f + p * 0.35f),
                    0.6f + p, 2.5f + p * 3f, 0.2f);
            }

            PullOrbs(center, 4f + p * 14f);
            yield return null;
        }

        // ----- 2) 붕괴: 작아지며 하얗게 극광 -----
        CombatCamera.Instance?.HoldZoomOffset(-2.2f, collapseTime);
        CombatCamera.Instance?.Shake(0.9f, collapseTime, 50f);
        BossCombatHud.Instance?.DeathFlash(0.35f, 0.55f);
        t = 0f;
        while (t < collapseTime)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / collapseTime);
            // ease-in 붕괴
            float e = p * p;
            transform.localScale = originalScale * Mathf.Lerp(1.4f, 0.15f, e);
            if (bossSr != null)
                bossSr.color = Color.Lerp(new Color(2f, 2f, 2.2f), new Color(4f, 4f, 4.5f), e);

            PullOrbs(center, 22f);
            if (p > 0.5f && Mathf.FloorToInt(t * 20f) != Mathf.FloorToInt((t - Time.deltaTime) * 20f))
                BossVfx.SpawnSparkBurst(center, Color.white, 8, 3f + p * 4f);

            yield return null;
        }

        ConsumeOrbs();

        // ----- 3) 폭발 -----
        transform.localScale = originalScale * 0.05f;
        if (bossSr != null) bossSr.color = new Color(5f, 5f, 5.5f, 1f);

        CombatCamera.Instance?.ClearHoldZoom();
        CombatCamera.Instance?.HoldZoomOffset(3.5f, explodeTime * 0.6f);
        CombatCamera.Instance?.Shake(2.2f, explodeTime * 0.85f, 55f);
        CombatCamera.Instance?.Impact(1.8f, 0.5f, 2.5f, 0.45f);
        BossCombatHud.Instance?.DeathFlash(1.1f, 0.95f);
        BossCombatHud.Instance?.Shake(1.2f);

        // 다층 충격파
        BossVfx.SpawnPulseRing(center, new Color(2f, 2f, 2.2f, 0.95f), 0.2f, 18f, 0.7f);
        BossVfx.SpawnPulseRing(center, new Color(1.5f, 0.85f, 2f, 0.7f), 0.3f, 14f, 0.85f);
        BossVfx.SpawnPulseRing(center, new Color(2.2f, 1.6f, 0.6f, 0.65f), 0.5f, 10f, 1f);
        BossVfx.SpawnSparkBurst(center, Color.white, 48, 16f);
        BossVfx.SpawnSparkBurst(center, new Color(2f, 1.4f, 0.5f), 36, 12f);
        BossVfx.SpawnSparkBurst(center, new Color(1.2f, 0.6f, 2f), 28, 10f);
        SpawnDebrisBurst(center, 40);

        t = 0f;
        while (t < explodeTime)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / explodeTime);

            if (bossSr != null)
            {
                var c = bossSr.color;
                c.a = Mathf.Lerp(1f, 0f, p);
                bossSr.color = c;
            }

            transform.localScale = originalScale * Mathf.Lerp(0.2f, 0.01f, p);

            if (Mathf.FloorToInt(t * 6f) != Mathf.FloorToInt((t - Time.deltaTime) * 6f))
            {
                BossVfx.SpawnPulseRing(center,
                    new Color(1.5f, 1.5f, 1.7f, 0.45f * (1f - p)),
                    1f + p * 4f, 8f + p * 10f, 0.35f);
                CombatCamera.Instance?.Shake(0.6f * (1f - p * 0.5f), 0.15f, 40f);
            }

            yield return null;
        }

        // ----- 4) 잔광 -----
        CombatCamera.Instance?.ClearHoldZoom();
        CombatCamera.Instance?.Shake(0.35f, afterglowTime * 0.5f, 18f);
        BossCombatHud.Instance?.SetStateLabel("DEFEATED");
        if (bossSr != null)
        {
            var c = bossSr.color;
            c.a = 0f;
            bossSr.color = c;
            bossSr.enabled = false;
        }

        // 충돌 끄기
        foreach (var col in GetComponents<Collider2D>())
            col.enabled = false;

        t = 0f;
        while (t < afterglowTime)
        {
            t += Time.deltaTime;
            if (Mathf.FloorToInt(t * 3f) != Mathf.FloorToInt((t - Time.deltaTime) * 3f))
                BossVfx.SpawnSparkBurst(center + (Vector3)(Random.insideUnitCircle * 1.5f),
                    new Color(1.4f, 1.2f, 0.8f), 4, 3f);
            yield return null;
        }

        IsPlaying = false;
        // 객체는 남기되 비활성 (씬에 잔해)
        gameObject.SetActive(false);
    }

    void SpawnGatherOrbs(Vector3 center, int count, float radius)
    {
        ClearOrbs();
        for (int i = 0; i < count; i++)
        {
            float ang = Random.Range(0f, Mathf.PI * 2f);
            float r = Random.Range(radius * 0.45f, radius);
            Vector3 pos = center + new Vector3(Mathf.Cos(ang), Mathf.Sin(ang), 0f) * r;
            var go = new GameObject("NovaOrb");
            go.transform.position = pos;
            float s = Random.Range(0.12f, 0.35f);
            go.transform.localScale = Vector3.one * s;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = BossTestSprites.Circle;
            float hue = Random.value;
            sr.color = hue < 0.7f
                ? new Color(2f, 2f, 2.3f, 0.9f)
                : new Color(1.6f, 1.3f, 2.4f, 0.85f);
            sr.sortingOrder = 40;
            orbs.Add(go);
        }
    }

    void PullOrbs(Vector3 center, float speed)
    {
        for (int i = orbs.Count - 1; i >= 0; i--)
        {
            var go = orbs[i];
            if (go == null)
            {
                orbs.RemoveAt(i);
                continue;
            }

            Vector3 to = center - go.transform.position;
            to.z = 0f;
            float dist = to.magnitude;
            if (dist < 0.2f)
            {
                Destroy(go);
                orbs.RemoveAt(i);
                continue;
            }

            go.transform.position += to.normalized * (speed * Time.deltaTime);
            float scale = Mathf.Lerp(0.08f, go.transform.localScale.x, Mathf.Clamp01(dist / 4f));
            go.transform.localScale = Vector3.one * Mathf.Max(0.06f, scale * 0.985f);
        }
    }

    void ConsumeOrbs()
    {
        for (int i = 0; i < orbs.Count; i++)
            if (orbs[i] != null) Destroy(orbs[i]);
        orbs.Clear();
    }

    void ClearOrbs() => ConsumeOrbs();

    void SpawnDebrisBurst(Vector3 center, int count)
    {
        for (int i = 0; i < count; i++)
        {
            float ang = Random.Range(0f, Mathf.PI * 2f);
            Vector3 dir = new Vector3(Mathf.Cos(ang), Mathf.Sin(ang), 0f);
            var go = new GameObject("NovaDebris");
            go.transform.position = center;
            go.transform.localScale = Vector3.one * Random.Range(0.15f, 0.45f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = BossTestSprites.Circle;
            sr.color = Random.value > 0.5f
                ? new Color(2.5f, 2.5f, 2.6f)
                : new Color(2.2f, 1.2f, 0.4f);
            sr.sortingOrder = 45;
            go.AddComponent<BossVfxSpark>().Init(dir, Random.Range(8f, 22f), Random.Range(0.6f, 1.4f));
        }
    }

    void OnDestroy()
    {
        ConsumeOrbs();
        if (IsPlaying && enemy == this.GetComponent<EnemyController>())
            IsPlaying = false;
    }
}
