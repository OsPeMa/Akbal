using UnityEngine;
using UnityEngine.UI;

public class ConductorDebugHUD : MonoBehaviour
{
    public float pulseScale = 1.45f;
    public float pulseDecay = 7f;
    public Vector2 anchoredPos = new Vector2(0f, -16f);
    public Vector2 size = new Vector2(280f, 36f);

    Conductor conductor;
    Text label;
    Image pulse;
    Sprite circle;
    float pulseAmount;
    int lastDisplayedBeat = -1;

    void Awake() { Build(); }

    void Build()
    {
        var rt = (RectTransform)transform;
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = size;

        circle = UICirclePrimitive.Create(32);

        var pulseGo = new GameObject("Pulse", typeof(Image));
        pulseGo.transform.SetParent(transform, false);
        pulse = pulseGo.GetComponent<Image>();
        pulse.sprite = circle;
        pulse.color = new Color(0.5f, 0.85f, 1f, 0.85f);
        var prt = (RectTransform)pulseGo.transform;
        prt.anchorMin = prt.anchorMax = new Vector2(0f, 0.5f);
        prt.pivot = new Vector2(0.5f, 0.5f);
        prt.sizeDelta = new Vector2(22f, 22f);
        prt.anchoredPosition = new Vector2(18f, 0f);

        var labelGo = new GameObject("Label", typeof(Text));
        labelGo.transform.SetParent(transform, false);
        label = labelGo.GetComponent<Text>();
        label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        label.alignment = TextAnchor.MiddleLeft;
        label.color = Color.white;
        label.fontSize = 18;
        var lrt = (RectTransform)labelGo.transform;
        lrt.anchorMin = new Vector2(0f, 0f);
        lrt.anchorMax = new Vector2(1f, 1f);
        lrt.offsetMin = new Vector2(44f, 0f);
        lrt.offsetMax = Vector2.zero;
    }

    void Update()
    {
        if (conductor == null) conductor = Conductor.I;
        if (conductor == null) { if (label != null) label.text = "(no Conductor)"; return; }

        int beat = conductor.BeatIndex;
        if (beat > lastDisplayedBeat)
        {
            pulseAmount = 1f;
            lastDisplayedBeat = beat;
            bool downbeat = conductor.BeatInBar == 0;
            pulse.color = downbeat
                ? new Color(1f, 0.55f, 0.30f, 0.95f)
                : new Color(0.5f, 0.85f, 1f, 0.85f);
        }

        pulseAmount = Mathf.Max(0f, pulseAmount - Time.unscaledDeltaTime * pulseDecay);
        float s = 1f + (pulseScale - 1f) * pulseAmount;
        pulse.transform.localScale = new Vector3(s, s, 1f);

        if (label != null)
        {
            label.text = $"BPM {conductor.bpm:F0}   Beat {beat}   Bar {conductor.BarIndex} : {conductor.BeatInBar + 1}/{conductor.beatsPerBar}";
        }
    }
}
