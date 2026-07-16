using UnityEngine;

public class BossProjectile : MonoBehaviour
{
    Vector3 direction;
    float speed;
    float damage;
    float life;

    public void Init(Vector3 dir, float speed, float damage, float life = 3f)
    {
        direction = new Vector3(dir.x, dir.y, 0f).normalized;
        this.speed = speed;
        this.damage = damage;
        this.life = life;

        if (direction.sqrMagnitude > 0.01f)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }
    }

    void Update()
    {
        transform.position += direction * speed * Time.deltaTime;
        life -= Time.deltaTime;
        if (life <= 0f)
            Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (BossDeathSupernova.IsPlaying) return;

        var player = other.GetComponent<PlayerController>();
        if (player == null)
            player = other.GetComponentInParent<PlayerController>();
        if (player == null) return;

        player.TakeDamage(damage);
        Destroy(gameObject);
    }
}
