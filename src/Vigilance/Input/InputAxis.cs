using Vigilance.Core;
using Vigilance.Math;

namespace Vigilance.Input;

public sealed class InputAxis
{
    private static InputAxesConfig _config = new();
    public IReadOnlyList<Key> NegativeKeys { get; init; } = Array.Empty<Key>();
    public IReadOnlyList<Key> PositiveKeys { get; init; } = Array.Empty<Key>();
    public IReadOnlyList<GamepadButton> NegativeGamepadButtons { get; init; } = Array.Empty<GamepadButton>();
    public IReadOnlyList<GamepadButton> PositiveGamepadButtons { get; init; } = Array.Empty<GamepadButton>();
    public IReadOnlyList<GamepadAxis> GamepadAxes { get; init; } = Array.Empty<GamepadAxis>();
    public IReadOnlyList<Gamepad> Gamepads { get; init; } = Gamepad.Gamepads;
    public float DeadZone { get; init; } = 0;

    public static InputAxis Horizontal
    {
        get => _config.Horizontal;
        set => _config.Horizontal = value;
    }

    public static InputAxis Vertical
    {
        get => _config.Vertical;
        set => _config.Vertical = value;
    }

    public static Vector2 Both => new(Horizontal.Value, Vertical.Value);

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

    internal static void Initialize()
    {
        if (Game.Configs.TryTake(out InputAxesConfig config))
            _config = config;
    }
}
