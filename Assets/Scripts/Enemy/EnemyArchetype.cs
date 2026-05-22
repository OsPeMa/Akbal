using UnityEngine;

[CreateAssetMenu(menuName = "Akbal/Enemy Archetype", fileName = "EnemyArchetype")]
public class EnemyArchetype : ScriptableObject
{
    [Header("Identity")]
    public string archetypeName = "Drum";

    [Header("Corruption")]
    public float maxCorruption = 5f;

    [Header("Attack")]
    public EnemyAttackPattern attackPattern;
    public float moveSpeed = 4f;
    public float attackRange = 2.2f;
    public float strikeReach = 0.5f;
    public float strikeRadius = 1.3f;
    public float strikeDamage = 1.0f;
    public float strikeDurationBeats = 0.2f;

    [Header("Pressure (parry-driven stun)")]
    public float pressureThreshold = 3f;
    public float pressurePerOnBeatParry = 2f;
    public float pressurePerOffBeatParry = 1f;

    [Header("Stun (window during which player can open ritual)")]
    public float stunDurationBeats = 4f;

    [Header("Ritual")]
    [Tooltip("Optional. If null, RhythmMinigame's default pattern is used.")]
    public RhythmPattern ritualPattern;

    [Header("Resolution")]
    [Tooltip("Brief post-ritual phase before returning to attacking.")]
    public float resolveDurationBeats = 1f;

    [Header("Visuals")]
    public Color baseColor = new Color(0.7f, 0.25f, 0.25f);
    public Color telegraphColor = new Color(1f, 0.85f, 0.2f);
    public Color vulnerableColor = new Color(0.45f, 0.75f, 1f);
}
