using System;
using UnityEngine;

[Serializable]
public class RangedAttackState : BaseState
{
    public float windup;
    public float recover;
    public float damage;
    public float projectileSpeed;
    public GameObject projectilePrefab;

    float timer;
    bool fired;
    bool finishedWindup;

    public RangedAttackState(EnemyController enemyController, StateMachine stateMachine, RangedAttackStateSo data)
        : base(enemyController, stateMachine)
    {
        windup = data.windup;
        recover = data.recover;
        damage = data.damage;
        projectileSpeed = data.projectileSpeed;
        projectilePrefab = data.projectilePrefab;
    }

    public override void Enter()
    {
        Debug.Log("[Boss] Ranged");
        timer = 0f;
        fired = false;
        finishedWindup = false;
        enemyController.FacePlayer();
    }

    public override void Update()
    {
        timer += Time.deltaTime;
        enemyController.FacePlayer();

        if (!finishedWindup)
        {
            if (timer >= windup)
            {
                finishedWindup = true;
                timer = 0f;
                Fire();
            }
            return;
        }

        if (timer >= recover)
            enemyController.Navigator.GoToNextNode();
    }

    void Fire()
    {
        if (fired) return;
        fired = true;

        if (enemyController.PlayerTransform == null) return;

        Vector3 origin = enemyController.transform.position;
        Vector3 dir = enemyController.PlayerTransform.position - origin;
        dir.z = 0f;
        if (dir.sqrMagnitude < 0.001f)
            dir = enemyController.FacingDirection2D();
        dir.Normalize();

        GameObject go;
        if (projectilePrefab != null)
        {
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            go = UnityEngine.Object.Instantiate(projectilePrefab, origin, Quaternion.Euler(0f, 0f, angle));
        }
        else
        {
            go = new GameObject("BossProjectile");
            go.transform.position = origin;
            go.transform.localScale = Vector3.one * 0.35f;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = BossTestSprites.Circle;
            sr.color = new Color(2.2f, 1.6f, 0.35f); // hot yellow bloom
            sr.sortingOrder = 10;

            var col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.5f;

            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }

        var proj = go.GetComponent<BossProjectile>();
        if (proj == null)
            proj = go.AddComponent<BossProjectile>();
        proj.Init(dir, projectileSpeed, damage);
        Debug.Log("[Boss] Projectile Fired");
    }
}
