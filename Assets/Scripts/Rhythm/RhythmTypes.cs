using UnityEngine.Serialization;

public enum RhythmRail { Up = 0, Right = 1, Down = 2, Left = 3 }

[System.Serializable]
public struct RhythmBeat
{
    [FormerlySerializedAs("time")]
    public float beat;
    public RhythmRail rail;
}

public enum HitResult { None, Good, Perfect }
