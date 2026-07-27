using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Vigilance.Input;

[StructLayout(LayoutKind.Explicit)]
[Union]
public readonly record struct Axis : IUnion
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

    public Axis(Key negativeKey, Key positiveKey)
    {
        Type = AxisType.Key;
        NegativeKey = negativeKey;
        PositiveKey = positiveKey;
    }

    public Axis((Key NegativeKey, Key PositiveKey) keys)
        : this(keys.NegativeKey, keys.PositiveKey) { }

    public Axis(MouseButton negativeMouseButton, MouseButton positiveMouseButton)
    {
        Type = AxisType.MouseButton;
        NegativeMouseButton = negativeMouseButton;
        PositiveMouseButton = positiveMouseButton;
    }

    public Axis((MouseButton NegativeMouseButton, MouseButton PositiveMouseButton) mouseButtons)
        : this(mouseButtons.NegativeMouseButton, mouseButtons.PositiveMouseButton) { }

    public Axis(
        GamepadButton negativeGamepadButton,
        GamepadButton positiveGamepadButton,
        Gamepads gamepads = Gamepads.All
    )
    {
        Type = AxisType.GamepadButton;
        NegativeGamepadButton = negativeGamepadButton;
        PositiveGamepadButton = positiveGamepadButton;
        Gamepads = gamepads;
    }

    public Axis((GamepadButton NegativeGamepadButton, GamepadButton PositiveGamepadButton) gamepadButtons)
        : this(gamepadButtons.NegativeGamepadButton, gamepadButtons.PositiveGamepadButton) { }

    public Axis(
        (GamepadButton NegativeGamepadButton, GamepadButton PositiveGamepadButton, Gamepads Gamepads) gamepadButtons
    )
        : this(gamepadButtons.NegativeGamepadButton, gamepadButtons.PositiveGamepadButton, gamepadButtons.Gamepads) { }

    public Axis(GamepadAxis gamepadAxis, Gamepads gamepads, float deadZone = 0)
    {
        Type = AxisType.GamepadAxis;
        GamepadAxis = gamepadAxis;
        Gamepads = gamepads;
        DeadZone = deadZone;
    }

    public Axis(GamepadAxis gamepadAxis)
        // ReSharper disable once IntroduceOptionalParameters.Global
        : this(gamepadAxis, Gamepads.All) { }

    public Axis((GamepadAxis GamepadAxis, Gamepads Gamepads) gamepadAxis)
        : this(gamepadAxis.GamepadAxis, gamepadAxis.Gamepads) { }

    public Axis((GamepadAxis GamepadAxis, Gamepads Gamepads, float DeadZone) gamepadAxis)
        : this(gamepadAxis.GamepadAxis, gamepadAxis.Gamepads, gamepadAxis.DeadZone) { }

    public static implicit operator Axis((Key NegativeKey, Key PositiveKey) keys)
    {
        return new Axis(keys.NegativeKey, keys.PositiveKey);
    }

    public static implicit operator Axis(
        (MouseButton NegativeMouseButton, MouseButton PositiveMouseButton) mouseButtons
    )
    {
        return new Axis(mouseButtons.NegativeMouseButton, mouseButtons.PositiveMouseButton);
    }

    public static implicit operator Axis(
        (GamepadButton NegativeGamepadButton, GamepadButton PositiveGamepadButton) gamepadButtons
    )
    {
        return new Axis(gamepadButtons.NegativeGamepadButton, gamepadButtons.PositiveGamepadButton);
    }

    public static implicit operator Axis(
        (GamepadButton NegativeGamepadButton, GamepadButton PositiveGamepadButton, Gamepads Gamepads) gamepadButtons
    )
    {
        return new Axis(
            gamepadButtons.NegativeGamepadButton,
            gamepadButtons.PositiveGamepadButton,
            gamepadButtons.Gamepads
        );
    }

    public static implicit operator Axis(GamepadAxis gamepadAxis)
    {
        return new Axis(gamepadAxis);
    }

    public static implicit operator Axis((GamepadAxis GamepadAxis, Gamepads Gamepads) gamepadAxis)
    {
        return new Axis(gamepadAxis.GamepadAxis, gamepadAxis.Gamepads);
    }

    public static implicit operator Axis((GamepadAxis GamepadAxis, Gamepads Gamepads, float DeadZone) gamepadAxis)
    {
        return new Axis(gamepadAxis.GamepadAxis, gamepadAxis.Gamepads, gamepadAxis.DeadZone);
    }

    public int Position
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

    public float Magnitude
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

    public float RawMagnitude
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

    public object? Value
    {
        get
        {
            return Type switch
            {
                AxisType.Key => (NegativeKey, PositiveKey),
                AxisType.MouseButton => (NegativeMouseButton, PositiveMouseButton),
                AxisType.GamepadButton => (NegativeGamepadButton, PositiveGamepadButton, Gamepads),
                AxisType.GamepadAxis => (GamepadAxis, Gamepads, DeadZone),
                _ => null,
            };
        }
    }

    public bool TryGetValue(out (Key NegativeKey, Key PositiveKey) keys)
    {
        if (Type == AxisType.Key)
        {
            keys = (NegativeKey, PositiveKey);
            return true;
        }

        keys = default;
        return false;
    }

    public bool TryGetValue(out (MouseButton NegativeMouseButton, MouseButton PositiveMouseButton) mouseButtons)
    {
        if (Type == AxisType.MouseButton)
        {
            mouseButtons = (NegativeMouseButton, PositiveMouseButton);
            return true;
        }

        mouseButtons = default;
        return false;
    }

    public bool TryGetValue(
        out (GamepadButton NegativeGamepadButton, GamepadButton PositiveGamepadButton) gamepadButtons
    )
    {
        if (Type == AxisType.GamepadButton)
        {
            gamepadButtons = (NegativeGamepadButton, PositiveGamepadButton);
            return true;
        }

        gamepadButtons = default;
        return false;
    }

    public bool TryGetValue(
        out (GamepadButton NegativeGamepadButton, GamepadButton PositiveGamepadButton, Gamepads Gamepads) gamepadButtons
    )
    {
        if (Type == AxisType.GamepadButton)
        {
            gamepadButtons = (NegativeGamepadButton, PositiveGamepadButton, Gamepads);
            return true;
        }

        gamepadButtons = default;
        return false;
    }

    public bool TryGetValue(out GamepadAxis gamepadAxis)
    {
        if (Type == AxisType.GamepadAxis)
        {
            gamepadAxis = GamepadAxis;
            return true;
        }

        gamepadAxis = default;
        return false;
    }

    public bool TryGetValue(out (GamepadAxis GamepadAxis, Gamepads Gamepads) gamepadAxis)
    {
        if (Type == AxisType.GamepadAxis)
        {
            gamepadAxis = (GamepadAxis, Gamepads);
            return true;
        }

        gamepadAxis = default;
        return false;
    }

    public bool TryGetValue(out (GamepadAxis GamepadAxis, Gamepads Gamepads, float DeadZone) gamepadAxis)
    {
        if (Type == AxisType.GamepadAxis)
        {
            gamepadAxis = (GamepadAxis, Gamepads, DeadZone);
            return true;
        }

        gamepadAxis = default;
        return false;
    }

    public bool Equals(Axis other)
    {
        return Type switch
        {
            AxisType.Key => other.Type == AxisType.Key
                && other.NegativeKey == NegativeKey
                && other.PositiveKey == PositiveKey,
            AxisType.MouseButton => other.Type == AxisType.MouseButton
                && other.NegativeMouseButton == NegativeMouseButton
                && other.PositiveMouseButton == PositiveMouseButton,
            AxisType.GamepadButton => other.Type == AxisType.GamepadButton
                && other.NegativeGamepadButton == NegativeGamepadButton
                && other.PositiveGamepadButton == PositiveGamepadButton
                && other.Gamepads == Gamepads,
            AxisType.GamepadAxis => other.Type == AxisType.GamepadAxis
                && other.GamepadAxis == GamepadAxis
                && other.Gamepads == Gamepads
                && other.DeadZone.Equals(DeadZone),
            _ => other.Type == Type,
        };
    }

    public override int GetHashCode()
    {
        return Type switch
        {
            AxisType.Key => HashCode.Combine(Type, NegativeKey, PositiveKey),
            AxisType.MouseButton => HashCode.Combine(Type, NegativeMouseButton, PositiveMouseButton),
            AxisType.GamepadButton => HashCode.Combine(Type, NegativeGamepadButton, PositiveGamepadButton, Gamepads),
            AxisType.GamepadAxis => HashCode.Combine(Type, GamepadAxis, Gamepads, DeadZone),
            _ => Type.GetHashCode(),
        };
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
