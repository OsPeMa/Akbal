using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Akbal/Enemy Attack Pattern", fileName = "EnemyAttackPattern")]
public class EnemyAttackPattern : ScriptableObject
{
    [Tooltip("Total length of one pattern cycle, in beats.")]
    public float totalBeats = 8f;

    [Tooltip("Attack events. beat = when the strike connects. telegraphBeats = windup duration before the strike. kind = attack family (only Melee implemented).")]
    public List<AttackEvent> attacks = new List<AttackEvent>
    {
        new AttackEvent { beat = 1f, telegraphBeats = 1f, kind = AttackKind.Melee },
        new AttackEvent { beat = 3f, telegraphBeats = 1f, kind = AttackKind.Melee },
    };

    [Tooltip("Beat at which the patterned vulnerable window starts.")]
    public float vulnerableStartBeat = 4f;

    [Tooltip("Beat at which the patterned vulnerable window ends.")]
    public float vulnerableEndBeat = 8f;
}

public enum AttackKind { Melee, Ranged, AreaOfEffect }

[System.Serializable]
public struct AttackEvent
{
    public float beat;
    public float telegraphBeats;
    public AttackKind kind;
}
