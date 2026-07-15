using UnityEngine;

public class HomingOrb : MonoBehaviour
{
    float speed;
    float turn;
    float damage;
    float life;
    Transform target;

    public void Init(float speed, float turnRate, float damage, float life)
    {
        this.speed = speed;
        turn = turnRate;
        this.damage = damage;
        this.life = life;
        target = PlayerController.Instance != null ? PlayerController.Instance.Transform : null;
    }

    void Update()
    {
        life -= Time.deltaTime;
        if (life <= 0f)
        {
            Destroy(gameObject);
            return;
        }

        Vector3 dir = transform.right;
        if (target != null)
        {
            Vector3 to = target.position - transform.position;
            to.z = 0f;
            if (to.sqrMagnitude > 0.001f)
            {
                to.Normalize();
                dir = Vector3.RotateTowards(dir, to, turn * Mathf.Deg2Rad * Time.deltaTime, 0f);
            }
        }

        float ang = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, ang);
        transform.position += dir * speed * Time.deltaTime;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        var player = other.GetComponent<PlayerController>() ?? other.GetComponentInParent<PlayerController>();
        if (player == null) return;
        player.TakeDamage(damage);
        BossVfx.SpawnSparkBurst(transform.position, new Color(0.7f, 0.3f, 1f), 10, 6f);
        Destroy(gameObject);
    }
}
