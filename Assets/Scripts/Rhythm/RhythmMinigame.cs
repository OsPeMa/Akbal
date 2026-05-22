using System;
using UnityEngine;

public class RhythmMinigame : MonoBehaviour
{
    public event Action<HitResult> HitFeedback;
    public event Action Closed;


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

    public void Open(IRhythmAction action, RhythmPattern overridePattern = null)
    {
        if (IsOpen) return;
        var conductor = Conductor.I;
        if (conductor == null) return;
        var usePattern = overridePattern != null ? overridePattern : pattern;
        if (usePattern == null) return;
        IsOpen = true;
        CurrentAction = action;
        Session = new RhythmSession(usePattern, conductor)
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
        Closed?.Invoke();
    }

    void Update()
    {
        if (!IsOpen || Session == null) return;
        Session.Tick();
        if (Session.Finished) { Close(); return; }
        if (CurrentAction != null && !CurrentAction.IsValid()) Close();
    }

    void OnRail(RhythmRail rail)
    {
        if (!IsOpen || Session == null) return;
        var result = Session.Press(rail);
        HitFeedback?.Invoke(result);
        CurrentAction?.OnHit(result);
    }
}
