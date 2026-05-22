using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RhythmHUD : MonoBehaviour
{
    public RhythmMinigame minigame;
    public float outerRadius = 220f;
    public float innerRadius = 60f;
    public float beatSize = 28f;

    static readonly Vector2[] RailDirs =
    {
        new Vector2(0f, 1f),    // Up
        new Vector2(1f, 0f),    // Right
        new Vector2(0f, -1f),   // Down
        new Vector2(-1f, 0f),   // Left
    };
    static readonly Color[] RailColors =
    {
        new Color(0.40f, 0.85f, 1.00f),
        new Color(1.00f, 0.55f, 0.40f),
        new Color(0.55f, 1.00f, 0.55f),
        new Color(1.00f, 0.82f, 0.40f),
    };

    RectTransform root;
    Sprite circle;
    readonly List<Image> beatPool = new List<Image>();
    bool built;

    void Awake()
    {
        root = (RectTransform)transform;
        circle = UICirclePrimitive.Create(64);
        BuildStatic();
    }

    void BuildStatic()
    {
        if (built) return;
        built = true;

        MakeCircle("HitRing", innerRadius * 2f, new Color(1f, 1f, 1f, 0.45f), Vector2.zero);

        for (int i = 0; i < 4; i++)
        {
            var rail = new GameObject($"Rail_{(RhythmRail)i}", typeof(Image));
            rail.transform.SetParent(root, false);
            var rimg = rail.GetComponent<Image>();
            rimg.color = new Color(1f, 1f, 1f, 0.12f);
            var rrt = (RectTransform)rail.transform;
            rrt.sizeDelta = new Vector2(4f, outerRadius - innerRadius);
            rrt.anchoredPosition = RailDirs[i] * (innerRadius + (outerRadius - innerRadius) * 0.5f);
            rrt.localRotation = Quaternion.Euler(0f, 0f, -Vector2.SignedAngle(Vector2.up, RailDirs[i]));

            var capColor = new Color(RailColors[i].r, RailColors[i].g, RailColors[i].b, 0.35f);
            MakeCircle($"Cap_{(RhythmRail)i}", beatSize * 1.5f, capColor, RailDirs[i] * innerRadius);
        }
    }

    Image MakeCircle(string n, float diameter, Color c, Vector2 anchoredPos)
    {
        var go = new GameObject(n, typeof(Image));
        go.transform.SetParent(root, false);
        var img = go.GetComponent<Image>();
        img.sprite = circle;
        img.color = c;
        var rt = (RectTransform)go.transform;
        rt.sizeDelta = new Vector2(diameter, diameter);
        rt.anchoredPosition = anchoredPos;
        return img;
    }

    void Update()
    {
        if (minigame == null || !minigame.IsOpen || minigame.Session == null)
        {
            HideAllBeats();
            return;
        }
        var session = minigame.Session;
        int idx = 0;
        foreach (var b in session.Live)
        {
            float t = Mathf.Clamp01((b.hitBeat - session.Time) / Mathf.Max(0.0001f, session.LookAhead));
            float r = Mathf.Lerp(innerRadius, outerRadius, t);
            int railIdx = (int)b.rail;
            var view = GetOrCreateBeatView(idx);
            view.gameObject.SetActive(true);
            view.color = b.resolved ? new Color(1f, 1f, 1f, 0.25f) : RailColors[railIdx];
            ((RectTransform)view.transform).anchoredPosition = RailDirs[railIdx] * r;
            idx++;
        }
        for (int i = idx; i < beatPool.Count; i++) beatPool[i].gameObject.SetActive(false);
    }

    void HideAllBeats()
    {
        for (int i = 0; i < beatPool.Count; i++) beatPool[i].gameObject.SetActive(false);
    }

    Image GetOrCreateBeatView(int i)
    {
        while (beatPool.Count <= i)
        {
            var go = new GameObject($"Beat_{beatPool.Count}", typeof(Image));
            go.transform.SetParent(root, false);
            var img = go.GetComponent<Image>();
            img.sprite = circle;
            var rt = (RectTransform)go.transform;
            rt.sizeDelta = new Vector2(beatSize, beatSize);
            beatPool.Add(img);
        }
        return beatPool[i];
    }
}
