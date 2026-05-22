using UnityEngine;

[RequireComponent(typeof(PlayerMover))]
public class PlayerDash : MonoBehaviour
{
    public float speed = 22f;
    public float duration = 0.16f;
    public float cooldown = 0.45f;

    PlayerMover mover;
    float dashTimer;
    float cooldownTimer;
    Vector3 dashDir;

    public bool IsDashing => dashTimer > 0f;
    public bool IsReady => dashTimer <= 0f && cooldownTimer <= 0f;

    void Awake() { mover = GetComponent<PlayerMover>(); }

    public bool TryStart(Vector3 worldDirection)
    {
        if (!IsReady) return false;
        if (worldDirection.sqrMagnitude < 0.01f) worldDirection = mover.LastFacing;
        dashDir = worldDirection.normalized;
        dashTimer = duration;
        cooldownTimer = cooldown;
        return true;
    }

    void Update()
    {
        if (dashTimer > 0f)
        {
            dashTimer -= Time.deltaTime;
            mover.OverrideVelocity = dashDir * speed;
        }
        else if (mover.OverrideVelocity.HasValue)
        {
            mover.OverrideVelocity = null;
        }
        if (cooldownTimer > 0f) cooldownTimer -= Time.deltaTime;
    }
}
