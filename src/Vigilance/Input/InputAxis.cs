using Vigilance.Math;
using ZLinq;

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
                NegativeKeys.AsValueEnumerable().Any(Keyboard.IsKeyDown)
                || Gamepads
                    .AsValueEnumerable()
                    .Any(gamepad =>
                        NegativeGamepadButtons.AsValueEnumerable().Any(gamepad.IsButtonDown)
                        || GamepadAxes
                            .AsValueEnumerable()
                            .Any(axis => (int)(gamepad.GetAxis(axis) - DeadZone).Round() <= -1)
                    );
            var positive =
                PositiveKeys.AsValueEnumerable().Any(Keyboard.IsKeyDown)
                || Gamepads
                    .AsValueEnumerable()
                    .Any(gamepad =>
                        PositiveGamepadButtons.AsValueEnumerable().Any(gamepad.IsButtonDown)
                        || GamepadAxes
                            .AsValueEnumerable()
                            .Any(axis => (int)(gamepad.GetAxis(axis) + DeadZone).Round() >= 1)
                    );
            if (negative && !positive)
                return -1;
            if (positive && !negative)
                return 1;
            return 0;
        }
    }
}
