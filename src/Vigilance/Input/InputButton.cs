using Vigilance.Collections;
using ZLinq;

namespace Vigilance.Input;

public sealed class InputButton
{
    public List<GamepadButton> GamepadButtons { get; set; } = [];
    public List<Gamepad> Gamepads { get; set; } = Gamepad.Gamepads.AsValueEnumerable().ToList();
    public List<Key> Keys { get; set; } = [];
    public List<MouseButton> MouseButtons { get; set; } = [];

    public bool IsDown =>
        Keys.AsValueEnumerable().Any(Keyboard.IsKeyDown)
        || MouseButtons.AsValueEnumerable().Any(Mouse.IsButtonDown)
        || Gamepads
            .AsValueEnumerable()
            .Cross(GamepadButtons.AsValueEnumerable())
            .Any(x => x.Left.IsButtonDown(x.Right));

    public bool IsUp => !IsDown;

    public bool IsPressed =>
        Keys.AsValueEnumerable().Any(Keyboard.IsKeyPressed)
        || MouseButtons.AsValueEnumerable().Any(Mouse.IsButtonPressed)
        || Gamepads
            .AsValueEnumerable()
            .Cross(GamepadButtons.AsValueEnumerable())
            .Any(x => x.Left.IsButtonPressed(x.Right));

    public bool IsReleased =>
        Keys.AsValueEnumerable().Any(Keyboard.IsKeyReleased)
        || MouseButtons.AsValueEnumerable().Any(Mouse.IsButtonReleased)
        || Gamepads
            .AsValueEnumerable()
            .Cross(GamepadButtons.AsValueEnumerable())
            .Any(x => x.Left.IsButtonReleased(x.Right));

    public static implicit operator InputButton(Key key)
    {
        return new InputButton { Keys = [key] };
    }
}
