using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Vigilance.Logging;
using ZLinq;

namespace Vigilance.Input;

[StructLayout(LayoutKind.Explicit)]
[Union]
public readonly record struct Button : IUnion
{
    private string InvalidMessage => $"{nameof(Button)} contains a {Type}.";

    [field: FieldOffset(0)]
    public ButtonType Type { get; }

    [field: FieldOffset(1)]
    public Key Key
    {
        get => Type == ButtonType.Key ? field : throw new InvalidOperationException(InvalidMessage);
        init => field = Type == ButtonType.Key ? value : throw new InvalidOperationException(InvalidMessage);
    }

    [field: FieldOffset(1)]
    public MouseButton MouseButton
    {
        get => Type == ButtonType.MouseButton ? field : throw new InvalidOperationException(InvalidMessage);
        init => field = Type == ButtonType.MouseButton ? value : throw new InvalidOperationException(InvalidMessage);
    }

    [field: FieldOffset(1)]
    public GamepadButton GamepadButton
    {
        get => Type == ButtonType.GamepadButton ? field : throw new InvalidOperationException(InvalidMessage);
        init => field = Type == ButtonType.GamepadButton ? value : throw new InvalidOperationException(InvalidMessage);
    }

    [field: FieldOffset(2)]
    public Gamepads Gamepads
    {
        get => Type == ButtonType.GamepadButton ? field : throw new InvalidOperationException(InvalidMessage);
        init => field = Type == ButtonType.GamepadButton ? value : throw new InvalidOperationException(InvalidMessage);
    }

    public Button(Key key)
    {
        Type = ButtonType.Key;
        Key = key;
    }

    public Button(MouseButton mouseButton)
    {
        Type = ButtonType.MouseButton;
        MouseButton = mouseButton;
    }

    public Button(GamepadButton gamepadButton, Gamepads gamepads)
    {
        Type = ButtonType.GamepadButton;
        GamepadButton = gamepadButton;
        Gamepads = gamepads;
    }

    // ReSharper disable once IntroduceOptionalParameters.Global
    public Button(GamepadButton gamepadButton)
        : this(gamepadButton, Gamepads.All) { }

    public Button((GamepadButton GamepadButton, Gamepads Gamepads) gamepadButton)
        : this(gamepadButton.GamepadButton, gamepadButton.Gamepads) { }

    public static implicit operator Button(Key key)
    {
        return new Button(key);
    }

    public static implicit operator Button(MouseButton mouseButton)
    {
        return new Button(mouseButton);
    }

    public static implicit operator Button(GamepadButton gamepadButton)
    {
        return new Button(gamepadButton);
    }

    public static implicit operator Button((GamepadButton GamepadButton, Gamepads Gamepads) gamepadButton)
    {
        return new Button(gamepadButton);
    }

    public bool IsDown
    {
        get
        {
            switch (Type)
            {
                case ButtonType.Key:
                    return Keyboard.IsKeyDown(Key);
                case ButtonType.MouseButton:
                    return Mouse.IsButtonDown(MouseButton);
                case ButtonType.GamepadButton:
                    foreach (var gamepad in Gamepads.AsValueEnumerable())
                        if (gamepad.IsButtonDown(GamepadButton))
                            return true;
                    break;
            }

            return false;
        }
    }

    public bool IsUp => !IsDown;

    public bool IsPressed
    {
        get
        {
            switch (Type)
            {
                case ButtonType.Key:
                    return Keyboard.IsKeyPressed(Key);
                case ButtonType.MouseButton:
                    return Mouse.IsButtonPressed(MouseButton);
                case ButtonType.GamepadButton:
                    foreach (var gamepad in Gamepads.AsValueEnumerable())
                        if (gamepad.IsButtonPressed(GamepadButton))
                            return true;
                    break;
            }

            return false;
        }
    }

    public bool IsReleased
    {
        get
        {
            switch (Type)
            {
                case ButtonType.Key:
                    return Keyboard.IsKeyReleased(Key);
                case ButtonType.MouseButton:
                    return Mouse.IsButtonReleased(MouseButton);
                case ButtonType.GamepadButton:
                    foreach (var gamepad in Gamepads.AsValueEnumerable())
                        if (gamepad.IsButtonReleased(GamepadButton))
                            return true;
                    break;
            }

            return false;
        }
    }

    public object? Value
    {
        get
        {
            return Type switch
            {
                ButtonType.Key => Key,
                ButtonType.MouseButton => MouseButton,
                ButtonType.GamepadButton => (GamepadButton, Gamepads),
                _ => null,
            };
        }
    }

    public bool TryGetValue(out Key key)
    {
        if (Type == ButtonType.Key)
        {
            key = Key;
            return true;
        }

        key = default;
        return false;
    }

    public bool TryGetValue(out MouseButton mouseButton)
    {
        if (Type == ButtonType.MouseButton)
        {
            mouseButton = MouseButton;
            return true;
        }

        mouseButton = default;
        return false;
    }

    public bool TryGetValue(out GamepadButton gamepadButton)
    {
        if (Type == ButtonType.GamepadButton)
        {
            gamepadButton = GamepadButton;
            return true;
        }

        gamepadButton = default;
        return false;
    }

    public bool TryGetValue(out (GamepadButton GamepadButton, Gamepads Gamepads) gamepadButton)
    {
        if (Type == ButtonType.GamepadButton)
        {
            gamepadButton = (GamepadButton, Gamepads);
            return true;
        }

        gamepadButton = default;
        return false;
    }

    public bool Equals(Button other)
    {
        return Type switch
        {
            ButtonType.Key => other.Type == ButtonType.Key && other.Key == Key,
            ButtonType.MouseButton => other.Type == ButtonType.MouseButton && other.MouseButton == MouseButton,
            ButtonType.GamepadButton => other.Type == ButtonType.GamepadButton
                && other.GamepadButton == GamepadButton
                && other.Gamepads == Gamepads,
            _ => other.Type == Type,
        };
    }

    public override int GetHashCode()
    {
        return Type switch
        {
            ButtonType.Key => HashCode.Combine(Type, Key),
            ButtonType.MouseButton => HashCode.Combine(Type, MouseButton),
            ButtonType.GamepadButton => HashCode.Combine(Type, GamepadButton, Gamepads),
            _ => Type.GetHashCode(),
        };
    }

    public override string ToString()
    {
        return Type switch
        {
            ButtonType.Key => ObjectPrinter.Print(this, ObjectPrinter.Include([nameof(Type), nameof(Key)])),
            ButtonType.MouseButton => ObjectPrinter.Print(
                this,
                ObjectPrinter.Include([nameof(Type), nameof(MouseButton)])
            ),
            ButtonType.GamepadButton => ObjectPrinter.Print(
                this,
                ObjectPrinter.Include([nameof(Type), nameof(GamepadButton), nameof(Gamepads)])
            ),
            _ => ObjectPrinter.Print(this, ObjectPrinter.Include([nameof(Type)])),
        };
    }
}
