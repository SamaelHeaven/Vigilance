using Vigilance.Collections;
using ZLinq;

namespace Vigilance.Input;

public sealed class InputButton
{
    private ValueList<GamepadButton> _gamepadButtons = [];
    private ValueList<Gamepad> _gamepads = Gamepad.Gamepads.AsValueEnumerable().ToValueList();
    private ValueList<Key> _keys = [];
    private ValueList<MouseButton> _mouseButtons = [];

    public InputButton(
        in ReadOnlySpan<Key> keys = default,
        in ReadOnlySpan<MouseButton> mouseButtons = default,
        in ReadOnlySpan<GamepadButton> gamepadButtons = default,
        in ReadOnlySpan<Gamepad> gamepads = default
    )
    {
        if (!keys.IsEmpty)
            _keys = keys.AsValueEnumerable().ToValueList();
        if (!mouseButtons.IsEmpty)
            _mouseButtons = mouseButtons.AsValueEnumerable().ToValueList();
        if (!gamepadButtons.IsEmpty)
            _gamepadButtons = gamepadButtons.AsValueEnumerable().ToValueList();
        if (!gamepads.IsEmpty)
            _gamepads = gamepads.AsValueEnumerable().ToValueList();
    }

    public ValueListRef<GamepadButton> GamepadButtons => _gamepadButtons;
    public ValueListRef<Gamepad> Gamepads => _gamepads;
    public ValueListRef<Key> Keys => _keys;
    public ValueListRef<MouseButton> MouseButtons => _mouseButtons;

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
        return new InputButton(keys: [key]);
    }
}
