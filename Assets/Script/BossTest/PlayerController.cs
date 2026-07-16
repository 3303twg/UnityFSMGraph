using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }

    [SerializeField] float moveSpeed = 8f;
    [SerializeField] float maxHp = 100f;
    [SerializeField] float bulletDamage = 8f;
    [SerializeField] float bulletSpeed = 18f;
    [SerializeField] float fireCooldown = 0.18f;

    float fireTimer;
    Camera cam;

    public float Hp { get; private set; }
    public float MaxHp => maxHp;
    public float Hp01 => maxHp > 0f ? Hp / maxHp : 0f;
    public Transform Transform => transform;
    public bool ControlEnabled { get; private set; } = true;

    public void SetControlEnabled(bool enabled) => ControlEnabled = enabled;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        Hp = maxHp;
        cam = Camera.main;
        EnsureCombatCamera();
    }

    static void EnsureCombatCamera()
    {
        if (CombatCamera.Instance != null) return;
        var main = Camera.main;
        if (main == null) return;
        if (main.GetComponent<CombatCamera>() == null)
            main.gameObject.AddComponent<CombatCamera>();
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    void Update()
    {
        if (!ControlEnabled || Hp <= 0f) return;

        Vector3 input = new Vector3(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"), 0f);
        if (input.sqrMagnitude > 0.01f)
        {
            input.Normalize();
            transform.position += input * moveSpeed * Time.deltaTime;
        }

        Vector3 aim = GetMouseAimDir();
        if (aim.sqrMagnitude > 0.001f)
        {
            float angle = Mathf.Atan2(aim.y, aim.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }

        fireTimer -= Time.deltaTime;
        if (Input.GetMouseButton(0) && fireTimer <= 0f)
        {
            fireTimer = fireCooldown;
            Fire(aim);
        }
    }

    Vector3 GetMouseAimDir()
    {
        if (cam == null) cam = Camera.main;
        if (cam == null) return transform.right;

        Vector3 mouse = Input.mousePosition;
        mouse.z = Mathf.Abs(cam.transform.position.z);
        Vector3 world = cam.ScreenToWorldPoint(mouse);
        world.z = 0f;
        Vector3 dir = world - transform.position;
        dir.z = 0f;
        return dir.sqrMagnitude > 0.0001f ? dir.normalized : transform.right;
    }

    void Fire(Vector3 dir)
    {
        if (dir.sqrMagnitude < 0.0001f) dir = transform.right;

        var go = new GameObject("PlayerBullet");
        go.transform.position = transform.position + dir * 0.6f;
        go.transform.localScale = Vector3.one * 0.28f;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = BossTestSprites.Circle;
        sr.color = new Color(0.55f, 1.4f, 1.8f); // HDR-ish cyan for bloom
        sr.sortingOrder = 15;

        var col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.5f;

        var rb = go.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        go.AddComponent<PlayerBullet>().Init(dir, bulletSpeed, bulletDamage);
        BossVfx.SpawnPulseRing(go.transform.position, new Color(0.6f, 1.4f, 2f, 0.75f), 0.25f, 0.7f, 0.15f);
    }

    public void TakeDamage(float amount)
    {
        if (!ControlEnabled || BossDeathSupernova.IsPlaying) return;

        Hp = Mathf.Max(0f, Hp - amount);
        BossCombatHud.Instance?.Shake(0.85f);
        var sr = GetComponent<SpriteRenderer>();
        BossVfx.HitFlash(sr, new Color(2.5f, 2.5f, 2.5f), 0.12f);
        Vector3 from = transform.position;
        var bossGo = GameObject.Find("Boss");
        if (bossGo != null) from = bossGo.transform.position;
        CombatCamera.Instance?.HitReaction(from, 1.25f);
        BossVfx.SpawnSparkBurst(transform.position, new Color(1.5f, 0.35f, 0.45f), 16, 9f);
        BossVfx.SpawnPulseRing(transform.position, new Color(1.4f, 0.25f, 0.35f, 0.7f), 0.4f, 1.8f, 0.2f);
        Debug.Log($"[Player] HP {Hp}");
    }
}
