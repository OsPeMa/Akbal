using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMover : MonoBehaviour
{
    [Header("Speed")]
    public float maxSpeed = 8f;
    public float accelTime = 0.06f;
    public float decelTime = 0.08f;

    public Vector3 DesiredVelocity { get; set; }
    public Vector3? OverrideVelocity { get; set; }
    public Vector3 CurrentVelocity { get; private set; }
    public Vector3 LastFacing { get; private set; } = Vector3.forward;

    Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;
        rb.linearDamping = 0f;
    }

    void FixedUpdate()
    {
        Vector3 v;
        if (OverrideVelocity.HasValue)
        {
            v = OverrideVelocity.Value;
            CurrentVelocity = v;
        }
        else
        {
            float rate = (DesiredVelocity.sqrMagnitude > CurrentVelocity.sqrMagnitude)
                ? maxSpeed / Mathf.Max(0.0001f, accelTime)
                : maxSpeed / Mathf.Max(0.0001f, decelTime);
            CurrentVelocity = Vector3.MoveTowards(CurrentVelocity, DesiredVelocity, rate * Time.fixedDeltaTime);
            v = CurrentVelocity;
        }
        rb.linearVelocity = new Vector3(v.x, 0f, v.z);
        if (v.sqrMagnitude > 0.01f) LastFacing = v.normalized;
    }
}
