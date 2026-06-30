using Vigilance.Collections;
using ZLinq;

namespace Vigilance.Input;

public sealed class InputButton
{
    private ValueList<GamepadButton> _gamepadButtons;
    private ValueList<Gamepad> _gamepads;
    private ValueList<Key> _keys;
    private ValueList<MouseButton> _mouseButtons;

    public InputButton(
        in ReadOnlySpan<Key> keys = default,
        in ReadOnlySpan<MouseButton> mouseButtons = default,
        in ReadOnlySpan<GamepadButton> gamepadButtons = default
    )
        : this(Gamepad.Gamepads, keys, mouseButtons, gamepadButtons) { }

    public InputButton(
        in ReadOnlySpan<Gamepad> gamepads,
        in ReadOnlySpan<Key> keys = default,
        in ReadOnlySpan<MouseButton> mouseButtons = default,
        in ReadOnlySpan<GamepadButton> gamepadButtons = default
    )
    {
        _gamepads = gamepads.AsValueEnumerable().ToValueList();
        _keys = keys.AsValueEnumerable().ToValueList();
        _mouseButtons = mouseButtons.AsValueEnumerable().ToValueList();
        _gamepadButtons = gamepadButtons.AsValueEnumerable().ToValueList();
    }

    public ValueListRef<Gamepad> Gamepads => _gamepads.AsRef();
    public ValueListRef<Key> Keys => _keys.AsRef();
    public ValueListRef<MouseButton> MouseButtons => _mouseButtons.AsRef();
    public ValueListRef<GamepadButton> GamepadButtons => _gamepadButtons.AsRef();

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

    public static implicit operator InputButton(Key key)
    {
        return new InputButton([key]);
    }
}
