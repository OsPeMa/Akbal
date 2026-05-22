using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class RhythmInputReader : MonoBehaviour
{
    public event Action<RhythmRail> RailPressed;

    void Update()
    {
        var kb = Keyboard.current;
        var gp = Gamepad.current;

        if (kb != null)
        {
            if (kb.upArrowKey.wasPressedThisFrame)    RailPressed?.Invoke(RhythmRail.Up);
            if (kb.rightArrowKey.wasPressedThisFrame) RailPressed?.Invoke(RhythmRail.Right);
            if (kb.downArrowKey.wasPressedThisFrame)  RailPressed?.Invoke(RhythmRail.Down);
            if (kb.leftArrowKey.wasPressedThisFrame)  RailPressed?.Invoke(RhythmRail.Left);
        }
        if (gp != null)
        {
            if (gp.dpad.up.wasPressedThisFrame    || gp.buttonNorth.wasPressedThisFrame) RailPressed?.Invoke(RhythmRail.Up);
            if (gp.dpad.right.wasPressedThisFrame || gp.buttonEast.wasPressedThisFrame)  RailPressed?.Invoke(RhythmRail.Right);
            if (gp.dpad.down.wasPressedThisFrame  || gp.buttonSouth.wasPressedThisFrame) RailPressed?.Invoke(RhythmRail.Down);
            if (gp.dpad.left.wasPressedThisFrame  || gp.buttonWest.wasPressedThisFrame)  RailPressed?.Invoke(RhythmRail.Left);
        }
    }
}
