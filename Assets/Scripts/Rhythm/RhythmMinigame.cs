using System;
using UnityEngine;

public class RhythmMinigame : MonoBehaviour
{
    public event Action<HitResult> HitFeedback;


    [Header("Data")]
    public RhythmPattern pattern;

    [Header("Refs")]
    public RhythmInputReader input;
    public RhythmHUD hud;

    [Header("Timing (beats)")]
    public float lookAhead = 2f;
    public float perfectWindow = 0.15f;
    public float goodWindow = 0.35f;
    public float biasOffset = 0f;

    public bool IsOpen { get; private set; }
    public RhythmSession Session { get; private set; }
    public IRhythmAction CurrentAction { get; private set; }

    void Start()     { if (input != null) input.RailPressed += OnRail; }
    void OnDestroy() { if (input != null) input.RailPressed -= OnRail; }

    public void Open(IRhythmAction action)
    {
        if (IsOpen || pattern == null) return;
        var conductor = Conductor.I;
        if (conductor == null) return;
        IsOpen = true;
        CurrentAction = action;
        Session = new RhythmSession(pattern, conductor)
        {
            LookAhead = lookAhead,
            PerfectWindow = perfectWindow,
            GoodWindow = goodWindow,
            Bias = biasOffset,
        };
        if (hud != null) hud.gameObject.SetActive(true);
    }

    public void Close()
    {
        if (!IsOpen) return;
        IsOpen = false;
        if (Session != null)
        {
            Session.Finish();
            CurrentAction?.Apply(Session);
            Debug.Log($"[Rhythm] {CurrentAction?.GetType().Name ?? "None"} — Perfect {Session.PerfectCount} / Good {Session.GoodCount} / Miss {Session.MissCount}");
        }
        CurrentAction = null;
        Session = null;
        if (hud != null) hud.gameObject.SetActive(false);
    }

    void Update()
    {
        if (IsOpen && Session != null) Session.Tick();
    }

    void OnRail(RhythmRail rail)
    {
        if (!IsOpen || Session == null) return;
        var result = Session.Press(rail);
        HitFeedback?.Invoke(result);
    }
}
