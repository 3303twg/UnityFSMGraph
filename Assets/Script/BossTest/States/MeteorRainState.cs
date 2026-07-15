using System;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
public class MeteorRainState : BaseState
{
    public int count;
    public float windup;
    public float interval;
    public float recover;
    public float damage;
    public float fallSpeed;
    public float spawnHeight;

    float timer;
    int spawned;
    bool raining;

    public MeteorRainState(EnemyController c, StateMachine s, MeteorRainStateSo d) : base(c, s)
    {
        count = Mathf.Max(1, d.count);
        windup = d.windup;
        interval = d.interval;
        recover = d.recover;
        damage = d.damage;
        fallSpeed = d.fallSpeed;
        spawnHeight = d.spawnHeight;
    }

    public override void Enter()
    {
        timer = 0f;
        spawned = 0;
        raining = false;
        BossCombatHud.Instance?.SetStateLabel("METEOR RAIN");
        BossVfx.AttachTelegraph(enemyController.transform, new Color(1f, 0.25f, 0.15f), 6f);
        CombatCamera.Instance?.HoldZoomOffset(1.6f, windup + count * interval + recover);
        CombatCamera.Instance?.Shake(0.15f, windup, 14f);
        Debug.Log("[Boss] Meteor Rain");
    }

    public override void Update()
    {
        timer += Time.deltaTime;
        if (!raining)
        {
            if (timer < windup) return;
            raining = true;
            timer = 0f;
            BossVfx.ClearTelegraph(enemyController.transform);
            SpawnOne();
            return;
        }

        if (spawned < count)
        {
            if (timer >= interval)
            {
                timer = 0f;
                SpawnOne();
            }
            return;
        }

        if (timer >= recover)
            enemyController.Navigator.GoToNextNode();
    }

    void SpawnOne()
    {
        Vector3 center = enemyController.PlayerTransform != null
            ? enemyController.PlayerTransform.position
            : enemyController.transform.position;

        Vector3 spawn = center + new Vector3(Random.Range(-4f, 4f), spawnHeight, 0f);
        Vector3 dir = Vector3.down;

        var go = new GameObject("Meteor");
        go.transform.position = spawn;
        go.transform.localScale = Vector3.one * 0.45f;
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = BossTestSprites.Circle;
        sr.color = new Color(2.5f, 0.55f, 0.12f);
        sr.sortingOrder = 18;
        var col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        var rb = go.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        go.AddComponent<BossProjectile>().Init(dir, fallSpeed, damage, 2.5f);
        BossVfx.SpawnPulseRing(new Vector3(spawn.x, center.y, 0f), new Color(1f, 0.3f, 0.1f, 0.5f), 0.3f, 1.2f, 0.45f);
        CombatCamera.Instance?.Shake(0.25f, 0.15f, 24f);

        spawned++;
    }

    public override void Exit()
    {
        BossVfx.ClearTelegraph(enemyController.transform);
        CombatCamera.Instance?.ClearHoldZoom();
    }
}
