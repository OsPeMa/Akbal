using System;
using System.Collections.Generic;

public class RhythmSession
{
    public class LiveBeat
    {
        public float hitBeat;
        public RhythmRail rail;
        public bool resolved;
    }

    public RhythmPattern Pattern { get; }
    public Conductor Conductor { get; }
    public double LoopOriginBeat { get; private set; }
    public float Time { get; private set; }
    public bool Started { get; private set; }
    public bool Finished { get; private set; }
    public float LookAhead { get; set; } = 2f;
    public float PerfectWindow { get; set; } = 0.15f;
    public float GoodWindow { get; set; } = 0.35f;
    public float Bias { get; set; } = 0f;

    public int PerfectCount { get; private set; }
    public int GoodCount { get; private set; }
    public int MissCount { get; private set; }

    readonly List<LiveBeat> live = new List<LiveBeat>();
    public IReadOnlyList<LiveBeat> Live => live;
    int patternIndex;
    int loopCount;

    public RhythmSession(RhythmPattern pattern, Conductor conductor)
    {
        Pattern = pattern;
        Conductor = conductor;
    }

    public void Tick()
    {
        if (Finished) return;
        if (Pattern == null || Conductor == null || !Conductor.IsPlaying) return;

        if (!Started)
        {
            LoopOriginBeat = Math.Ceiling(Conductor.SongBeats);
            Started = true;
        }

        Time = (float)(Conductor.SongBeats - LoopOriginBeat);
        if (Time < 0f) return;

        while (patternIndex < Pattern.beats.Count
               && Pattern.beats[patternIndex].beat <= Time + LookAhead)
        {
            var b = Pattern.beats[patternIndex];
            live.Add(new LiveBeat { hitBeat = b.beat, rail = b.rail });
            patternIndex++;
        }

        for (int i = live.Count - 1; i >= 0; i--)
        {
            var b = live[i];
            if (b.resolved) { live.RemoveAt(i); continue; }
            if (Time - b.hitBeat > GoodWindow)
            {
                MissCount++;
                live.RemoveAt(i);
            }
        }

        if (Time >= Pattern.totalBeats && patternIndex >= Pattern.beats.Count && live.Count == 0)
        {
            loopCount++;
            Finished = true;
        }
    }

    public HitResult Press(RhythmRail rail)
    {
        if (!Started) return HitResult.None;
        LiveBeat best = null;
        float bestDelta = float.MaxValue;
        foreach (var b in live)
        {
            if (b.resolved || b.rail != rail) continue;
            float delta = Math.Abs(Time - (b.hitBeat + Bias));
            if (delta < bestDelta) { bestDelta = delta; best = b; }
        }
        if (best == null) return HitResult.None;
        if (bestDelta <= PerfectWindow) { PerfectCount++; best.resolved = true; return HitResult.Perfect; }
        if (bestDelta <= GoodWindow) { GoodCount++; best.resolved = true; return HitResult.Good; }
        return HitResult.None;
    }

    public void Finish()
    {
        foreach (var b in live) if (!b.resolved) MissCount++;
        live.Clear();
    }
}
