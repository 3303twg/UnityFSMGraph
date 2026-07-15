using UnityEngine;

public class BossCombatHud : MonoBehaviour
{
    public static BossCombatHud Instance { get; private set; }

    [SerializeField] Color playerFill = new Color(0.25f, 0.85f, 1f);
    [SerializeField] Color bossFill = new Color(1f, 0.25f, 0.3f);
    [SerializeField] Color back = new Color(0f, 0f, 0f, 0.55f);

    string stateLabel = "";
    float shake;

    void Awake()
    {
        Instance = this;
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void SetStateLabel(string label) => stateLabel = label;

    public void Shake(float amount = 0.25f) => shake = Mathf.Max(shake, amount);

    void OnGUI()
    {
        float w = Screen.width;
        float pad = 24f;
        float barW = Mathf.Min(420f, w * 0.42f);
        float barH = 22f;

        // Player top-left
        DrawBar(pad, pad, barW, barH, "PLAYER", GetPlayerHp01(), playerFill);

        // Boss top-right
        DrawBar(w - pad - barW, pad, barW, barH, "BOSS", GetBossHp01(), bossFill);

        // State / help
        var style = new GUIStyle(GUI.skin.label)
        {
            fontSize = 14,
            alignment = TextAnchor.UpperCenter,
            normal = { textColor = Color.white }
        };
        string help = "WASD 이동  |  마우스 조준  |  좌클릭 사격  |  F 보스강제피해";
        GUI.Label(new Rect(0, pad + barH + 10, w, 24), help, style);
        if (!string.IsNullOrEmpty(stateLabel))
            GUI.Label(new Rect(0, pad + barH + 32, w, 24), $"Boss State: {stateLabel}", style);

        // hit flash overlay
        if (shake > 0f)
        {
            shake -= Time.deltaTime * 1.35f;
            float a = Mathf.Clamp01(shake) * 0.32f;
            GUI.color = new Color(1f, 0.12f, 0.18f, a);
            GUI.DrawTexture(new Rect(0, 0, w, Screen.height), Texture2D.whiteTexture);
            GUI.color = Color.white;
        }
    }

    void DrawBar(float x, float y, float w, float h, string title, float fill01, Color fill)
    {
        GUI.color = back;
        GUI.DrawTexture(new Rect(x, y, w, h + 18), Texture2D.whiteTexture);
        GUI.color = Color.white;

        var titleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 12,
            fontStyle = FontStyle.Bold,
            normal = { textColor = Color.white }
        };
        GUI.Label(new Rect(x + 6, y + 1, w, 16), title, titleStyle);

        float fillW = Mathf.Clamp01(fill01) * (w - 12);
        GUI.color = new Color(0.15f, 0.15f, 0.15f, 0.9f);
        GUI.DrawTexture(new Rect(x + 6, y + 16, w - 12, h - 4), Texture2D.whiteTexture);
        GUI.color = fill;
        GUI.DrawTexture(new Rect(x + 6, y + 16, fillW, h - 4), Texture2D.whiteTexture);
        GUI.color = Color.white;

        var pct = new GUIStyle(GUI.skin.label)
        {
            fontSize = 11,
            alignment = TextAnchor.MiddleRight,
            normal = { textColor = Color.white }
        };
        GUI.Label(new Rect(x, y + 14, w - 10, h), $"{Mathf.RoundToInt(fill01 * 100)}%", pct);
    }

    static float GetPlayerHp01()
    {
        if (PlayerController.Instance == null) return 0f;
        return PlayerController.Instance.Hp01;
    }

    static float GetBossHp01()
    {
        var boss = Object.FindObjectOfType<EnemyController>();
        if (boss == null || boss.enemyStat == null || boss.enemyStat.maxHp <= 0f) return 0f;
        return Mathf.Clamp01(boss.enemyStat.hp / boss.enemyStat.maxHp);
    }
}
