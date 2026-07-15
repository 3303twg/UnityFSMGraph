using System;
using UnityEngine;

[Serializable]
public class HomingVolleyState : BaseState
{
    public int count;
    public float windup;
    public float interval;
    public float recover;
    public float damage;
    public float speed;
    public float turnRate;

    float timer;
    int fired;
    bool started;

    public HomingVolleyState(EnemyController c, StateMachine s, HomingVolleyStateSo d) : base(c, s)
    {
        count = Mathf.Max(1, d.count);
        windup = d.windup;
        interval = d.interval;
        recover = d.recover;
        damage = d.damage;
        speed = d.speed;
        turnRate = d.turnRate;
    }

    public override void Enter()
    {
        timer = 0f;
        fired = 0;
        started = false;
        BossCombatHud.Instance?.SetStateLabel("HOMING ORBS");
        BossVfx.AttachTelegraph(enemyController.transform, new Color(0.6f, 0.3f, 1f), 7f);
        Debug.Log("[Boss] Homing Volley");
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
            FireOne();
            return;
        }

        if (fired < count)
        {
            if (timer >= interval)
            {
                timer = 0f;
                FireOne();
            }
            return;
        }

        if (timer >= recover)
            enemyController.Navigator.GoToNextNode();
    }

    void FireOne()
    {
        float ang = (360f / count) * fired * Mathf.Deg2Rad;
        Vector3 dir = new Vector3(Mathf.Cos(ang), Mathf.Sin(ang), 0f);

        var go = new GameObject("HomingOrb");
        go.transform.position = enemyController.transform.position + dir * 0.8f;
        go.transform.localScale = Vector3.one * 0.4f;
        go.transform.rotation = Quaternion.Euler(0f, 0f, ang * Mathf.Rad2Deg);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = BossTestSprites.Circle;
        sr.color = new Color(1.6f, 0.55f, 2.2f);
        sr.sortingOrder = 16;
        var col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        var rb = go.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        go.AddComponent<HomingOrb>().Init(speed, turnRate, damage, 4f);
        BossVfx.SpawnPulseRing(go.transform.position, new Color(0.7f, 0.3f, 1f, 0.5f), 0.3f, 1f, 0.15f);
        fired++;
    }

    public override void Exit()
    {
        BossVfx.ClearTelegraph(enemyController.transform);
    }
}
