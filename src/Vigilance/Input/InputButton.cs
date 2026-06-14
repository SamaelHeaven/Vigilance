using Vigilance.Collections;
using ZLinq;

namespace Vigilance.Input;

public sealed class InputButton
{
    public List<GamepadButton> GamepadButtons { get; set; } = [];
    public List<Gamepad> Gamepads { get; set; } = Gamepad.Gamepads.AsValueEnumerable().ToList();
    public List<Key> Keys { get; set; } = [];
    public List<MouseButton> MouseButtons { get; set; } = [];

    public bool IsDown => IsKeyDown || IsMouseDown || IsGamepadDown;

    public bool IsUp => !IsDown;

    public bool IsPressed => IsKeyPressed || IsMousePressed || IsGamepadPressed;

    public bool IsReleased => IsKeyReleased || IsMouseReleased || IsGamepadReleased;

    public bool IsKeyDown => Keys.AsValueEnumerable().Any(Keyboard.IsKeyDown);

    public bool IsMouseDown => MouseButtons.AsValueEnumerable().Any(Mouse.IsButtonDown);

    public bool IsGamepadDown =>
        Gamepads.AsValueEnumerable().Cross(GamepadButtons.AsValueEnumerable()).Any(x => x.Left.IsButtonDown(x.Right));

    public bool IsKeyUp => !IsKeyDown;

    public bool IsMouseUp => !IsMouseDown;

    public bool IsGamepadUp => !IsGamepadDown;

    public bool IsKeyPressed => Keys.AsValueEnumerable().Any(Keyboard.IsKeyPressed);

    public bool IsMousePressed => MouseButtons.AsValueEnumerable().Any(Mouse.IsButtonPressed);

    public bool IsGamepadPressed =>
        Gamepads
            .AsValueEnumerable()
            .Cross(GamepadButtons.AsValueEnumerable())
            .Any(x => x.Left.IsButtonPressed(x.Right));

    public bool IsKeyReleased => Keys.AsValueEnumerable().Any(Keyboard.IsKeyReleased);

    public bool IsMouseReleased => MouseButtons.AsValueEnumerable().Any(Mouse.IsButtonReleased);

    public bool IsGamepadReleased =>
        Gamepads
            .AsValueEnumerable()
            .Cross(GamepadButtons.AsValueEnumerable())
            .Any(x => x.Left.IsButtonReleased(x.Right));

    public static implicit operator InputButton(Key key)
    {
        return new InputButton { Keys = [key] };
    }
}
