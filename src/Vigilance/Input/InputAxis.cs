using Vigilance.Math;
using ZLinq;

namespace Vigilance.Input;

public sealed class InputAxis
{
    private readonly List<GamepadAxis> _gamepadAxes = [];
    private readonly List<Gamepad> _gamepads = Gamepad.Gamepads.AsValueEnumerable().ToList();
    private readonly List<GamepadButton> _negativeGamepadButtons = [];
    private readonly List<Key> _negativeKeys = [];
    private readonly List<GamepadButton> _positiveGamepadButtons = [];
    private readonly List<Key> _positiveKeys = [];

    public IReadOnlyList<Key> NegativeKeys
    {
        get => _negativeKeys;
        init => _negativeKeys = value.AsValueEnumerable().ToList();
    }

    public IReadOnlyList<Key> PositiveKeys
    {
        get => _positiveKeys;
        init => _positiveKeys = value.AsValueEnumerable().ToList();
    }

    public IReadOnlyList<GamepadButton> NegativeGamepadButtons
    {
        get => _negativeGamepadButtons;
        init => _negativeGamepadButtons = value.AsValueEnumerable().ToList();
    }

    public IReadOnlyList<GamepadButton> PositiveGamepadButtons
    {
        get => _positiveGamepadButtons;
        init => _positiveGamepadButtons = value.AsValueEnumerable().ToList();
    }

    public IReadOnlyList<GamepadAxis> GamepadAxes
    {
        get => _gamepadAxes;
        init => _gamepadAxes = value.AsValueEnumerable().ToList();
    }

    public IReadOnlyList<Gamepad> Gamepads
    {
        get => _gamepads;
        init => _gamepads = value.AsValueEnumerable().ToList();
    }

    public float DeadZone { get; init; } = 0;

    public int Value
    {
        get
        {
            var negative =
                _negativeKeys.AsValueEnumerable().Any(Keyboard.IsKeyDown)
                || _gamepads
                    .AsValueEnumerable()
                    .Any(gamepad =>
                        _negativeGamepadButtons.AsValueEnumerable().Any(gamepad.IsButtonDown)
                        || _gamepadAxes
                            .AsValueEnumerable()
                            .Any(axis => (int)(gamepad.GetAxis(axis) - DeadZone).Round() <= -1)
                    );
            var positive =
                _positiveKeys.AsValueEnumerable().Any(Keyboard.IsKeyDown)
                || _gamepads
                    .AsValueEnumerable()
                    .Any(gamepad =>
                        _positiveGamepadButtons.AsValueEnumerable().Any(gamepad.IsButtonDown)
                        || _gamepadAxes
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
