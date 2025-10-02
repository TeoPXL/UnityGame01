// Assets/Scripts/state/InputUtils.cs
using UnityEngine;
using UnityEngine.InputSystem;

public static class InputUtils
{
    /// <summary>
    /// Returns true if the player pressed the "pause/menu" control this frame.
    /// Checks keyboard (Esc) and common gamepad buttons (Start/Select).
    /// Safe to call from non-MonoBehaviour code (like your IGameState Tick()).
    /// </summary>
    public static bool WasPausePressedThisFrame()
    {
        // Keyboard: Escape
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            return true;

        // Gamepad: check common menu/start/select buttons
        var gp = Gamepad.current;
        if (gp != null)
        {
            if (gp.startButton != null && gp.startButton.wasPressedThisFrame) return true;
            if (gp.selectButton != null && gp.selectButton.wasPressedThisFrame) return true;
        }

        // No pause input detected
        return false;
    }
}
