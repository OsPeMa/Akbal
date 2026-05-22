using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputReader : MonoBehaviour
{
    public Vector2 Move { get; private set; }
    public bool DashPressed { get; private set; }
    public bool ParryPressed { get; private set; }
    public bool PurifyPressed { get; private set; }
    public bool PurifyHeld { get; private set; }

    void Update()
    {
        var kb = Keyboard.current;
        var gp = Gamepad.current;
        var ms = Mouse.current;

        Vector2 m = Vector2.zero;
        if (kb != null)
        {
            if (kb.wKey.isPressed) m.y += 1f;
            if (kb.sKey.isPressed) m.y -= 1f;
            if (kb.aKey.isPressed) m.x -= 1f;
            if (kb.dKey.isPressed) m.x += 1f;
        }
        if (gp != null)
        {
            var stick = gp.leftStick.ReadValue();
            if (stick.sqrMagnitude > 0.04f) m = stick;
        }
        if (m.sqrMagnitude > 1f) m.Normalize();
        Move = m;

        DashPressed = (kb != null && kb.spaceKey.wasPressedThisFrame)
                   || (gp != null && gp.buttonSouth.wasPressedThisFrame);

        ParryPressed = (kb != null && kb.qKey.wasPressedThisFrame)
                    || (gp != null && gp.buttonNorth.wasPressedThisFrame);

        PurifyHeld = (ms != null && ms.rightButton.isPressed)
                  || (kb != null && kb.eKey.isPressed)
                  || (gp != null && gp.rightTrigger.ReadValue() > 0.5f);

        PurifyPressed = (ms != null && ms.rightButton.wasPressedThisFrame)
                     || (kb != null && kb.eKey.wasPressedThisFrame)
                     || (gp != null && gp.rightTrigger.wasPressedThisFrame);
    }
}
