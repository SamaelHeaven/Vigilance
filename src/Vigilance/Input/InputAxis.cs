using Vigilance.Math;

namespace Vigilance.Input;

public sealed class InputAxis
{
    public IReadOnlyList<Key> NegativeKeys { get; init; } = Array.Empty<Key>();
    public IReadOnlyList<Key> PositiveKeys { get; init; } = Array.Empty<Key>();
    public IReadOnlyList<GamepadButton> NegativeGamepadButtons { get; init; } = Array.Empty<GamepadButton>();
    public IReadOnlyList<GamepadButton> PositiveGamepadButtons { get; init; } = Array.Empty<GamepadButton>();
    public IReadOnlyList<GamepadAxis> GamepadAxes { get; init; } = Array.Empty<GamepadAxis>();
    public IReadOnlyList<Gamepad> Gamepads { get; init; } = Gamepad.Gamepads;
    public float DeadZone { get; init; } = 0;

    public int Value
    {
        get
        {
            var negative =
                NegativeKeys.Any(Keyboard.IsKeyDown)
                || Gamepads.Any(gamepad =>
                    NegativeGamepadButtons.Any(gamepad.IsButtonDown)
                    || GamepadAxes.Any(axis => (int)(gamepad.GetAxis(axis) - DeadZone).Round() <= -1)
                );
            var positive =
                PositiveKeys.Any(Keyboard.IsKeyDown)
                || Gamepads.Any(gamepad =>
                    PositiveGamepadButtons.Any(gamepad.IsButtonDown)
                    || GamepadAxes.Any(axis => (int)(gamepad.GetAxis(axis) + DeadZone).Round() >= 1)
                );
            if (negative && !positive)
                return -1;
            if (positive && !negative)
                return 1;
            return 0;
        }
    }
}
