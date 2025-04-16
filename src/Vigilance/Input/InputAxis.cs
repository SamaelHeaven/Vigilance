using Vigilance.Core;
using Vigilance.Math;

namespace Vigilance.Input;

public struct InputAxis
{
    public IReadOnlyList<Key> NegativeKeys { get; set; } = Array.Empty<Key>();
    public IReadOnlyList<Key> PositiveKeys { get; set; } = Array.Empty<Key>();
    public IReadOnlyList<GamepadButton> NegativeGamepadButtons { get; set; } = Array.Empty<GamepadButton>();
    public IReadOnlyList<GamepadButton> PositiveGamepadButtons { get; set; } = Array.Empty<GamepadButton>();
    public IReadOnlyList<GamepadAxis> GamepadAxes { get; set; } = Array.Empty<GamepadAxis>();
    public Gamepad Gamepad { get; set; } = Gamepad.First;
    public float DeadZone { get; set; } = 0;

    public static InputAxis Horizontal => Game.HorizontalInputAxis;

    public static InputAxis Vertical => Game.VerticalInputAxis;

    public static Vector2 Both()
    {
        return new Vector2(Horizontal.Get(), Vertical.Get());
    }

    public InputAxis() { }

    public readonly int Get()
    {
        var gamepad = Gamepad;
        var deadZone = DeadZone;
        var negative =
            NegativeKeys.Any(Keyboard.IsKeyDown)
            || NegativeGamepadButtons.Any(button => gamepad.IsButtonDown(button))
            || GamepadAxes.Any(axis => (int)MathF.Round(gamepad.GetAxis(axis) - deadZone) <= -1);
        var positive =
            PositiveKeys.Any(Keyboard.IsKeyDown)
            || PositiveGamepadButtons.Any(button => gamepad.IsButtonDown(button))
            || GamepadAxes.Any(axis => (int)MathF.Round(gamepad.GetAxis(axis) + deadZone) >= 1);
        if (negative && !positive)
            return -1;
        if (positive && !negative)
            return 1;
        return 0;
    }
}
