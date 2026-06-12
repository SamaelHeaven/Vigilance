using Vigilance.Collections;
using Vigilance.Math;
using ZLinq;

namespace Vigilance.Input;

public sealed class InputAxis
{
    public List<GamepadAxis> GamepadAxes { get; set; } = [];
    public List<Gamepad> Gamepads { get; set; } = Gamepad.Gamepads.AsValueEnumerable().ToList();
    public List<GamepadButton> NegativeGamepadButtons { get; set; } = [];
    public List<Key> NegativeKeys { get; set; } = [];
    public List<GamepadButton> PositiveGamepadButtons { get; set; } = [];
    public List<Key> PositiveKeys { get; set; } = [];

    public float DeadZone { get; set; } = 0;

    public int Direction
    {
        get
        {
            var deadZone = DeadZone;
            var negative =
                NegativeKeys.AsValueEnumerable().Any(Keyboard.IsKeyDown)
                || Gamepads
                    .AsValueEnumerable()
                    .Pair(NegativeGamepadButtons.AsValueEnumerable())
                    .Any(pair => pair.First.IsButtonDown(pair.Second))
                || Gamepads
                    .AsValueEnumerable()
                    .Pair(GamepadAxes.AsValueEnumerable())
                    .Pair(deadZone.AsSpan().AsValueEnumerable())
                    .Any(pair =>
                        (int)
                            (pair.First.First.GetAxis(pair.First.Second) - pair.Second).Round(
                                MidpointRounding.AwayFromZero
                            ) <= -1
                    );
            var positive =
                PositiveKeys.AsValueEnumerable().Any(Keyboard.IsKeyDown)
                || Gamepads
                    .AsValueEnumerable()
                    .Pair(PositiveGamepadButtons.AsValueEnumerable())
                    .Any(pair => pair.First.IsButtonDown(pair.Second))
                || Gamepads
                    .AsValueEnumerable()
                    .Pair(GamepadAxes.AsValueEnumerable())
                    .Pair(deadZone.AsSpan().AsValueEnumerable())
                    .Any(pair =>
                        (int)
                            (pair.First.First.GetAxis(pair.First.Second) + pair.Second).Round(
                                MidpointRounding.AwayFromZero
                            ) >= 1
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
            var deadZone = DeadZone;
            float negative = 0;
            if (NegativeKeys.AsValueEnumerable().Any(Keyboard.IsKeyDown))
                negative = -1;
            if (negative == 0)
                if (
                    Gamepads
                        .AsValueEnumerable()
                        .Pair(NegativeGamepadButtons.AsValueEnumerable())
                        .Any(pair => pair.First.IsButtonDown(pair.Second))
                )
                    negative = -1;
            if (negative == 0)
            {
                var pair = Gamepads
                    .AsValueEnumerable()
                    .Pair(GamepadAxes.AsValueEnumerable())
                    .Pair(deadZone.AsSpan().AsValueEnumerable())
                    .FirstOrDefault(value =>
                        (int)
                            (value.First.First.GetAxis(value.First.Second) - value.Second).Round(
                                MidpointRounding.AwayFromZero
                            ) <= -1
                    );
                if (pair != default)
                    negative = pair.First.First.GetAxis(pair.First.Second);
            }

            float positive = 0;
            if (PositiveKeys.AsValueEnumerable().Any(Keyboard.IsKeyDown))
                positive = 1;
            if (positive == 0)
                if (
                    Gamepads
                        .AsValueEnumerable()
                        .Pair(PositiveGamepadButtons.AsValueEnumerable())
                        .Any(pair => pair.First.IsButtonDown(pair.Second))
                )
                    positive = 1;
            if (positive == 0)
            {
                var pair = Gamepads
                    .AsValueEnumerable()
                    .Pair(GamepadAxes.AsValueEnumerable())
                    .Pair(deadZone.AsSpan().AsValueEnumerable())
                    .FirstOrDefault(value =>
                        (int)
                            (value.First.First.GetAxis(value.First.Second) - value.Second).Round(
                                MidpointRounding.AwayFromZero
                            ) >= 1
                    );
                if (pair != default)
                    positive = pair.First.First.GetAxis(pair.First.Second);
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
                        .Pair(NegativeGamepadButtons.AsValueEnumerable())
                        .Any(pair => pair.First.IsButtonDown(pair.Second))
                )
                    negative = -1;
            if (negative == 0)
            {
                var pair = Gamepads
                    .AsValueEnumerable()
                    .Pair(GamepadAxes.AsValueEnumerable())
                    .FirstOrDefault(value => value.First.GetAxis(value.Second) < 0);
                if (pair != default)
                    negative = pair.First.GetAxis(pair.Second);
            }

            float positive = 0;
            if (PositiveKeys.AsValueEnumerable().Any(Keyboard.IsKeyDown))
                positive = 1;
            if (positive == 0)
                if (
                    Gamepads
                        .AsValueEnumerable()
                        .Pair(PositiveGamepadButtons.AsValueEnumerable())
                        .Any(pair => pair.First.IsButtonDown(pair.Second))
                )
                    positive = 1;
            if (positive == 0)
            {
                var pair = Gamepads
                    .AsValueEnumerable()
                    .Pair(GamepadAxes.AsValueEnumerable())
                    .FirstOrDefault(value => value.First.GetAxis(value.Second) > 0);
                if (pair != default)
                    positive = pair.First.GetAxis(pair.Second);
            }

            if (negative.Abs() > positive.Abs())
                return negative;
            if (positive.Abs() > negative.Abs())
                return positive;
            return 0;
        }
    }
}
