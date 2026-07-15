using Vigilance.Collections;
using Vigilance.Core;
using Vigilance.Math;
using ZLinq;

namespace Vigilance.Input;

public sealed class InputAxis
{
    private ValueList<GamepadAxis> _gamepadAxes = [];
    private ValueList<Gamepad> _gamepads = Gamepad.Gamepads.AsValueEnumerable().ToValueList();
    private ValueList<GamepadButton> _negativeGamepadButtons = [];
    private ValueList<Key> _negativeKeys = [];
    private ValueList<GamepadButton> _positiveGamepadButtons = [];
    private ValueList<Key> _positiveKeys = [];

    public ValueListRef<GamepadAxis> GamepadAxes
    {
        get => _gamepadAxes.AsRef();
        set => value.CopyTo(ref _gamepadAxes);
    }

    public ValueListRef<Gamepad> Gamepads
    {
        get => _gamepads.AsRef();
        set => value.CopyTo(ref _gamepads);
    }

    public ValueListRef<GamepadButton> NegativeGamepadButtons
    {
        get => _negativeGamepadButtons.AsRef();
        set => value.CopyTo(ref _negativeGamepadButtons);
    }

    public ValueListRef<Key> NegativeKeys
    {
        get => _negativeKeys.AsRef();
        set => value.CopyTo(ref _negativeKeys);
    }

    public ValueListRef<GamepadButton> PositiveGamepadButtons
    {
        get => _positiveGamepadButtons.AsRef();
        set => value.CopyTo(ref _positiveGamepadButtons);
    }

    public ValueListRef<Key> PositiveKeys
    {
        get => _positiveKeys.AsRef();
        set => value.CopyTo(ref _positiveKeys);
    }

    public float DeadZone { get; set; } = 0;

    public int Direction
    {
        get
        {
            var negative =
                _negativeKeys.AsValueEnumerable().Any(Keyboard.IsKeyDown)
                || _gamepads
                    .AsValueEnumerable()
                    .Cross(_negativeGamepadButtons.AsValueEnumerable())
                    .Any(pair => pair.Left.IsButtonDown(pair.Right))
                || _gamepads
                    .AsValueEnumerable()
                    .Cross(_gamepadAxes.AsValueEnumerable())
                    .Cross(DeadZone.AsValueSingleton())
                    .Any(x =>
                        (int)(x.Left.Left.GetAxis(x.Left.Right) - x.Right).Round(MidpointRounding.AwayFromZero) <= -1
                    );
            var positive =
                _positiveKeys.AsValueEnumerable().Any(Keyboard.IsKeyDown)
                || _gamepads
                    .AsValueEnumerable()
                    .Cross(_positiveGamepadButtons.AsValueEnumerable())
                    .Any(pair => pair.Left.IsButtonDown(pair.Right))
                || _gamepads
                    .AsValueEnumerable()
                    .Cross(_gamepadAxes.AsValueEnumerable())
                    .Cross(DeadZone.AsValueSingleton())
                    .Any(x =>
                        (int)(x.Left.Left.GetAxis(x.Left.Right) + x.Right).Round(MidpointRounding.AwayFromZero) >= 1
                    );
            if (negative && !positive)
                return -1;
            if (positive && !negative)
                return 1;
            return 0;
        }
    }

    public float Value
    {
        get
        {
            float negative = 0;
            if (_negativeKeys.AsValueEnumerable().Any(Keyboard.IsKeyDown))
                negative = -1;
            if (negative == 0)
                if (
                    _gamepads
                        .AsValueEnumerable()
                        .Cross(_negativeGamepadButtons.AsValueEnumerable())
                        .Any(x => x.Left.IsButtonDown(x.Right))
                )
                    negative = -1;
            if (negative == 0)
            {
                var pair = _gamepads
                    .AsValueEnumerable()
                    .Cross(_gamepadAxes.AsValueEnumerable())
                    .Cross(DeadZone.AsValueSingleton())
                    .Where(x =>
                        (int)(x.Left.Left.GetAxis(x.Left.Right) - x.Right).Round(MidpointRounding.AwayFromZero) <= -1
                    )
                    .Select(x => x.Left)
                    .FirstOrDefault();
                if (pair != default)
                    negative = pair.Left.GetAxis(pair.Right);
            }

            float positive = 0;
            if (_positiveKeys.AsValueEnumerable().Any(Keyboard.IsKeyDown))
                positive = 1;
            if (positive == 0)
                if (
                    _gamepads
                        .AsValueEnumerable()
                        .Cross(_positiveGamepadButtons.AsValueEnumerable())
                        .Any(x => x.Left.IsButtonDown(x.Right))
                )
                    positive = 1;
            if (positive == 0)
            {
                var cross = _gamepads
                    .AsValueEnumerable()
                    .Cross(_gamepadAxes.AsValueEnumerable())
                    .Cross(DeadZone.AsValueSingleton())
                    .Where(x =>
                        (int)(x.Left.Left.GetAxis(x.Left.Right) - x.Right).Round(MidpointRounding.AwayFromZero) >= 1
                    )
                    .Select(x => x.Left)
                    .FirstOrDefault();
                if (cross != default)
                    positive = cross.Left.GetAxis(cross.Right);
            }

            if (negative.Abs() > positive.Abs())
                return negative;
            if (positive.Abs() > negative.Abs())
                return positive;
            return 0;
        }
    }

    public float RawValue
    {
        get
        {
            float negative = 0;
            if (_negativeKeys.AsValueEnumerable().Any(Keyboard.IsKeyDown))
                negative = -1;
            if (negative == 0)
                if (
                    _gamepads
                        .AsValueEnumerable()
                        .Cross(_negativeGamepadButtons.AsValueEnumerable())
                        .Any(x => x.Left.IsButtonDown(x.Right))
                )
                    negative = -1;
            if (negative == 0)
            {
                var cross = _gamepads
                    .AsValueEnumerable()
                    .Cross(_gamepadAxes.AsValueEnumerable())
                    .FirstOrDefault(x => x.Left.GetAxis(x.Right) < 0);
                if (cross != default)
                    negative = cross.Left.GetAxis(cross.Right);
            }

            float positive = 0;
            if (_positiveKeys.AsValueEnumerable().Any(Keyboard.IsKeyDown))
                positive = 1;
            if (positive == 0)
                if (
                    _gamepads
                        .AsValueEnumerable()
                        .Cross(_positiveGamepadButtons.AsValueEnumerable())
                        .Any(x => x.Left.IsButtonDown(x.Right))
                )
                    positive = 1;
            if (positive == 0)
            {
                var cross = _gamepads
                    .AsValueEnumerable()
                    .Cross(_gamepadAxes.AsValueEnumerable())
                    .FirstOrDefault(x => x.Left.GetAxis(x.Right) > 0);
                if (cross != default)
                    positive = cross.Left.GetAxis(cross.Right);
            }

            if (negative.Abs() > positive.Abs())
                return negative;
            if (positive.Abs() > negative.Abs())
                return positive;
            return 0;
        }
    }
}
