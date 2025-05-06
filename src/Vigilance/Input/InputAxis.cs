using Vigilance.Core;
using Vigilance.Math;

namespace Vigilance.Input;

public sealed class InputAxis
{
    public IReadOnlyList<Key> NegativeKeys { get; init; } = Array.Empty<Key>();
    public IReadOnlyList<Key> PositiveKeys { get; init; } = Array.Empty<Key>();
    public IReadOnlyList<GamepadButton> NegativeGamepadButtons { get; init; } = Array.Empty<GamepadButton>();
    public IReadOnlyList<GamepadButton> PositiveGamepadButtons { get; init; } = Array.Empty<GamepadButton>();
    public IReadOnlyList<GamepadAxis> GamepadAxes { get; init; } = Array.Empty<GamepadAxis>();
    public Gamepad Gamepad { get; init; } = Gamepad.First;
    public float DeadZone { get; init; } = 0;

    public static InputAxis Horizontal => Game.HorizontalInputAxis;

    public static InputAxis Vertical => Game.VerticalInputAxis;

    public static Vector2 Both => new(Horizontal.Value, Vertical.Value);

    public int Value
    {
        get
        {
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
    }
}
