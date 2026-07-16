using System;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
public class StalkState : BaseState
{
    public float duration;
    public float orbitSpeed;
    public float approachSpeed;
    public float preferDistance;
    public float forceDecideDistance;
    float timer;
    float orbitSign;
    float dashTimer;
    float dashRemain;
    Vector3 dashDir;

    public StalkState(EnemyController c, StateMachine s, StalkStateSo d) : base(c, s)
    {
        duration = d.duration;
        orbitSpeed = d.orbitSpeed;
        approachSpeed = d.approachSpeed;
        preferDistance = d.preferDistance;
        forceDecideDistance = d.forceDecideDistance;
    }

    public override void Enter()
    {
        timer = 0f;
        dashTimer = 0f;
        dashRemain = 0f;
        orbitSign = Random.value > 0.5f ? 1f : -1f;
        Debug.Log("[Boss] Stalk");
        BossCombatHud.Instance?.SetStateLabel("STALK");
    }

    public override void Update()
    {
        timer += Time.deltaTime;
        dashTimer += Time.deltaTime;

        if (dashRemain > 0f)
        {
            dashRemain -= Time.deltaTime;
            enemyController.MoveInDirection(dashDir, 13.5f);
            BossVfx.SpawnAfterimage(enemyController.transform, new Color(1.2f, 0.35f, 0.55f), 0.12f);
        }
        else
        {
            enemyController.StalkPlayer(orbitSpeed * orbitSign, approachSpeed, preferDistance);

            // 짧고 거친 대시로 stalk을 정적으로 안 보이게
            if (dashTimer >= Random.Range(0.35f, 0.55f))
            {
                dashTimer = 0f;
                BeginDash();
            }
        }

        if (timer >= duration || enemyController.GetDistanceToPlayer() <= forceDecideDistance)
            enemyController.Navigator.GoToNextNode();
    }

    void BeginDash()
    {
        if (enemyController.PlayerTransform == null) return;

        Vector3 to = enemyController.PlayerTransform.position - enemyController.transform.position;
        to.z = 0f;
        if (to.sqrMagnitude < 0.001f) return;
        to.Normalize();

        Vector3 side = new Vector3(-to.y, to.x, 0f) * orbitSign;
        float roll = Random.value;
        if (roll < 0.4f)
            dashDir = side;
        else if (roll < 0.7f)
            dashDir = (side + to).normalized;
        else
        {
            dashDir = -to; // 백스텝성 움직임
            orbitSign = -orbitSign;
        }

        dashRemain = Random.Range(0.1f, 0.18f);
        BossVfx.SpawnPulseRing(enemyController.transform.position, new Color(1.4f, 0.4f, 0.7f), 0.35f, 1.4f, 0.12f);
    }
}
