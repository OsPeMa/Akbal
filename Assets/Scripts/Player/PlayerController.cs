using UnityEngine;

[RequireComponent(typeof(PlayerInputReader))]
[RequireComponent(typeof(PlayerMover))]
[RequireComponent(typeof(PlayerDash))]
[RequireComponent(typeof(PlayerParry))]
[RequireComponent(typeof(Health))]
public class PlayerController : MonoBehaviour
{
    [Header("Refs")]
    public RhythmMinigame rhythm;

    [Header("Rhythm")]
    [Range(0f, 1f)] public float rhythmSpeedMultiplier = 0.4f;
    public float purifyRange = 4f;

    PlayerInputReader input;
    PlayerMover mover;
    PlayerDash dash;
    PlayerParry parry;
    Health health;
    Transform camTf;
    Enemy ritualTarget;

    void Awake()
    {
        input = GetComponent<PlayerInputReader>();
        mover = GetComponent<PlayerMover>();
        dash = GetComponent<PlayerDash>();
        parry = GetComponent<PlayerParry>();
        health = GetComponent<Health>();
        if (Camera.main != null) camTf = Camera.main.transform;
    }

    void Start()     { if (rhythm != null) rhythm.Closed += OnRitualClosed; }
    void OnDestroy() { if (rhythm != null) rhythm.Closed -= OnRitualClosed; }

    void OnRitualClosed()
    {
        if (ritualTarget != null) ritualTarget.NotifyRitualClosed();
        ritualTarget = null;
    }

    void Update()
    {
        if (camTf == null && Camera.main != null) camTf = Camera.main.transform;

        Vector3 worldDir = CameraRelativeDirection.ToWorld(input.Move, camTf);
        float scale = (rhythm != null && rhythm.IsOpen) ? rhythmSpeedMultiplier : 1f;
        mover.DesiredVelocity = worldDir * mover.maxSpeed * scale;

        if (input.DashPressed && dash.IsReady) dash.TryStart(worldDir);
        if (input.ParryPressed && parry.IsReady) parry.TryStart();

        UpdateRhythmIntent();
    }

    void UpdateRhythmIntent()
    {
        if (rhythm == null || rhythm.IsOpen) return;

        if (input.PurifyPressed)
        {
            var target = EnemyRegistry.FindNearestVulnerable(transform.position, purifyRange);
            if (target == null) return;
            var ritualPattern = target.archetype != null ? target.archetype.ritualPattern : null;
            rhythm.Open(new PurifyAction(target, health), ritualPattern);
            if (rhythm.IsOpen)
            {
                ritualTarget = target;
                target.NotifyRitualOpened();
            }
        }
    }
}
