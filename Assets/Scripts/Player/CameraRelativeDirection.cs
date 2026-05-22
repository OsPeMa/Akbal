using UnityEngine;

public static class CameraRelativeDirection
{
    public static Vector3 ToWorld(Vector2 input, Transform cameraTransform)
    {
        if (cameraTransform == null) return new Vector3(input.x, 0f, input.y);
        Vector3 fwd = cameraTransform.forward; fwd.y = 0f; fwd.Normalize();
        Vector3 right = cameraTransform.right; right.y = 0f; right.Normalize();
        return right * input.x + fwd * input.y;
    }
}
