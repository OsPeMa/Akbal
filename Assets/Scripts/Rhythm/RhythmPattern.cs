using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "Tokum/Rhythm Pattern", fileName = "RhythmPattern")]
public class RhythmPattern : ScriptableObject
{
    [Tooltip("Length of one pass through the pattern, in beats.")]
    [FormerlySerializedAs("totalDuration")]
    public float totalBeats = 8f;

    [Tooltip("Restart from the beginning when totalBeats elapses while the wheel is still held.")]
    public bool loop = true;

    public List<RhythmBeat> beats = new List<RhythmBeat>
    {
        new RhythmBeat { beat = 1f,   rail = RhythmRail.Up },
        new RhythmBeat { beat = 2f,   rail = RhythmRail.Right },
        new RhythmBeat { beat = 3f,   rail = RhythmRail.Down },
        new RhythmBeat { beat = 4f,   rail = RhythmRail.Left },
        new RhythmBeat { beat = 5.5f, rail = RhythmRail.Up },
        new RhythmBeat { beat = 6f,   rail = RhythmRail.Right },
        new RhythmBeat { beat = 7f,   rail = RhythmRail.Down },
    };
}
