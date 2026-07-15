using System;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
public class BlinkStrikeState : BaseState
{
    public float hideTime;
    public float strikeDelay;
    public float recover;
    public float damage;
    public float hitRange;
    public float appearOffset;

    float timer;
    enum Step { Hide, Strike, Recover }
    Step step;
    SpriteRenderer sr;
    Color original;
    Vector3 appearPos;

    public BlinkStrikeState(EnemyController c, StateMachine s, BlinkStrikeStateSo d) : base(c, s)
    {
        hideTime = d.hideTime;
        strikeDelay = d.strikeDelay;
        recover = d.recover;
        damage = d.damage;
        hitRange = d.hitRange;
        appearOffset = d.appearOffset;
    }

    public override void Enter()
    {
        timer = 0f;
        step = Step.Hide;
        BossCombatHud.Instance?.SetStateLabel("BLINK STRIKE");
        sr = enemyController.GetComponent<SpriteRenderer>();
        if (sr != null) original = sr.color;
        BossVfx.SpawnPulseRing(enemyController.transform.position, new Color(0.6f, 0.2f, 1f), 0.8f, 2.5f, 0.3f);
        BossVfx.SpawnSparkBurst(enemyController.transform.position, new Color(0.7f, 0.3f, 1f), 16, 8f);
        CombatCamera.Instance?.PunchZoomOffset(-0.5f, hideTime * 0.5f);
        Debug.Log("[Boss] Blink Strike");
    }

    public override void Update()
    {
        timer += Time.deltaTime;
        switch (step)
        {
            case Step.Hide:
                if (sr != null)
                {
                    var c = sr.color;
                    c.a = Mathf.Lerp(1f, 0f, timer / hideTime);
                    sr.color = c;
                }
                if (timer >= hideTime)
                {
                    timer = 0f;
                    step = Step.Strike;
                    AppearNearPlayer();
                }
                break;
            case Step.Strike:
                if (sr != null)
                {
                    var c = original;
                    c.a = Mathf.Clamp01(timer / 0.08f);
                    sr.color = c;
                }
                if (timer >= strikeDelay)
                {
                    timer = 0f;
                    step = Step.Recover;
                    BossVfx.SpawnSparkBurst(enemyController.transform.position, new Color(1f, 0.4f, 1f), 18, 10f);
                    BossVfx.SpawnPulseRing(enemyController.transform.position, new Color(0.9f, 0.3f, 1f), 0.6f, 2.8f, 0.25f);
                    BossCombatHud.Instance?.Shake(0.35f);
                    CombatCamera.Instance?.Impact(0.6f, 0.28f, -0.75f, 0.18f);
                    if (enemyController.GetDistanceToPlayer() <= hitRange)
                        enemyController.DamagePlayer(damage);
                }
                break;
            case Step.Recover:
                if (timer >= recover)
                    enemyController.Navigator.GoToNextNode();
                break;
        }
    }

    void AppearNearPlayer()
    {
        if (enemyController.PlayerTransform == null) return;
        Vector3 p = enemyController.PlayerTransform.position;
        float ang = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        appearPos = p + new Vector3(Mathf.Cos(ang), Mathf.Sin(ang), 0f) * appearOffset;
        enemyController.transform.position = appearPos;
        enemyController.FacePlayer();
        BossVfx.SpawnPulseRing(appearPos, new Color(0.8f, 0.4f, 1f), 0.4f, 1.8f, 0.2f);
        CombatCamera.Instance?.Shake(0.35f, 0.18f, 40f);
        CombatCamera.Instance?.Kick((appearPos - p).normalized, 0.35f);
    }

    public override void Exit()
    {
        if (sr != null) sr.color = original;
    }
}
