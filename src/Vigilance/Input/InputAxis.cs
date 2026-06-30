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

    public InputAxis(
        in ReadOnlySpan<Key> negativeKeys = default,
        in ReadOnlySpan<Key> positiveKeys = default,
        in ReadOnlySpan<GamepadButton> negativeGamepadButtons = default,
        in ReadOnlySpan<GamepadButton> positiveGamepadButtons = default,
        in ReadOnlySpan<GamepadAxis> gamepadAxes = default,
        in ReadOnlySpan<Gamepad> gamepads = default
    )
    {
        if (!negativeKeys.IsEmpty)
            _negativeKeys = negativeKeys.AsValueEnumerable().ToValueList();
        if (!positiveKeys.IsEmpty)
            _positiveKeys = positiveKeys.AsValueEnumerable().ToValueList();
        if (!negativeGamepadButtons.IsEmpty)
            _negativeGamepadButtons = negativeGamepadButtons.AsValueEnumerable().ToValueList();
        if (!positiveGamepadButtons.IsEmpty)
            _positiveGamepadButtons = positiveGamepadButtons.AsValueEnumerable().ToValueList();
        if (!gamepadAxes.IsEmpty)
            _gamepadAxes = gamepadAxes.AsValueEnumerable().ToValueList();
        if (!gamepads.IsEmpty)
            _gamepads = gamepads.AsValueEnumerable().ToValueList();
    }

    public ValueListRef<GamepadAxis> GamepadAxes => _gamepadAxes;
    public ValueListRef<Gamepad> Gamepads => _gamepads;
    public ValueListRef<GamepadButton> NegativeGamepadButtons => _negativeGamepadButtons;
    public ValueListRef<Key> NegativeKeys => _negativeKeys;
    public ValueListRef<GamepadButton> PositiveGamepadButtons => _positiveGamepadButtons;
    public ValueListRef<Key> PositiveKeys => _positiveKeys;

    public float DeadZone { get; set; } = 0;

    public int Direction
    {
        get
        {
            var negative =
                NegativeKeys.AsValueEnumerable().Any(Keyboard.IsKeyDown)
                || Gamepads
                    .AsValueEnumerable()
                    .Cross(NegativeGamepadButtons.AsValueEnumerable())
                    .Any(pair => pair.Left.IsButtonDown(pair.Right))
                || Gamepads
                    .AsValueEnumerable()
                    .Cross(GamepadAxes.AsValueEnumerable())
                    .Cross(DeadZone.AsSingleton().AsValueEnumerable())
                    .Any(x =>
                        (int)(x.Left.Left.GetAxis(x.Left.Right) - x.Right).Round(MidpointRounding.AwayFromZero) <= -1
                    );
            var positive =
                PositiveKeys.AsValueEnumerable().Any(Keyboard.IsKeyDown)
                || Gamepads
                    .AsValueEnumerable()
                    .Cross(PositiveGamepadButtons.AsValueEnumerable())
                    .Any(pair => pair.Left.IsButtonDown(pair.Right))
                || Gamepads
                    .AsValueEnumerable()
                    .Cross(GamepadAxes.AsValueEnumerable())
                    .Cross(DeadZone.AsSingleton().AsValueEnumerable())
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
            if (NegativeKeys.AsValueEnumerable().Any(Keyboard.IsKeyDown))
                negative = -1;
            if (negative == 0)
                if (
                    Gamepads
                        .AsValueEnumerable()
                        .Cross(NegativeGamepadButtons.AsValueEnumerable())
                        .Any(x => x.Left.IsButtonDown(x.Right))
                )
                    negative = -1;
            if (negative == 0)
            {
                var pair = Gamepads
                    .AsValueEnumerable()
                    .Cross(GamepadAxes.AsValueEnumerable())
                    .Cross(DeadZone.AsSingleton().AsValueEnumerable())
                    .Where(x =>
                        (int)(x.Left.Left.GetAxis(x.Left.Right) - x.Right).Round(MidpointRounding.AwayFromZero) <= -1
                    )
                    .Select(x => x.Left)
                    .FirstOrDefault();
                if (pair != default)
                    negative = pair.Left.GetAxis(pair.Right);
            }

            float positive = 0;
            if (PositiveKeys.AsValueEnumerable().Any(Keyboard.IsKeyDown))
                positive = 1;
            if (positive == 0)
                if (
                    Gamepads
                        .AsValueEnumerable()
                        .Cross(PositiveGamepadButtons.AsValueEnumerable())
                        .Any(x => x.Left.IsButtonDown(x.Right))
                )
                    positive = 1;
            if (positive == 0)
            {
                var cross = Gamepads
                    .AsValueEnumerable()
                    .Cross(GamepadAxes.AsValueEnumerable())
                    .Cross(DeadZone.AsSingleton().AsValueEnumerable())
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
            if (NegativeKeys.AsValueEnumerable().Any(Keyboard.IsKeyDown))
                negative = -1;
            if (negative == 0)
                if (
                    Gamepads
                        .AsValueEnumerable()
                        .Cross(NegativeGamepadButtons.AsValueEnumerable())
                        .Any(x => x.Left.IsButtonDown(x.Right))
                )
                    negative = -1;
            if (negative == 0)
            {
                var cross = Gamepads
                    .AsValueEnumerable()
                    .Cross(GamepadAxes.AsValueEnumerable())
                    .FirstOrDefault(x => x.Left.GetAxis(x.Right) < 0);
                if (cross != default)
                    negative = cross.Left.GetAxis(cross.Right);
            }

            float positive = 0;
            if (PositiveKeys.AsValueEnumerable().Any(Keyboard.IsKeyDown))
                positive = 1;
            if (positive == 0)
                if (
                    Gamepads
                        .AsValueEnumerable()
                        .Cross(PositiveGamepadButtons.AsValueEnumerable())
                        .Any(x => x.Left.IsButtonDown(x.Right))
                )
                    positive = 1;
            if (positive == 0)
            {
                var cross = Gamepads
                    .AsValueEnumerable()
                    .Cross(GamepadAxes.AsValueEnumerable())
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
