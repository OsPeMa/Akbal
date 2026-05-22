using System;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class Conductor : MonoBehaviour
{
    public static Conductor I { get; private set; }

    [Header("Song")]
    public AudioClip song;
    public float bpm = 120f;
    public int beatsPerBar = 4;

    [Header("Playback")]
    [Range(0f, 1f)] public float volume = 0.3f;
    public float startDelay = 0.2f;

    [Header("Metronome fallback (used if song is null)")]
    public int metronomeBars = 8;
    public int metronomeSampleRate = 44100;

    AudioSource src;
    double dspStart;
    bool playing;
    int lastBeatBroadcast = -1;

    public event Action<int> Beat;
    public event Action<int> Bar;

    public bool IsPlaying => playing && AudioSettings.dspTime >= dspStart;
    public double SongTime => IsPlaying ? AudioSettings.dspTime - dspStart : 0.0;
    public double SecondsPerBeat => 60.0 / Mathf.Max(1f, bpm);
    public double SongBeats => SongTime / SecondsPerBeat;
    public int BeatIndex => Mathf.Max(0, (int)Math.Floor(SongBeats));
    public float SubBeat => (float)(SongBeats - Math.Floor(SongBeats));
    public int BarIndex => BeatIndex / Mathf.Max(1, beatsPerBar);
    public int BeatInBar => BeatIndex % Mathf.Max(1, beatsPerBar);

    void Awake()
    {
        if (I != null && I != this) { Destroy(gameObject); return; }
        I = this;
        src = GetComponent<AudioSource>();
        if (song == null) song = GenerateMetronomeClip();
        src.clip = song;
        src.loop = true;
        src.playOnAwake = false;
        src.volume = volume;
    }

    void Start() => Play();

    public void Play()
    {
        if (playing) return;
        dspStart = AudioSettings.dspTime + startDelay;
        src.PlayScheduled(dspStart);
        playing = true;
        lastBeatBroadcast = -1;
    }

    public void Stop()
    {
        if (!playing) return;
        src.Stop();
        playing = false;
    }

    void Update()
    {
        if (!IsPlaying) return;
        int idx = BeatIndex;
        if (idx > lastBeatBroadcast)
        {
            for (int b = lastBeatBroadcast + 1; b <= idx; b++)
            {
                Beat?.Invoke(b);
                if (b % Mathf.Max(1, beatsPerBar) == 0) Bar?.Invoke(b / beatsPerBar);
            }
            lastBeatBroadcast = idx;
        }
    }

    AudioClip GenerateMetronomeClip()
    {
        double spb = 60.0 / bpm;
        int totalBeats = Mathf.Max(1, metronomeBars * beatsPerBar);
        int totalSamples = (int)Math.Round(spb * totalBeats * metronomeSampleRate);
        var samples = new float[totalSamples];

        int clickSamples = Mathf.Min((int)(0.04f * metronomeSampleRate), totalSamples);
        for (int b = 0; b < totalBeats; b++)
        {
            int startSample = (int)Math.Round(b * spb * metronomeSampleRate);
            bool downbeat = (b % beatsPerBar) == 0;
            float freq = downbeat ? 1320f : 880f;
            float amp = downbeat ? 0.6f : 0.4f;
            for (int s = 0; s < clickSamples && startSample + s < totalSamples; s++)
            {
                float t = s / (float)metronomeSampleRate;
                float env = Mathf.Exp(-t * 80f);
                samples[startSample + s] = Mathf.Sin(2f * Mathf.PI * freq * t) * env * amp;
            }
        }

        var clip = AudioClip.Create($"Metronome_{bpm:F0}bpm", totalSamples, 1, metronomeSampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }
}
