using UnityEngine;

/// <summary>
/// 우주 느낌 배경. 카메라 패럴랙스 + 느린 드리프트로 이동감 확보.
/// </summary>
public class SpaceBackdrop : MonoBehaviour
{
    public static SpaceBackdrop Instance { get; private set; }

    [SerializeField] int farStarCount = 90;
    [SerializeField] int midStarCount = 55;
    [SerializeField] int nearStarCount = 28;
    [SerializeField] float viewPadding = 4f;
    [SerializeField] float farParallax = 0.15f;
    [SerializeField] float midParallax = 0.4f;
    [SerializeField] float nearParallax = 0.7f;
    [SerializeField] float driftSpeed = 0.35f;

    Camera cam;
    Transform farLayer;
    Transform midLayer;
    Transform nearLayer;
    Transform nebulaLayer;
    Vector2 driftFar;
    Vector2 driftMid;
    Vector2 driftNear;
    Vector2 driftNebula;
    float wrapHalfW;
    float wrapHalfH;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Bootstrap()
    {
        if (FindObjectOfType<SpaceBackdrop>() != null) return;
        var go = new GameObject("SpaceBackdrop");
        go.AddComponent<SpaceBackdrop>();
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        cam = Camera.main;
        Build();
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    void LateUpdate()
    {
        if (cam == null) cam = Camera.main;
        if (cam == null) return;

        UpdateWrapSize();
        float dt = Time.deltaTime;
        driftFar += new Vector2(-0.2f, 0.05f) * driftSpeed * dt;
        driftMid += new Vector2(-0.55f, 0.12f) * driftSpeed * dt;
        driftNear += new Vector2(-1.1f, 0.2f) * driftSpeed * dt;
        driftNebula += new Vector2(-0.08f, 0.02f) * driftSpeed * dt;

        Vector3 c = cam.transform.position;
        PlaceLayer(nebulaLayer, c, 0.08f, driftNebula);
        PlaceLayer(farLayer, c, farParallax, driftFar);
        PlaceLayer(midLayer, c, midParallax, driftMid);
        PlaceLayer(nearLayer, c, nearParallax, driftNear);

        WrapChildren(farLayer);
        WrapChildren(midLayer);
        WrapChildren(nearLayer);
        WrapChildren(nebulaLayer);
    }

    void PlaceLayer(Transform layer, Vector3 camPos, float parallax, Vector2 drift)
    {
        if (layer == null) return;
        layer.position = new Vector3(camPos.x * parallax + drift.x, camPos.y * parallax + drift.y, 0f);
    }

    void UpdateWrapSize()
    {
        float h = cam.orthographicSize + viewPadding;
        float w = h * cam.aspect + viewPadding;
        wrapHalfW = w;
        wrapHalfH = h;
    }

    void WrapChildren(Transform layer)
    {
        if (layer == null || cam == null) return;
        Vector3 c = cam.transform.position;
        for (int i = 0; i < layer.childCount; i++)
        {
            var t = layer.GetChild(i);
            if (t.name == "DeepVeil") continue;
            Vector3 p = t.position;
            if (p.x < c.x - wrapHalfW) p.x += wrapHalfW * 2f;
            else if (p.x > c.x + wrapHalfW) p.x -= wrapHalfW * 2f;
            if (p.y < c.y - wrapHalfH) p.y += wrapHalfH * 2f;
            else if (p.y > c.y + wrapHalfH) p.y -= wrapHalfH * 2f;
            t.position = p;
        }
    }

    void Build()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
            Destroy(transform.GetChild(i).gameObject);

        if (cam == null) cam = Camera.main;
        UpdateWrapSize();

        nebulaLayer = NewLayer("Nebulae");
        farLayer = NewLayer("Stars_Far");
        midLayer = NewLayer("Stars_Mid");
        nearLayer = NewLayer("Stars_Near");

        SpawnNebulae(nebulaLayer, 7);
        SpawnStars(farLayer, farStarCount, 0.04f, 0.12f, new Color(0.55f, 0.65f, 1f, 0.55f), -40);
        SpawnStars(midLayer, midStarCount, 0.07f, 0.18f, new Color(0.75f, 0.85f, 1f, 0.85f), -30);
        SpawnStars(nearLayer, nearStarCount, 0.12f, 0.28f, new Color(1f, 0.95f, 0.85f, 1f), -20);

        var veil = new GameObject("DeepVeil");
        veil.transform.SetParent(nebulaLayer, false);
        veil.transform.localPosition = new Vector3(0f, 0f, 5f);
        veil.transform.localScale = new Vector3(80f, 80f, 1f);
        var vsr = veil.AddComponent<SpriteRenderer>();
        vsr.sprite = BossTestSprites.Circle;
        vsr.color = new Color(0.04f, 0.05f, 0.12f, 1f);
        vsr.sortingOrder = -50;
    }

    Transform NewLayer(string name)
    {
        var go = new GameObject(name);
        go.transform.SetParent(transform, false);
        go.transform.position = cam != null
            ? new Vector3(cam.transform.position.x, cam.transform.position.y, 0f)
            : Vector3.zero;
        return go.transform;
    }

    void SpawnStars(Transform parent, int count, float minScale, float maxScale, Color tint, int sorting)
    {
        Vector3 origin = cam != null ? cam.transform.position : Vector3.zero;
        for (int i = 0; i < count; i++)
        {
            var go = new GameObject("Star");
            go.transform.SetParent(parent, false);
            float x = origin.x + Random.Range(-wrapHalfW, wrapHalfW);
            float y = origin.y + Random.Range(-wrapHalfH, wrapHalfH);
            go.transform.position = new Vector3(x, y, 0f);
            float s = Random.Range(minScale, maxScale);
            go.transform.localScale = Vector3.one * s;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = BossTestSprites.Circle;
            // 가끔 색성(청/홍)
            Color c = tint;
            float roll = Random.value;
            if (roll < 0.12f) c = new Color(0.45f, 0.75f, 1.4f, tint.a);
            else if (roll < 0.2f) c = new Color(1.3f, 0.55f, 0.45f, tint.a);
            else if (roll < 0.28f) c = new Color(1.1f, 1.1f, 1.3f, tint.a);
            sr.color = c;
            sr.sortingOrder = sorting;
        }
    }

    void SpawnNebulae(Transform parent, int count)
    {
        Vector3 origin = cam != null ? cam.transform.position : Vector3.zero;
        Color[] palette =
        {
            new Color(0.25f, 0.12f, 0.55f, 0.18f),
            new Color(0.12f, 0.22f, 0.55f, 0.16f),
            new Color(0.45f, 0.1f, 0.35f, 0.14f),
            new Color(0.08f, 0.35f, 0.4f, 0.12f),
        };

        for (int i = 0; i < count; i++)
        {
            var go = new GameObject("Nebula");
            go.transform.SetParent(parent, false);
            float x = origin.x + Random.Range(-wrapHalfW * 0.9f, wrapHalfW * 0.9f);
            float y = origin.y + Random.Range(-wrapHalfH * 0.9f, wrapHalfH * 0.9f);
            go.transform.position = new Vector3(x, y, 0f);
            float sx = Random.Range(6f, 14f);
            float sy = Random.Range(4f, 10f);
            go.transform.localScale = new Vector3(sx, sy, 1f);
            go.transform.rotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = BossTestSprites.Circle;
            sr.color = palette[i % palette.Length];
            sr.sortingOrder = -45;
        }
    }
}
