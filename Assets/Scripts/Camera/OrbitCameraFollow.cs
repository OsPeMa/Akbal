using UnityEngine;

public class OrbitCameraFollow : MonoBehaviour
{
    public Transform target;
    [Tooltip("Distance from the target along the view direction.")]
    public float distance = 25f;
    [Tooltip("Tilt down from horizontal. 90 = pure top-down, 30 = classic isometric.")]
    [Range(10f, 90f)] public float pitch = 30f;
    [Tooltip("Yaw around the target. 45 gives the standard isometric look.")]
    public float yaw = 45f;
    public float smooth = 8f;

    void LateUpdate()
    {
        if (target == null) return;
        Quaternion rot = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 want = target.position - rot * Vector3.forward * distance;
        transform.position = Vector3.Lerp(transform.position, want, 1f - Mathf.Exp(-smooth * Time.deltaTime));
        transform.rotation = rot;
    }
}
