using UnityEngine;
using UnityEngine.UI;

public class RhythmFeedbackHUD : MonoBehaviour
{
    public RhythmMinigame minigame;
    public float showDuration = 0.5f;
    public Vector2 anchoredPos = new Vector2(0f, -130f);
    public float popScale = 1.4f;

    static readonly Color PerfectColor = new Color(0.55f, 1f, 0.6f);
    static readonly Color GoodColor    = new Color(1f, 0.92f, 0.4f);
    static readonly Color MissColor    = new Color(1f, 0.45f, 0.45f);

    Text label;
    float remaining;

    void Awake()
    {
        var rt = (RectTransform)transform;
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = new Vector2(320f, 64f);

        var labelGo = new GameObject("Label", typeof(Text));
        labelGo.transform.SetParent(transform, false);
        label = labelGo.GetComponent<Text>();
        label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        label.alignment = TextAnchor.MiddleCenter;
        label.fontSize = 44;
        label.fontStyle = FontStyle.Bold;
        label.color = new Color(1f, 1f, 1f, 0f);

        var lrt = (RectTransform)labelGo.transform;
        lrt.anchorMin = Vector2.zero;
        lrt.anchorMax = Vector2.one;
        lrt.offsetMin = Vector2.zero;
        lrt.offsetMax = Vector2.zero;
    }

    void Start()     { if (minigame != null) minigame.HitFeedback += OnHit; }
    void OnDestroy() { if (minigame != null) minigame.HitFeedback -= OnHit; }

    void OnHit(HitResult result)
    {
        Color color;
        switch (result)
        {
            case HitResult.Perfect: label.text = "Perfect!"; color = PerfectColor; break;
            case HitResult.Good:    label.text = "Good!";    color = GoodColor;    break;
            default:                label.text = "Miss!";    color = MissColor;    break;
        }
        color.a = 1f;
        label.color = color;
        remaining = showDuration;
        label.transform.localScale = Vector3.one * popScale;
    }

    void Update()
    {
        if (remaining <= 0f) return;
        remaining -= Time.unscaledDeltaTime;
        float t = Mathf.Clamp01(remaining / showDuration);
        var c = label.color;
        c.a = t;
        label.color = c;
        float s = Mathf.Lerp(1f, popScale, t);
        label.transform.localScale = Vector3.one * s;
        if (remaining <= 0f) label.text = "";
    }
}
