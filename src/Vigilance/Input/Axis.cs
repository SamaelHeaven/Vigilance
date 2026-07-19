using System.Runtime.InteropServices;
using Vigilance.Collections;
using Vigilance.Core;
using Vigilance.Logging;
using Vigilance.Math;
using ZLinq;

namespace Vigilance.Input;

[StructLayout(LayoutKind.Explicit)]
public readonly record struct Axis
{
    private string InvalidMessage => $"{nameof(Axis)} contains a {Type}.";

    [field: FieldOffset(0)]
    public AxisType Type { get; }

    [field: FieldOffset(1)]
    public Key NegativeKey
    {
        get => Type == AxisType.Key ? field : throw new InvalidOperationException(InvalidMessage);
        init => field = Type == AxisType.Key ? value : throw new InvalidOperationException(InvalidMessage);
    }

    [field: FieldOffset(3)]
    public Key PositiveKey
    {
        get => Type == AxisType.Key ? field : throw new InvalidOperationException(InvalidMessage);
        init => field = Type == AxisType.Key ? value : throw new InvalidOperationException(InvalidMessage);
    }

    [field: FieldOffset(1)]
    public MouseButton NegativeMouseButton
    {
        get => Type == AxisType.MouseButton ? field : throw new InvalidOperationException(InvalidMessage);
        init => field = Type == AxisType.MouseButton ? value : throw new InvalidOperationException(InvalidMessage);
    }

    [field: FieldOffset(2)]
    public MouseButton PositiveMouseButton
    {
        get => Type == AxisType.MouseButton ? field : throw new InvalidOperationException(InvalidMessage);
        init => field = Type == AxisType.MouseButton ? value : throw new InvalidOperationException(InvalidMessage);
    }

    [field: FieldOffset(1)]
    public GamepadButton NegativeGamepadButton
    {
        get => Type == AxisType.GamepadButton ? field : throw new InvalidOperationException(InvalidMessage);
        init => field = Type == AxisType.GamepadButton ? value : throw new InvalidOperationException(InvalidMessage);
    }

    [field: FieldOffset(2)]
    public GamepadButton PositiveGamepadButton
    {
        get => Type == AxisType.GamepadButton ? field : throw new InvalidOperationException(InvalidMessage);
        init => field = Type == AxisType.GamepadButton ? value : throw new InvalidOperationException(InvalidMessage);
    }

    [field: FieldOffset(1)]
    public GamepadAxis GamepadAxis
    {
        get => Type == AxisType.GamepadAxis ? field : throw new InvalidOperationException(InvalidMessage);
        init => field = Type == AxisType.GamepadAxis ? value : throw new InvalidOperationException(InvalidMessage);
    }

    [field: FieldOffset(3)]
    public Gamepads Gamepads
    {
        get =>
            Type is AxisType.GamepadButton or AxisType.GamepadAxis
                ? field
                : throw new InvalidOperationException(InvalidMessage);
        init =>
            field = Type is AxisType.GamepadButton or AxisType.GamepadAxis
                ? value
                : throw new InvalidOperationException(InvalidMessage);
    }

    [field: FieldOffset(4)]
    public float DeadZone
    {
        get => Type == AxisType.GamepadAxis ? field : throw new InvalidOperationException(InvalidMessage);
        init => field = Type == AxisType.GamepadAxis ? value : throw new InvalidOperationException(InvalidMessage);
    }

    public Axis(AxisType type)
    {
        Type = type;
    }

    public static Axis From(Key negativeKey, Key positiveKey)
    {
        return new Axis(AxisType.Key) { NegativeKey = negativeKey, PositiveKey = positiveKey };
    }

    public static Axis From(MouseButton negativeMouseButton, MouseButton positiveMouseButton)
    {
        return new Axis(AxisType.MouseButton)
        {
            NegativeMouseButton = negativeMouseButton,
            PositiveMouseButton = positiveMouseButton,
        };
    }

    public static Axis From(
        GamepadButton negativeGamepadButton,
        GamepadButton positiveGamepadButton,
        Gamepads gamepads = Gamepads.All
    )
    {
        return new Axis(AxisType.GamepadButton)
        {
            NegativeGamepadButton = negativeGamepadButton,
            PositiveGamepadButton = positiveGamepadButton,
            Gamepads = gamepads,
        };
    }

    public static Axis From(GamepadAxis gamepadAxis, Gamepads gamepads = Gamepads.All, float deadZone = 0)
    {
        return new Axis(AxisType.GamepadAxis)
        {
            GamepadAxis = gamepadAxis,
            Gamepads = gamepads,
            DeadZone = deadZone,
        };
    }

    public static implicit operator Axis((Key NegativeKey, Key PositiveKey) keys)
    {
        return From(keys.NegativeKey, keys.PositiveKey);
    }

    public static implicit operator Axis(
        (MouseButton NegativeMouseButton, MouseButton PositiveMouseButton) mouseButtons
    )
    {
        return From(mouseButtons.NegativeMouseButton, mouseButtons.PositiveMouseButton);
    }

    public static implicit operator Axis(
        (GamepadButton NegativeGamepadButton, GamepadButton PositiveGamepadButton) gamepadButtons
    )
    {
        return From(gamepadButtons.NegativeGamepadButton, gamepadButtons.PositiveGamepadButton);
    }

    public static implicit operator Axis(GamepadAxis gamepadAxis)
    {
        return From(gamepadAxis);
    }

    public int Direction
    {
        get
        {
            var negative = false;
            var positive = false;
            switch (Type)
            {
                case AxisType.Key:
                    negative = Keyboard.IsKeyDown(NegativeKey);
                    positive = Keyboard.IsKeyDown(PositiveKey);
                    break;
                case AxisType.MouseButton:
                    negative = Mouse.IsButtonDown(NegativeMouseButton);
                    positive = Mouse.IsButtonDown(PositiveMouseButton);
                    break;
                case AxisType.GamepadButton:
                    negative = Gamepads
                        .AsValueEnumerable()
                        .Cross(NegativeGamepadButton.AsValueSingleton())
                        .Any(pair => pair.Left.IsButtonDown(pair.Right));
                    positive = Gamepads
                        .AsValueEnumerable()
                        .Cross(PositiveGamepadButton.AsValueSingleton())
                        .Any(pair => pair.Left.IsButtonDown(pair.Right));
                    break;
                case AxisType.GamepadAxis:
                    negative = Gamepads
                        .AsValueEnumerable()
                        .Cross(GamepadAxis.AsValueSingleton())
                        .Cross(DeadZone.AsValueSingleton())
                        .Any(x =>
                            (int)(x.Left.Left.GetAxis(x.Left.Right) - x.Right).Round(MidpointRounding.AwayFromZero)
                            <= -1
                        );
                    positive = Gamepads
                        .AsValueEnumerable()
                        .Cross(GamepadAxis.AsValueSingleton())
                        .Cross(DeadZone.AsValueSingleton())
                        .Any(x =>
                            (int)(x.Left.Left.GetAxis(x.Left.Right) + x.Right).Round(MidpointRounding.AwayFromZero) >= 1
                        );
                    break;
            }

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
            var negative = 0f;
            var positive = 0f;
            switch (Type)
            {
                case AxisType.Key:
                    negative = Keyboard.IsKeyDown(NegativeKey) ? -1 : 0;
                    positive = Keyboard.IsKeyDown(PositiveKey) ? 1 : 0;
                    break;
                case AxisType.MouseButton:
                    negative = Mouse.IsButtonDown(NegativeMouseButton) ? -1 : 0;
                    positive = Mouse.IsButtonDown(PositiveMouseButton) ? 1 : 0;
                    break;
                case AxisType.GamepadButton:
                    negative = Gamepads
                        .AsValueEnumerable()
                        .Cross(NegativeGamepadButton.AsValueSingleton())
                        .Any(x => x.Left.IsButtonDown(x.Right))
                        ? -1
                        : 0;
                    positive = Gamepads
                        .AsValueEnumerable()
                        .Cross(PositiveGamepadButton.AsValueSingleton())
                        .Any(x => x.Left.IsButtonDown(x.Right))
                        ? 1
                        : 0;
                    break;
                case AxisType.GamepadAxis:
                    var cross = Gamepads
                        .AsValueEnumerable()
                        .Cross(GamepadAxis.AsValueSingleton())
                        .Cross(DeadZone.AsValueSingleton())
                        .Where(x =>
                            (int)(x.Left.Left.GetAxis(x.Left.Right) - x.Right).Round(MidpointRounding.AwayFromZero)
                            <= -1
                        )
                        .Select(x => x.Left)
                        .FirstOrDefault();
                    if (cross != default)
                        negative = cross.Left.GetAxis(cross.Right);
                    cross = Gamepads
                        .AsValueEnumerable()
                        .Cross(GamepadAxis.AsValueSingleton())
                        .Cross(DeadZone.AsValueSingleton())
                        .Where(x =>
                            (int)(x.Left.Left.GetAxis(x.Left.Right) + x.Right).Round(MidpointRounding.AwayFromZero) >= 1
                        )
                        .Select(x => x.Left)
                        .FirstOrDefault();
                    if (cross != default)
                        positive = cross.Left.GetAxis(cross.Right);
                    break;
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
            var negative = 0f;
            var positive = 0f;
            switch (Type)
            {
                case AxisType.Key:
                    negative = Keyboard.IsKeyDown(NegativeKey) ? -1 : 0;
                    positive = Keyboard.IsKeyDown(PositiveKey) ? 1 : 0;
                    break;
                case AxisType.MouseButton:
                    negative = Mouse.IsButtonDown(NegativeMouseButton) ? -1 : 0;
                    positive = Mouse.IsButtonDown(PositiveMouseButton) ? 1 : 0;
                    break;
                case AxisType.GamepadButton:
                    negative = Gamepads
                        .AsValueEnumerable()
                        .Cross(NegativeGamepadButton.AsValueSingleton())
                        .Any(x => x.Left.IsButtonDown(x.Right))
                        ? -1
                        : 0;
                    positive = Gamepads
                        .AsValueEnumerable()
                        .Cross(PositiveGamepadButton.AsValueSingleton())
                        .Any(x => x.Left.IsButtonDown(x.Right))
                        ? 1
                        : 0;
                    break;
                case AxisType.GamepadAxis:
                    var cross = Gamepads
                        .AsValueEnumerable()
                        .Cross(GamepadAxis.AsValueSingleton())
                        .FirstOrDefault(x => x.Left.GetAxis(x.Right) < 0);
                    if (cross != default)
                        negative = cross.Left.GetAxis(cross.Right);
                    cross = Gamepads
                        .AsValueEnumerable()
                        .Cross(GamepadAxis.AsValueSingleton())
                        .FirstOrDefault(x => x.Left.GetAxis(x.Right) > 0);
                    if (cross != default)
                        positive = cross.Left.GetAxis(cross.Right);
                    break;
            }

            if (negative.Abs() > positive.Abs())
                return negative;
            if (positive.Abs() > negative.Abs())
                return positive;
            return 0;
        }
    }

    public override string ToString()
    {
        return Type switch
        {
            AxisType.Key => ObjectPrinter.Print(
                this,
                ObjectPrinter.Include([nameof(Type), nameof(NegativeKey), nameof(PositiveKey)])
            ),
            AxisType.MouseButton => ObjectPrinter.Print(
                this,
                ObjectPrinter.Include([nameof(Type), nameof(NegativeMouseButton), nameof(PositiveMouseButton)])
            ),
            AxisType.GamepadButton => ObjectPrinter.Print(
                this,
                ObjectPrinter.Include([
                    nameof(Type),
                    nameof(NegativeGamepadButton),
                    nameof(PositiveGamepadButton),
                    nameof(Gamepads),
                ])
            ),
            AxisType.GamepadAxis => ObjectPrinter.Print(
                this,
                ObjectPrinter.Include([nameof(Type), nameof(GamepadAxis), nameof(Gamepads), nameof(DeadZone)])
            ),
            _ => ObjectPrinter.Print(this, ObjectPrinter.Include([nameof(Type)])),
        };
    }
}
