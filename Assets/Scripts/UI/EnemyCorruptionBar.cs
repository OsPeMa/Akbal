using UnityEngine;
using UnityEngine.UI;

public class EnemyCorruptionBar : MonoBehaviour
{
    public Health source;
    public Vector3 worldOffset = new Vector3(0f, 1.5f, 0f);
    public Vector2 size = new Vector2(80f, 8f);
    public Color corruptColor = new Color(0.85f, 0.35f, 0.45f);
    public Color lucidColor   = new Color(0.55f, 0.85f, 1f);
    public Color bgColor      = new Color(0f, 0f, 0f, 0.6f);

    RectTransform canvasRt;
    RectTransform root;
    RectTransform fillRt;
    Image fillImg;
    Camera cam;

    void Awake()
    {
        var canvasGo = GameObject.Find("HudCanvas");
        if (canvasGo == null) return;
        canvasRt = (RectTransform)canvasGo.transform;

        var rootGo = new GameObject("EnemyCorruptionBar", typeof(RectTransform));
        rootGo.transform.SetParent(canvasGo.transform, false);
        root = (RectTransform)rootGo.transform;
        root.anchorMin = root.anchorMax = new Vector2(0.5f, 0.5f);
        root.pivot = new Vector2(0.5f, 0.5f);
        root.sizeDelta = size;

        var bgGo = new GameObject("Bg", typeof(Image));
        bgGo.transform.SetParent(root, false);
        bgGo.GetComponent<Image>().color = bgColor;
        StretchToParent((RectTransform)bgGo.transform);

        var fillGo = new GameObject("Fill", typeof(Image));
        fillGo.transform.SetParent(root, false);
        fillImg = fillGo.GetComponent<Image>();
        fillImg.color = corruptColor;
        fillRt = (RectTransform)fillGo.transform;
        fillRt.anchorMin = new Vector2(0f, 0f);
        fillRt.anchorMax = new Vector2(1f, 1f);
        fillRt.pivot = new Vector2(0f, 0.5f);
        fillRt.offsetMin = Vector2.zero;
        fillRt.offsetMax = Vector2.zero;
    }

    static void StretchToParent(RectTransform r)
    {
        r.anchorMin = Vector2.zero;
        r.anchorMax = Vector2.one;
        r.offsetMin = Vector2.zero;
        r.offsetMax = Vector2.zero;
    }

    void LateUpdate()
    {
        if (root == null) return;
        if (source == null || canvasRt == null) { root.gameObject.SetActive(false); return; }
        if (cam == null) cam = Camera.main;
        if (cam == null) return;

        Vector3 worldPos = transform.position + worldOffset;
        Vector3 screenPos = cam.WorldToScreenPoint(worldPos);
        if (screenPos.z < 0f) { root.gameObject.SetActive(false); return; }

        root.gameObject.SetActive(true);
        Vector2 canvasPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRt, screenPos, null, out canvasPos);
        root.anchoredPosition = canvasPos;

        float pct = Mathf.Clamp01(source.Fraction);
        fillRt.anchorMax = new Vector2(pct, 1f);
        fillImg.color = Color.Lerp(lucidColor, corruptColor, pct);
    }

    void OnDestroy()
    {
        if (root != null) Destroy(root.gameObject);
    }
}
