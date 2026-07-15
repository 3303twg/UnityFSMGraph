using System;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
public class ChargeState : BaseState
{
    public ChargeStyle style;
    public float chargeSpeed;
    public float duration;
    public float damage;
    public float hitRadius;
    public float zigzagAmp;
    public float zigzagFreq;
    public float curveTurnRate;

    ChargeStyle active;
    Vector3 chargeDir;
    Vector3 aimTarget;
    float timer;
    float afterimageTimer;
    float phaseTimer;
    bool hit;
    int phase; // Double/Feint steps
    Color trailColor;

    public ChargeState(EnemyController enemyController, StateMachine stateMachine, ChargeStateSo data)
        : base(enemyController, stateMachine)
    {
        style = data.style;
        chargeSpeed = data.chargeSpeed;
        duration = data.duration;
        damage = data.damage;
        hitRadius = data.hitRadius;
        zigzagAmp = data.zigzagAmp;
        zigzagFreq = data.zigzagFreq;
        curveTurnRate = data.curveTurnRate;
    }

    public override void Enter()
    {
        active = style == ChargeStyle.Random
            ? (ChargeStyle)Random.Range(1, 6)
            : style;

        timer = 0f;
        phaseTimer = 0f;
        afterimageTimer = 0f;
        hit = false;
        phase = 0;

        string label = active switch
        {
            ChargeStyle.Zigzag => "ZIGZAG CHARGE",
            ChargeStyle.Curve => "CURVE CHARGE",
            ChargeStyle.Double => "DOUBLE CHARGE",
            ChargeStyle.Feint => "FEINT CHARGE",
            _ => "CHARGE"
        };
        BossCombatHud.Instance?.SetStateLabel(label);
        Debug.Log($"[Boss] {label}");

        trailColor = active switch
        {
            ChargeStyle.Zigzag => new Color(1f, 0.85f, 0.2f),
            ChargeStyle.Curve => new Color(1f, 0.35f, 0.85f),
            ChargeStyle.Double => new Color(1f, 0.45f, 0.15f),
            ChargeStyle.Feint => new Color(0.5f, 1f, 0.9f),
            _ => new Color(1f, 0.45f, 0.15f)
        };

        BossVfx.ClearTelegraph(enemyController.transform);
        BossVfx.SpawnPulseRing(enemyController.transform.position, trailColor, 0.8f, 2.4f, 0.25f);
        BossVfx.SpawnSparkBurst(enemyController.transform.position, trailColor, 14, 8f);
        CombatCamera.Instance?.Shake(0.18f, 0.15f);

        LockAimDir();
        if (active == ChargeStyle.Feint)
        {
            // 페이크: 옆으로 살짝
            Vector3 side = new Vector3(-chargeDir.y, chargeDir.x, 0f);
            if (Random.value > 0.5f) side = -side;
            chargeDir = (chargeDir * 0.35f + side).normalized;
        }
    }

    void LockAimDir()
    {
        if (enemyController.PlayerTransform != null)
        {
            aimTarget = enemyController.PlayerTransform.position;
            chargeDir = aimTarget - enemyController.transform.position;
            chargeDir.z = 0f;
            if (chargeDir.sqrMagnitude < 0.001f)
                chargeDir = enemyController.FacingDirection2D();
            chargeDir.Normalize();
        }
        else
        {
            chargeDir = enemyController.FacingDirection2D();
            aimTarget = enemyController.transform.position + chargeDir;
        }
    }

    public override void Update()
    {
        timer += Time.deltaTime;
        phaseTimer += Time.deltaTime;
        afterimageTimer += Time.deltaTime;

        switch (active)
        {
            case ChargeStyle.Zigzag:
                UpdateZigzag();
                break;
            case ChargeStyle.Curve:
                UpdateCurve();
                break;
            case ChargeStyle.Double:
                UpdateDouble();
                break;
            case ChargeStyle.Feint:
                UpdateFeint();
                break;
            default:
                enemyController.MoveInDirection(chargeDir, chargeSpeed);
                TryHit();
                if (timer >= duration)
                    Finish();
                break;
        }

        if (afterimageTimer >= 0.04f)
        {
            afterimageTimer = 0f;
            BossVfx.SpawnAfterimage(enemyController.transform, trailColor, 0.2f);
        }
    }

    void UpdateZigzag()
    {
        Vector3 forward = chargeDir;
        Vector3 side = new Vector3(-forward.y, forward.x, 0f);
        float wiggle = Mathf.Sin(timer * zigzagFreq) * zigzagAmp;
        Vector3 move = (forward * chargeSpeed + side * wiggle).normalized;
        enemyController.MoveInDirection(move, chargeSpeed * 1.05f);
        TryHit();
        if (timer >= duration)
            Finish();
    }

    void UpdateCurve()
    {
        if (enemyController.PlayerTransform != null)
        {
            Vector3 toPlayer = enemyController.PlayerTransform.position - enemyController.transform.position;
            toPlayer.z = 0f;
            if (toPlayer.sqrMagnitude > 0.01f)
            {
                toPlayer.Normalize();
                chargeDir = Vector3.RotateTowards(
                    chargeDir, toPlayer,
                    curveTurnRate * Mathf.Deg2Rad * Time.deltaTime,
                    0f).normalized;
            }
        }
        enemyController.MoveInDirection(chargeDir, chargeSpeed);
        TryHit();
        if (timer >= duration)
            Finish();
    }

    void UpdateDouble()
    {
        float half = duration * 0.42f;
        float pause = 0.18f;
        if (phase == 0)
        {
            enemyController.MoveInDirection(chargeDir, chargeSpeed);
            TryHit();
            if (phaseTimer >= half)
            {
                phase = 1;
                phaseTimer = 0f;
                hit = false;
                BossVfx.SpawnPulseRing(enemyController.transform.position, trailColor, 0.5f, 1.8f, 0.2f);
            }
        }
        else if (phase == 1)
        {
            // 짧은 브레이크 — 재조준
            if (phaseTimer >= pause)
            {
                phase = 2;
                phaseTimer = 0f;
                LockAimDir();
                BossVfx.SpawnSparkBurst(enemyController.transform.position, trailColor, 10, 7f);
                CombatCamera.Instance?.Shake(0.2f, 0.12f);
            }
        }
        else
        {
            enemyController.MoveInDirection(chargeDir, chargeSpeed * 1.15f);
            TryHit();
            if (phaseTimer >= half)
                Finish();
        }
    }

    void UpdateFeint()
    {
        float feintTime = duration * 0.35f;
        if (phase == 0)
        {
            enemyController.MoveInDirection(chargeDir, chargeSpeed * 0.85f);
            if (phaseTimer >= feintTime)
            {
                phase = 1;
                phaseTimer = 0f;
                hit = false;
                LockAimDir();
                BossVfx.SpawnSparkBurst(enemyController.transform.position, trailColor, 12, 8f);
                BossVfx.SpawnPulseRing(enemyController.transform.position, new Color(0.4f, 1.2f, 1.4f), 0.6f, 2f, 0.22f);
                CombatCamera.Instance?.Shake(0.22f, 0.15f);
            }
        }
        else
        {
            enemyController.MoveInDirection(chargeDir, chargeSpeed * 1.25f);
            TryHit();
            if (timer >= duration)
                Finish();
        }
    }

    void TryHit()
    {
        if (hit || enemyController.GetDistanceToPlayer() > hitRadius) return;
        enemyController.DamagePlayer(damage);
        hit = true;
        BossVfx.SpawnSparkBurst(enemyController.transform.position, Color.yellow, 14, 8f);
        BossCombatHud.Instance?.Shake(0.4f);
        CombatCamera.Instance?.Impact(0.55f, 0.3f, -0.7f, 0.18f);
        CombatCamera.Instance?.Kick(chargeDir, 0.55f);
        Debug.Log("[Boss] Charge Hit");
    }

    void Finish()
    {
        enemyController.Navigator.GoToNextNode();
    }

    public override void Exit()
    {
        BossVfx.ClearTelegraph(enemyController.transform);
    }
}
