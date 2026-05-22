using UnityEngine;
using UnityEngine.UI;

public class HealthBarHUD : MonoBehaviour
{
    public Health source;
    public Vector2 anchoredPos = new Vector2(40f, -40f);
    public Vector2 size = new Vector2(360f, 32f);
    public Color fillColor = new Color(0.9f, 0.3f, 0.3f);
    public Color bgColor = new Color(0f, 0f, 0f, 0.55f);

    RectTransform fillRt;

    void Awake() { Build(); }

    void Build()
    {
        var rt = (RectTransform)transform;
        rt.anchorMin = rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot = new Vector2(0f, 1f);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = size;

        var bg = NewImage("Bg", bgColor);
        Stretch((RectTransform)bg.transform);

        var fill = NewImage("Fill", fillColor);
        fillRt = (RectTransform)fill.transform;
        fillRt.anchorMin = new Vector2(0f, 0f);
        fillRt.anchorMax = new Vector2(1f, 1f);
        fillRt.pivot = new Vector2(0f, 0.5f);
        fillRt.offsetMin = Vector2.zero;
        fillRt.offsetMax = Vector2.zero;
    }

    GameObject NewImage(string n, Color c)
    {
        var go = new GameObject(n, typeof(Image));
        go.transform.SetParent(transform, false);
        go.GetComponent<Image>().color = c;
        return go;
    }

    void Stretch(RectTransform r)
    {
        r.anchorMin = Vector2.zero;
        r.anchorMax = Vector2.one;
        r.offsetMin = Vector2.zero;
        r.offsetMax = Vector2.zero;
    }

    void Update()
    {
        if (source != null && fillRt != null)
        {
            float pct = Mathf.Clamp01(source.Fraction);
            fillRt.anchorMax = new Vector2(pct, 1f);
        }
    }
}
