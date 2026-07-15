using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
public class LaserSweepState : BaseState
{
    public float previewDuration;
    public float holdDuration;
    public float strikeDuration;
    public float recover;
    public float damage;
    public float beamLength;
    public float beamWidth;
    public float sweepAngle;
    public int strikeCount;

    float timer;
    enum Step { Preview, Hold, Strike, Recover }
    Step step;
    float startAngle;
    float curAngle;
    float lastMarkAngle;
    int strikesDone;
    bool strikeForward;
    float hitCooldown;
    LineRenderer ghostLine;
    LineRenderer hotLine;
    readonly List<GameObject> scorches = new List<GameObject>();

    static readonly Color GhostWhite = new Color(1f, 1f, 1f, 0.45f);
    static readonly Color GhostTrail = new Color(1f, 1f, 1f, 0.22f);
    static readonly Color HotCore = new Color(2.2f, 0.35f, 0.55f, 1f);
    static readonly Color HotEdge = new Color(2.5f, 1.4f, 1.6f, 0.95f);

    public LaserSweepState(EnemyController c, StateMachine s, LaserSweepStateSo d) : base(c, s)
    {
        previewDuration = Mathf.Max(0.15f, d.previewDuration);
        holdDuration = Mathf.Max(0.05f, d.holdDuration);
        strikeDuration = Mathf.Max(0.08f, d.strikeDuration);
        recover = d.recover;
        damage = d.damage;
        beamLength = d.beamLength;
        beamWidth = d.beamWidth;
        sweepAngle = d.sweepAngle;
        strikeCount = Mathf.Max(1, d.strikeCount);
    }

    public override void Enter()
    {
        timer = 0f;
        step = Step.Preview;
        strikesDone = 0;
        strikeForward = true;
        hitCooldown = 0f;
        lastMarkAngle = -999f;
        ClearScorches();

        BossCombatHud.Instance?.SetStateLabel("LASER WARN");
        enemyController.FacePlayer();
        float face = enemyController.transform.eulerAngles.z;
        startAngle = face - sweepAngle * 0.5f;
        curAngle = startAngle;

        EnsureLines();
        SetLine(ghostLine, curAngle, GhostWhite, beamWidth * 0.55f, beamWidth * 0.95f);
        if (hotLine != null) hotLine.enabled = false;

        BossVfx.AttachTelegraph(enemyController.transform, new Color(1.2f, 0.9f, 1f), 10f);
        BossVfx.SpawnPulseRing(enemyController.transform.position, new Color(1f, 1f, 1f, 0.6f), 0.6f, 2.2f, 0.3f);
        CombatCamera.Instance?.Shake(0.12f, 0.15f, 20f);
        Debug.Log("[Boss] Laser Sweep — preview");
    }

    public override void Update()
    {
        timer += Time.deltaTime;
        hitCooldown -= Time.deltaTime;

        switch (step)
        {
            case Step.Preview:
                UpdatePreview();
                break;
            case Step.Hold:
                UpdateHold();
                break;
            case Step.Strike:
                UpdateStrike();
                break;
            case Step.Recover:
                if (hotLine != null) hotLine.enabled = false;
                if (ghostLine != null)
                {
                    // 잔상 살짝 남기다 페이드
                    float a = Mathf.Lerp(0.2f, 0f, timer / Mathf.Max(0.01f, recover));
                    SetLine(ghostLine, curAngle, new Color(1f, 1f, 1f, a), beamWidth * 0.4f, beamWidth * 0.7f);
                }
                FadeScorches(1f - Mathf.Clamp01(timer / Mathf.Max(0.01f, recover)));
                if (timer >= recover)
                    enemyController.Navigator.GoToNextNode();
                break;
        }
    }

    void UpdatePreview()
    {
        float t = Mathf.Clamp01(timer / previewDuration);
        // ease-in-out으로 슥~
        float te = t * t * (3f - 2f * t);
        curAngle = Mathf.Lerp(startAngle, startAngle + sweepAngle, te);
        enemyController.transform.rotation = Quaternion.Euler(0f, 0f, curAngle);

        float pulse = 0.28f + Mathf.PingPong(timer * 3f, 0.25f);
        SetLine(ghostLine, curAngle, new Color(1f, 1f, 1f, pulse), beamWidth * 0.5f, beamWidth * 0.9f);
        if (hotLine != null) hotLine.enabled = false;

        // 지나간 자리에 허연 잔상
        if (Mathf.Abs(curAngle - lastMarkAngle) >= 6f)
        {
            lastMarkAngle = curAngle;
            DropScorch(curAngle);
        }

        if (timer >= previewDuration)
        {
            timer = 0f;
            step = Step.Hold;
            BossCombatHud.Instance?.SetStateLabel("LASER CHARGE");
            // 끝에서 전체 부채를 희미하게 깔아두기
            PaintArcGhost();
            CombatCamera.Instance?.Shake(0.18f, holdDuration * 0.8f, 24f);
        }
    }

    void UpdateHold()
    {
        // 마지막 각도에 흰 레이저 고정 + 깜빡
        float blink = 0.25f + Mathf.PingPong(timer * 8f, 0.45f);
        SetLine(ghostLine, startAngle + sweepAngle * 0.5f, new Color(1.2f, 1.1f, 1.2f, blink), beamWidth * 0.7f, beamWidth * 1.1f);
        enemyController.transform.rotation = Quaternion.Euler(0f, 0f, startAngle + sweepAngle * 0.5f);

        if (timer >= holdDuration)
        {
            timer = 0f;
            step = Step.Strike;
            strikesDone = 0;
            strikeForward = true;
            curAngle = startAngle;
            BossVfx.ClearTelegraph(enemyController.transform);
            BossCombatHud.Instance?.SetStateLabel("LASER SLASH");
            BossVfx.SpawnSparkBurst(enemyController.transform.position, HotEdge, 20, 10f);
            BossVfx.SpawnPulseRing(enemyController.transform.position, HotCore, 0.8f, 3f, 0.25f);
            CombatCamera.Instance?.Impact(0.55f, 0.28f, -0.7f, 0.2f);
            CombatCamera.Instance?.Shake(0.4f, strikeDuration * strikeCount, 40f);
            if (ghostLine != null) ghostLine.enabled = true;
            if (hotLine != null) hotLine.enabled = true;
        }
    }

    void UpdateStrike()
    {
        float t = Mathf.Clamp01(timer / strikeDuration);
        // 빠르게 툭 긁기 — ease-in 공격적
        float te = t * t;
        float from = strikeForward ? startAngle : startAngle + sweepAngle;
        float to = strikeForward ? startAngle + sweepAngle : startAngle;
        curAngle = Mathf.Lerp(from, to, te);
        enemyController.transform.rotation = Quaternion.Euler(0f, 0f, curAngle);

        SetLine(ghostLine, curAngle, new Color(1.4f, 1.2f, 1.3f, 0.55f), beamWidth * 1.1f, beamWidth * 1.6f);
        SetLine(hotLine, curAngle, HotCore, beamWidth * 0.35f, beamWidth * 0.75f);
        if (hotLine != null)
        {
            hotLine.startColor = HotEdge;
            hotLine.endColor = new Color(HotCore.r, HotCore.g, HotCore.b, 0.35f);
        }

        CheckBeamHit(curAngle, multiHit: true);

        // 스윕하면서 스파크
        if (Mathf.FloorToInt(timer * 28f) != Mathf.FloorToInt((timer - Time.deltaTime) * 28f))
        {
            Vector3 tip = AnglePoint(curAngle, beamLength * 0.65f);
            BossVfx.SpawnSparkBurst(tip, HotEdge, 4, 8f);
            DropScorch(curAngle, hot: true);
        }

        if (timer >= strikeDuration)
        {
            strikesDone++;
            if (strikesDone < strikeCount)
            {
                timer = 0f;
                strikeForward = !strikeForward;
                CombatCamera.Instance?.Shake(0.35f, 0.15f, 42f);
                BossVfx.SpawnPulseRing(enemyController.transform.position, HotCore, 0.5f, 2.2f, 0.18f);
            }
            else
            {
                timer = 0f;
                step = Step.Recover;
                if (hotLine != null) hotLine.enabled = false;
                BossVfx.SpawnSparkBurst(enemyController.transform.position, Color.white, 16, 9f);
            }
        }
    }

    void CheckBeamHit(float angleDeg, bool multiHit)
    {
        if (PlayerController.Instance == null) return;
        if (!multiHit && hitCooldown > 0f) return;
        if (multiHit && hitCooldown > 0f) return;

        Vector3 origin = enemyController.transform.position;
        float rad = angleDeg * Mathf.Deg2Rad;
        Vector3 dir = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f);
        Vector3 toPlayer = PlayerController.Instance.transform.position - origin;
        toPlayer.z = 0f;
        float proj = Vector3.Dot(toPlayer, dir);
        if (proj < 0f || proj > beamLength) return;
        float dist = Vector3.Cross(dir, toPlayer).magnitude;
        float width = step == Step.Strike ? beamWidth * 1.15f : beamWidth;
        if (dist > width) return;

        hitCooldown = multiHit ? 0.12f : 999f;
        enemyController.DamagePlayer(damage);
        BossVfx.SpawnSparkBurst(PlayerController.Instance.transform.position, HotEdge, 18, 11f);
        BossVfx.SpawnPulseRing(PlayerController.Instance.transform.position, HotCore, 0.4f, 1.6f, 0.18f);
        CombatCamera.Instance?.HitReaction(origin, 1.1f);
    }

    void DropScorch(float angleDeg, bool hot = false)
    {
        Vector3 origin = enemyController.transform.position;
        float rad = angleDeg * Mathf.Deg2Rad;
        Vector3 dir = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f);
        int dots = hot ? 2 : 3;
        for (int i = 1; i <= dots; i++)
        {
            float along = (beamLength / (dots + 1)) * i + (hot ? 0f : Random.Range(-0.3f, 0.3f));
            var go = new GameObject(hot ? "LaserBurn" : "LaserWarn");
            go.transform.position = origin + dir * along;
            float s = hot ? Random.Range(0.35f, 0.55f) : Random.Range(0.45f, 0.85f);
            go.transform.localScale = new Vector3(s * (hot ? 0.35f : 1.1f), s * 0.22f, 1f);
            go.transform.rotation = Quaternion.Euler(0f, 0f, angleDeg);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = BossTestSprites.Circle;
            sr.color = hot
                ? new Color(2f, 0.45f, 0.6f, 0.55f)
                : new Color(1f, 1f, 1f, 0.28f);
            sr.sortingOrder = hot ? 14 : 4;
            scorches.Add(go);
        }
    }

    void PaintArcGhost()
    {
        int slices = Mathf.Clamp(Mathf.RoundToInt(sweepAngle / 8f), 8, 24);
        for (int i = 0; i <= slices; i++)
        {
            float a = Mathf.Lerp(startAngle, startAngle + sweepAngle, i / (float)slices);
            DropScorch(a);
        }
    }

    void FadeScorches(float alphaMul)
    {
        for (int i = 0; i < scorches.Count; i++)
        {
            var go = scorches[i];
            if (go == null) continue;
            var sr = go.GetComponent<SpriteRenderer>();
            if (sr == null) continue;
            var c = sr.color;
            c.a *= Mathf.Clamp01(alphaMul);
            // recover 동안 점점 죽이기
            c.a = Mathf.Min(c.a, 0.3f * alphaMul);
            sr.color = c;
        }
    }

    void ClearScorches()
    {
        for (int i = 0; i < scorches.Count; i++)
        {
            if (scorches[i] != null)
                UnityEngine.Object.Destroy(scorches[i]);
        }
        scorches.Clear();
    }

    Vector3 AnglePoint(float angleDeg, float dist)
    {
        float rad = angleDeg * Mathf.Deg2Rad;
        return enemyController.transform.position + new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f) * dist;
    }

    void EnsureLines()
    {
        ghostLine = FindOrCreateLine("LaserGhostLine", 28);
        hotLine = FindOrCreateLine("LaserHotLine", 32);
    }

    LineRenderer FindOrCreateLine(string childName, int sorting)
    {
        Transform child = enemyController.transform.Find(childName);
        LineRenderer lr;
        if (child != null)
            lr = child.GetComponent<LineRenderer>();
        else
        {
            var go = new GameObject(childName);
            go.transform.SetParent(enemyController.transform, false);
            lr = go.AddComponent<LineRenderer>();
        }

        lr.positionCount = 2;
        lr.useWorldSpace = true;
        lr.numCapVertices = 4;
        lr.material = new Material(Shader.Find("Sprites/Default")
            ?? Shader.Find("Universal Render Pipeline/Unlit")
            ?? Shader.Find("Unlit/Color"));
        if (lr.material != null && lr.material.HasProperty("_Color"))
            lr.material.color = Color.white;
        lr.sortingOrder = sorting;
        lr.enabled = true;
        return lr;
    }

    void SetLine(LineRenderer lr, float angleDeg, Color color, float startW, float endW)
    {
        if (lr == null) return;
        lr.enabled = true;
        lr.startWidth = startW;
        lr.endWidth = endW;
        lr.startColor = color;
        lr.endColor = new Color(color.r, color.g, color.b, color.a * 0.15f);
        float rad = angleDeg * Mathf.Deg2Rad;
        Vector3 dir = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f);
        Vector3 origin = enemyController.transform.position;
        lr.SetPosition(0, origin);
        lr.SetPosition(1, origin + dir * beamLength);
    }

    public override void Exit()
    {
        BossVfx.ClearTelegraph(enemyController.transform);
        if (ghostLine != null) ghostLine.enabled = false;
        if (hotLine != null) hotLine.enabled = false;
        ClearScorches();
    }
}
