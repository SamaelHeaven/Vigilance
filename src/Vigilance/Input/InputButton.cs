using Vigilance.Collections;
using ZLinq;

namespace Vigilance.Input;

public sealed class InputButton
{
    private ValueList<GamepadButton> _gamepadButtons = [];
    private ValueList<Gamepad> _gamepads = Gamepad.Gamepads.AsValueEnumerable().ToValueList();
    private ValueList<Key> _keys = [];
    private ValueList<MouseButton> _mouseButtons = [];

    public ValueListRef<GamepadButton> GamepadButtons
    {
        get => _gamepadButtons.AsRef();
        set => value.CopyTo(ref _gamepadButtons);
    }

    public ValueListRef<Gamepad> Gamepads
    {
        get => _gamepads.AsRef();
        set => value.CopyTo(ref _gamepads);
    }

    public ValueListRef<Key> Keys
    {
        get => _keys.AsRef();
        set => value.CopyTo(ref _keys);
    }

    public ValueListRef<MouseButton> MouseButtons
    {
        get => _mouseButtons.AsRef();
        set => value.CopyTo(ref _mouseButtons);
    }

    public bool IsDown => IsKeyDown || IsMouseDown || IsGamepadDown;

    public bool IsUp => !IsDown;

    public bool IsPressed => IsKeyPressed || IsMousePressed || IsGamepadPressed;

    public bool IsReleased => IsKeyReleased || IsMouseReleased || IsGamepadReleased;

    public bool IsKeyDown => _keys.AsValueEnumerable().Any(Keyboard.IsKeyDown);

    public bool IsMouseDown => _mouseButtons.AsValueEnumerable().Any(Mouse.IsButtonDown);

    public bool IsGamepadDown =>
        _gamepads.AsValueEnumerable().Cross(_gamepadButtons.AsValueEnumerable()).Any(x => x.Left.IsButtonDown(x.Right));

    public bool IsKeyUp => !IsKeyDown;

    public bool IsMouseUp => !IsMouseDown;

    public bool IsGamepadUp => !IsGamepadDown;

    public bool IsKeyPressed => _keys.AsValueEnumerable().Any(Keyboard.IsKeyPressed);

    public bool IsMousePressed => _mouseButtons.AsValueEnumerable().Any(Mouse.IsButtonPressed);

    public bool IsGamepadPressed =>
        _gamepads
            .AsValueEnumerable()
            .Cross(_gamepadButtons.AsValueEnumerable())
            .Any(x => x.Left.IsButtonPressed(x.Right));

    public bool IsKeyReleased => _keys.AsValueEnumerable().Any(Keyboard.IsKeyReleased);

    public bool IsMouseReleased => _mouseButtons.AsValueEnumerable().Any(Mouse.IsButtonReleased);

    public bool IsGamepadReleased =>
        _gamepads
            .AsValueEnumerable()
            .Cross(_gamepadButtons.AsValueEnumerable())
            .Any(x => x.Left.IsButtonReleased(x.Right));

    public static implicit operator InputButton(GamepadButton button)
    {
        return new InputButton { GamepadButtons = [button] };
    }

    public static implicit operator InputButton(Key key)
    {
        return new InputButton { Keys = [key] };
    }

    public static implicit operator InputButton(MouseButton button)
    {
        return new InputButton { MouseButtons = [button] };
    }
}
