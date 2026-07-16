using UnityEngine;

public partial class EnemyController
{
    public Transform PlayerTransform =>
        PlayerController.Instance != null ? PlayerController.Instance.Transform : null;

    public float MoveSpeedMul { get; set; } = 1f;
    public float DamageMul { get; set; } = 1f;

    static Vector3 Flat2D(Vector3 v) => new Vector3(v.x, v.y, 0f);

    public float GetDistanceToPlayer()
    {
        if (PlayerTransform == null) return float.MaxValue;
        return Vector3.Distance(Flat2D(transform.position), Flat2D(PlayerTransform.position));
    }

    public void SyncCombatBlackboard()
    {
        float hp = enemyStat.hp;
        float max = Mathf.Max(1f, enemyStat.maxHp);
        float ratio = hp / max;

        blackboard[BlackboardKey.CurHp] = hp;
        blackboard[BlackboardKey.DistToPlayer] = GetDistanceToPlayer();
        blackboard[BlackboardKey.DetectionDistance] = enemyStat.detectionDistance;

        // 1 = 정상, 2 = 분노 전조, 3 = 광폭
        float phase = ratio <= 0.30f ? 3f : ratio <= 0.65f ? 2f : 1f;
        blackboard[BlackboardKey.BossPhase] = phase;
        blackboard[BlackboardKey.HpRatio] = ratio * 100f; // Compare용 0~100
    }

    public void MoveTowardsPlayer(float speed)
    {
        if (PlayerTransform == null) return;

        Vector3 to = Flat2D(PlayerTransform.position - transform.position);
        if (to.sqrMagnitude < 0.0001f) return;

        to.Normalize();
        transform.position += to * speed * MoveSpeedMul * Time.deltaTime;
        FaceDirection(to);
    }

    public void MoveInDirection(Vector3 dir, float speed)
    {
        dir = Flat2D(dir);
        if (dir.sqrMagnitude < 0.0001f) return;
        dir.Normalize();
        transform.position += dir * speed * MoveSpeedMul * Time.deltaTime;
        FaceDirection(dir);
    }

    /// <summary>플레이어 주위를 돌며 접근. orbitSpeed 부호로 방향.</summary>
    public void StalkPlayer(float orbitSpeed, float approachSpeed, float preferDistance)
    {
        if (PlayerTransform == null) return;

        Vector3 to = Flat2D(PlayerTransform.position - transform.position);
        float dist = to.magnitude;
        if (dist < 0.001f) return;

        Vector3 radial = to / dist;
        Vector3 tangent = new Vector3(-radial.y, radial.x, 0f);

        float orbitAbs = Mathf.Abs(orbitSpeed);
        float sign = orbitSpeed >= 0f ? 1f : -1f;
        // 약간의 사인 웨이브로 궤도 반경을 출렁이게
        float wobble = Mathf.Sin(Time.time * 5.5f) * approachSpeed * 0.35f;

        Vector3 move = tangent * (orbitAbs * sign);
        if (dist > preferDistance)
            move += radial * (approachSpeed + wobble);
        else if (dist < preferDistance * 0.7f)
            move -= radial * (approachSpeed + Mathf.Abs(wobble));
        else
            move += radial * wobble * 0.5f;

        transform.position += move * MoveSpeedMul * Time.deltaTime;
        FaceDirection(radial);
    }

    public void FacePlayer()
    {
        if (PlayerTransform == null) return;
        Vector3 to = Flat2D(PlayerTransform.position - transform.position);
        if (to.sqrMagnitude > 0.0001f)
            FaceDirection(to.normalized);
    }

    public void FaceDirection(Vector3 dir)
    {
        dir = Flat2D(dir);
        if (dir.sqrMagnitude < 0.0001f) return;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    public Vector3 FacingDirection2D()
    {
        float rad = transform.eulerAngles.z * Mathf.Deg2Rad;
        return new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f);
    }

    public void DamagePlayer(float amount)
    {
        if (PlayerController.Instance == null) return;
        PlayerController.Instance.TakeDamage(amount * DamageMul);
    }

    public void FlashColor(Color color)
    {
        var sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.color = color;
    }
}
