using UnityEngine;

public static class BossTestSprites
{
    static Sprite circle;
    static Sprite ring;

    public static Sprite Circle
    {
        get
        {
            if (circle != null) return circle;
            circle = BuildCircle(32, 0f);
            return circle;
        }
    }

    public static Sprite Ring
    {
        get
        {
            if (ring != null) return ring;
            ring = BuildCircle(64, 0.78f);
            return ring;
        }
    }

    static Sprite BuildCircle(int size, float innerRatio)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        float r = (size - 1) * 0.5f;
        float inner = r * innerRatio;
        Vector2 c = new Vector2(r, r);
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float d = Vector2.Distance(new Vector2(x, y), c);
            bool on = d <= r && d >= inner;
            tex.SetPixel(x, y, on ? Color.white : Color.clear);
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }
}
