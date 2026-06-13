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
                    .Cross(NegativeGamepadButtons.AsValueEnumerable())
                    .Any(pair => pair.Left.IsButtonDown(pair.Right))
                || Gamepads
                    .AsValueEnumerable()
                    .Cross(GamepadAxes.AsValueEnumerable())
                    .Cross(deadZone.AsSpan().AsValueEnumerable())
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
                    .Cross(deadZone.AsSpan().AsValueEnumerable())
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
            var deadZone = DeadZone;
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
                    .Cross(deadZone.AsSpan().AsValueEnumerable())
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
                    .Cross(deadZone.AsSpan().AsValueEnumerable())
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
