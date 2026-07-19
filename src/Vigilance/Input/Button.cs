using System.Runtime.InteropServices;
using Vigilance.Logging;
using ZLinq;

namespace Vigilance.Input;

[StructLayout(LayoutKind.Explicit)]
public readonly record struct Button
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

    public Button(ButtonType type)
    {
        Type = type;
    }

    public static Button From(Key key)
    {
        return new Button(ButtonType.Key) { Key = key };
    }

    public static Button From(MouseButton mouseButton)
    {
        return new Button(ButtonType.MouseButton) { MouseButton = mouseButton };
    }

    public static Button From(GamepadButton gamepadButton, Gamepads gamepads = Gamepads.All)
    {
        return new Button(ButtonType.GamepadButton) { GamepadButton = gamepadButton, Gamepads = gamepads };
    }

    public static implicit operator Button(Key key)
    {
        return From(key);
    }

    public static implicit operator Button(MouseButton mouseButton)
    {
        return From(mouseButton);
    }

    public static implicit operator Button(GamepadButton gamepadButton)
    {
        return From(gamepadButton);
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
