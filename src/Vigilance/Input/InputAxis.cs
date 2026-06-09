using Vigilance.Math;
using ZLinq;

namespace Vigilance.Input;

public sealed class InputAxis
{
    public List<GamepadAxis> GamepadAxes { get; init; } = [];
    public List<Gamepad> Gamepads { get; init; } = Gamepad.Gamepads.AsValueEnumerable().ToList();
    public List<GamepadButton> NegativeGamepadButtons { get; init; } = [];
    public List<Key> NegativeKeys { get; init; } = [];
    public List<GamepadButton> PositiveGamepadButtons { get; init; } = [];
    public List<Key> PositiveKeys { get; init; } = [];

    public float DeadZone { get; set; } = 0;

    public int Direction
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
                            .Any(axis =>
                                (int)(gamepad.GetAxis(axis) - DeadZone).Round(MidpointRounding.AwayFromZero) <= -1
                            )
                    );
            var positive =
                PositiveKeys.AsValueEnumerable().Any(Keyboard.IsKeyDown)
                || Gamepads
                    .AsValueEnumerable()
                    .Any(gamepad =>
                        PositiveGamepadButtons.AsValueEnumerable().Any(gamepad.IsButtonDown)
                        || GamepadAxes
                            .AsValueEnumerable()
                            .Any(axis =>
                                (int)(gamepad.GetAxis(axis) + DeadZone).Round(MidpointRounding.AwayFromZero) >= 1
                            )
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
                        .Any(gamepad => NegativeGamepadButtons.AsValueEnumerable().Any(gamepad.IsButtonDown))
                )
                    negative = -1;
            if (negative == 0)
            {
                var (gamepad, axis) = Gamepads
                    .AsValueEnumerable()
                    .SelectMany(gamepad => GamepadAxes.AsValueEnumerable().Select(axis => (gamepad, axis)))
                    .FirstOrDefault(value =>
                        (int)(value.gamepad.GetAxis(value.axis) - DeadZone).Round(MidpointRounding.AwayFromZero) <= -1
                    );
                if (gamepad is not null)
                    negative = gamepad.GetAxis(axis);
            }

            float positive = 0;
            if (PositiveKeys.AsValueEnumerable().Any(Keyboard.IsKeyDown))
                positive = 1;
            if (positive == 0)
                if (
                    Gamepads
                        .AsValueEnumerable()
                        .Any(gamepad => PositiveGamepadButtons.AsValueEnumerable().Any(gamepad.IsButtonDown))
                )
                    positive = 1;
            if (positive == 0)
            {
                var (gamepad, axis) = Gamepads
                    .AsValueEnumerable()
                    .SelectMany(gamepad => GamepadAxes.AsValueEnumerable().Select(axis => (gamepad, axis)))
                    .FirstOrDefault(value =>
                        (int)(value.gamepad.GetAxis(value.axis) - DeadZone).Round(MidpointRounding.AwayFromZero) >= 1
                    );
                if (gamepad is not null)
                    positive = gamepad.GetAxis(axis);
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
                        .Any(gamepad => NegativeGamepadButtons.AsValueEnumerable().Any(gamepad.IsButtonDown))
                )
                    negative = -1;
            if (negative == 0)
            {
                var (gamepad, axis) = Gamepads
                    .AsValueEnumerable()
                    .SelectMany(gamepad => GamepadAxes.AsValueEnumerable().Select(axis => (gamepad, axis)))
                    .FirstOrDefault(value => value.gamepad.GetAxis(value.axis) < 0);
                if (gamepad is not null)
                    negative = gamepad.GetAxis(axis);
            }

            float positive = 0;
            if (PositiveKeys.AsValueEnumerable().Any(Keyboard.IsKeyDown))
                positive = 1;
            if (positive == 0)
                if (
                    Gamepads
                        .AsValueEnumerable()
                        .Any(gamepad => PositiveGamepadButtons.AsValueEnumerable().Any(gamepad.IsButtonDown))
                )
                    positive = 1;
            if (positive == 0)
            {
                var (gamepad, axis) = Gamepads
                    .AsValueEnumerable()
                    .SelectMany(gamepad => GamepadAxes.AsValueEnumerable().Select(axis => (gamepad, axis)))
                    .FirstOrDefault(value => value.gamepad.GetAxis(value.axis) > 0);
                if (gamepad is not null)
                    positive = gamepad.GetAxis(axis);
            }

            if (negative.Abs() > positive.Abs())
                return negative;
            if (positive.Abs() > negative.Abs())
                return positive;
            return 0;
        }
    }
}
