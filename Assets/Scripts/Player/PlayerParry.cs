using UnityEngine;

public class PlayerParry : MonoBehaviour
{
    [Header("Timing")]
    public float windowDuration = 0.3f;
    public float cooldown = 0.6f;

    [Header("Beat classification")]
    [Tooltip("How close to an integer beat (in beats) counts as on-beat.")]
    public float onBeatThreshold = 0.2f;

    [Header("Visual flash")]
    public Color parryColor = new Color(0.5f, 0.85f, 1f);
    public Color onBeatParryColor = new Color(1f, 0.85f, 0.3f);

    public bool IsActive => windowTimer > 0f;
    public bool IsReady => windowTimer <= 0f && cooldownTimer <= 0f;
    public bool LastParryWasOnBeat { get; private set; }

    float windowTimer;
    float cooldownTimer;
    MeshRenderer mr;
    Color baseColor;
    bool baseCaptured;

    void Awake()
    {
        mr = GetComponentInChildren<MeshRenderer>();
    }

    public bool TryStart()
    {
        if (!IsReady) return false;
        windowTimer = windowDuration;
        cooldownTimer = cooldown;

        var c = Conductor.I;
        if (c != null && c.IsPlaying)
        {
            float sub = c.SubBeat;
            float dist = Mathf.Min(sub, 1f - sub);
            LastParryWasOnBeat = dist <= onBeatThreshold;
            Debug.Log($"[Parry] {(LastParryWasOnBeat ? "ON-BEAT" : "off-beat")}  subBeat={sub:F2}  dist={dist:F2}");
        }
        else
        {
            LastParryWasOnBeat = false;
        }
        return true;
    }

    void Update()
    {
        if (windowTimer > 0f) windowTimer -= Time.deltaTime;
        if (cooldownTimer > 0f) cooldownTimer -= Time.deltaTime;

        if (mr == null) return;
        if (!baseCaptured)
        {
            baseColor = mr.material.color;
            baseCaptured = true;
        }

        if (windowTimer > 0f)
        {
            var flash = LastParryWasOnBeat ? onBeatParryColor : parryColor;
            float t = Mathf.Clamp01(windowTimer / Mathf.Max(0.0001f, windowDuration));
            mr.material.color = Color.Lerp(baseColor, flash, t);
        }
        else
        {
            mr.material.color = baseColor;
        }
    }
}
