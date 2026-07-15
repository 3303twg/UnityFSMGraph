using UnityEngine;

public class PlayerBullet : MonoBehaviour
{
    Vector3 dir;
    float speed;
    float damage;
    float life;

    public void Init(Vector3 direction, float speed, float damage, float life = 2.2f)
    {
        dir = new Vector3(direction.x, direction.y, 0f).normalized;
        this.speed = speed;
        this.damage = damage;
        this.life = life;
        float ang = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, ang);
    }

    void Update()
    {
        transform.position += dir * speed * Time.deltaTime;
        life -= Time.deltaTime;
        if (life <= 0f) Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        var enemy = other.GetComponent<EnemyController>();
        if (enemy == null)
            enemy = other.GetComponentInParent<EnemyController>();
        if (enemy == null) return;

        enemy.TakeDamage(damage);
        BossVfx.SpawnSparkBurst(transform.position, new Color(0.4f, 0.9f, 1f), 8, 5f);
        Destroy(gameObject);
    }
}
