using Vigilance.Collections;
using ZLinq;

namespace Vigilance.Input;

public sealed class InputButton
{
    public List<GamepadButton> GamepadButtons { get; set; } = [];
    public List<Gamepad> Gamepads { get; set; } = Gamepad.Gamepads.AsValueEnumerable().ToList();
    public List<Key> Keys { get; set; } = [];

    public bool IsDown =>
        Keys.AsValueEnumerable().Any(Keyboard.IsKeyDown)
        || Gamepads
            .AsValueEnumerable()
            .Pair(GamepadButtons.AsValueEnumerable())
            .Any(pair => pair.First.IsButtonDown(pair.Second));

    public bool IsUp => !IsDown;

    public bool IsPressed =>
        Keys.AsValueEnumerable().Any(Keyboard.IsKeyPressed)
        || Gamepads
            .AsValueEnumerable()
            .Pair(GamepadButtons.AsValueEnumerable())
            .Any(pair => pair.First.IsButtonPressed(pair.Second));

    public bool IsReleased =>
        Keys.AsValueEnumerable().Any(Keyboard.IsKeyReleased)
        || Gamepads
            .AsValueEnumerable()
            .Pair(GamepadButtons.AsValueEnumerable())
            .Any(pair => pair.First.IsButtonReleased(pair.Second));

    public static implicit operator InputButton(Key key)
    {
        return new InputButton { Keys = [key] };
    }
}
