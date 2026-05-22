using UnityEngine;

[RequireComponent(typeof(PlayerInputReader))]
[RequireComponent(typeof(PlayerMover))]
[RequireComponent(typeof(PlayerDash))]
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
    Health health;
    Transform camTf;

    void Awake()
    {
        input = GetComponent<PlayerInputReader>();
        mover = GetComponent<PlayerMover>();
        dash = GetComponent<PlayerDash>();
        health = GetComponent<Health>();
        if (Camera.main != null) camTf = Camera.main.transform;
    }

    void Update()
    {
        if (camTf == null && Camera.main != null) camTf = Camera.main.transform;

        Vector3 worldDir = CameraRelativeDirection.ToWorld(input.Move, camTf);
        float scale = (rhythm != null && rhythm.IsOpen) ? rhythmSpeedMultiplier : 1f;
        mover.DesiredVelocity = worldDir * mover.maxSpeed * scale;

        if (input.DashPressed && dash.IsReady) dash.TryStart(worldDir);

        UpdateRhythmIntent();
    }

    void UpdateRhythmIntent()
    {
        if (rhythm == null) return;

        if (rhythm.IsOpen)
        {
            bool keepHolding = rhythm.CurrentAction is PurifyAction p
                && input.PurifyHeld
                && p.IsValid();
            if (!keepHolding) rhythm.Close();
            return;
        }

        if (input.PurifyHeld)
        {
            var target = EnemyRegistry.FindNearestVulnerable(transform.position, purifyRange);
            if (target != null) rhythm.Open(new PurifyAction(target, health));
        }
    }
}
