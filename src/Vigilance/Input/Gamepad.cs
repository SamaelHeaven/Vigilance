using Raylib_cs;
using Vigilance.Collections;
using Vigilance.Core;
using Vigilance.Logging;

namespace Vigilance.Input;

public sealed unsafe class Gamepad
{
    private const int MaxGamepads = 4;
    private const string DefaultName = "Unknown gamepad";
    private static readonly GamepadButton[] _buttonValues = Enum.GetValues<GamepadButton>();
    private static readonly GamepadAxis[] _axisValues = Enum.GetValues<GamepadAxis>();
    private static readonly Gamepad[] _gamepads = GetGamepads();
    private ValueDictionary<GamepadAxis, float> _axes;
    private ValueList<GamepadButton> _currentButtons = [];
    private ValueList<GamepadButton> _downButtons = [];
    private ValueList<GamepadButton> _pressedButtons = [];
    private ValueList<GamepadButton> _releasedButtons = [];
    private ValueList<GamepadButton> _upButtons = [];

    private Gamepad(int id)
    {
        Id = id;
        _axes = new ValueDictionary<GamepadAxis, float>();
        foreach (var axis in _axisValues)
            _axes.Add(axis, 0);
    }

    public static ArrayView<Gamepad> Gamepads => _gamepads;
    public int Id { get; }

    public static Gamepad First => _gamepads[0];
    public static Gamepad Second => _gamepads[1];
    public static Gamepad Third => _gamepads[2];
    public static Gamepad Fourth => _gamepads[3];

    public ValueListView<GamepadButton> DownButtons => _downButtons;
    public ValueListView<GamepadButton> UpButtons => _upButtons;
    public ValueListView<GamepadButton> PressedButtons => _pressedButtons;
    public ValueListView<GamepadButton> ReleasedButtons => _releasedButtons;
    public ValueDictionaryView<GamepadAxis, float> Axes => _axes;
    public string Name { get; private set; } = DefaultName;
    public bool IsConnected { get; private set; } = false;

    internal static void UpdateAll()
    {
        foreach (var gamepad in _gamepads)
            gamepad.Update();
    }

    private static Gamepad[] GetGamepads()
    {
        var gamepads = new Gamepad[MaxGamepads];
        for (var i = 0; i < MaxGamepads; i++)
            gamepads[i] = new Gamepad(i);
        return gamepads;
    }

    public override string ToString()
    {
        return ObjectPrinter.Print(this, ObjectPrinter.Include([nameof(Id), nameof(Name), nameof(IsConnected)]));
    }

    public bool IsButtonDown(GamepadButton button)
    {
        return _downButtons.Contains(button);
    }

    public bool IsButtonUp(GamepadButton button)
    {
        return !_downButtons.Contains(button);
    }

    public bool IsButtonPressed(GamepadButton button)
    {
        return _pressedButtons.Contains(button);
    }

    public bool IsButtonReleased(GamepadButton button)
    {
        return _releasedButtons.Contains(button);
    }

    public float GetAxis(GamepadAxis axis)
    {
        return _axes[axis];
    }

    private void Update()
    {
        IsConnected = Raylib.IsGamepadAvailable(Id);
        Name = !IsConnected ? DefaultName : Utf8Ptr.GetString(Raylib.GetGamepadName(Id), DefaultName);
        if (!Display.Focused || !IsConnected)
        {
            Reset();
            return;
        }

        UpdateState();
    }

    private void Reset()
    {
        _upButtons.Clear();
        _upButtons.AddRange(_buttonValues);
        _downButtons.Clear();
        _pressedButtons.Clear();
        _releasedButtons.Clear();
        foreach (var axis in _axisValues)
            _axes[axis] = 0;
    }

    private void UpdateState()
    {
        _currentButtons.Clear();
        foreach (var button in _buttonValues)
            if (Raylib.IsGamepadButtonDown(Id, (Raylib_cs.GamepadButton)button))
                _currentButtons.Add(button);
        _pressedButtons.Clear();
        _pressedButtons.AddRange(_currentButtons);
        _pressedButtons.RemoveAll(_downButtons.Contains);
        _releasedButtons.Clear();
        _releasedButtons.AddRange(_downButtons);
        _releasedButtons.RemoveAll(_currentButtons.Contains);
        _downButtons.Clear();
        _downButtons.AddRange(_currentButtons);
        _upButtons.Clear();
        _upButtons.AddRange(_buttonValues);
        _upButtons.RemoveAll(_currentButtons.Contains);
        foreach (var axis in _axisValues)
            _axes[axis] = Raylib.GetGamepadAxisMovement(Id, (Raylib_cs.GamepadAxis)axis);
    }
}
