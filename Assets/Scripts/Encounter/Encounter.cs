using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Akbal/Encounter", fileName = "Encounter")]
public class Encounter : ScriptableObject
{
    public List<EnemySpawn> spawns = new List<EnemySpawn>();
}

[System.Serializable]
public struct EnemySpawn
{
    public EnemyArchetype archetype;
    [Tooltip("X,Z offset from the encounter origin (typically the player).")]
    public Vector2 positionOffset;
    [Tooltip("Phase offset (in beats) applied to this enemy's attack pattern. Used to stagger multi-enemy choreography.")]
    public float beatOffset;
}
