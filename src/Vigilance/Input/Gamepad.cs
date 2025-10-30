using Raylib_cs.BleedingEdge;
using Vigilance.Core;

namespace Vigilance.Input;

public sealed class Gamepad
{
    private const int MaxGamepads = 4;
    private const string DefaultName = "Unknown gamepad";
    private static readonly List<Gamepad> _gamepadList = GetGamepads();
    private static readonly GamepadButton[] _buttonValues = Enum.GetValues<GamepadButton>();
    private static readonly GamepadAxis[] _axisValues = Enum.GetValues<GamepadAxis>();
    private readonly Dictionary<GamepadAxis, float> _axes;
    private readonly List<GamepadButton> _currentButtons = [];
    private readonly List<GamepadButton> _downButtons = [];
    private readonly List<GamepadButton> _pressedButtons = [];
    private readonly List<GamepadButton> _releasedButtons = [];
    private readonly List<GamepadButton> _upButtons = [];

    private Gamepad(int id)
    {
        Id = id;
        IsConnected = false;
        Name = DefaultName;
        _axes = new Dictionary<GamepadAxis, float>();
        foreach (var axis in Enum.GetValues<GamepadAxis>())
            _axes.Add(axis, 0);
    }

    public static ListView<Gamepad> Gamepads => _gamepadList;
    public int Id { get; }

    public static Gamepad First => _gamepadList[0];
    public static Gamepad Second => _gamepadList[1];
    public static Gamepad Third => _gamepadList[2];
    public static Gamepad Fourth => _gamepadList[3];

    public ListView<GamepadButton> DownButtons => _downButtons;
    public ListView<GamepadButton> UpButtons => _upButtons;
    public ListView<GamepadButton> PressedButtons => _pressedButtons;
    public ListView<GamepadButton> ReleasedButtons => _releasedButtons;
    public DictionaryView<GamepadAxis, float> Axes => _axes;
    public bool IsConnected { get; private set; }
    public string Name { get; private set; }

    internal static void UpdateAll()
    {
        foreach (var gamepad in _gamepadList)
            gamepad.Update();
    }

    private static List<Gamepad> GetGamepads()
    {
        var gamepads = new List<Gamepad>(MaxGamepads);
        for (var i = 0; i < MaxGamepads; i++)
            gamepads.Add(new Gamepad(i));
        return gamepads;
    }

    public bool IsButtonDown(GamepadButton button)
    {
        return _downButtons.Contains(button);
    }

    public bool IsButtonUp(GamepadButton button)
    {
        return _upButtons.Contains(button);
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
        IsConnected = GetIsConnected();
        Name = GetName();
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
        foreach (var axis in _axes)
            _axes[axis.Key] = 0;
    }

    private void UpdateState()
    {
        _currentButtons.Clear();
        foreach (var button in _buttonValues)
            if (IsButtonDown(Id, button))
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
            _axes[axis] = GetGamepadAxis(Id, axis);
    }

    private bool GetIsConnected()
    {
        return Platform.Current switch
        {
            Platform.Web => JSEngine.Eval($"!!navigator.getGamepads()[{Id}]"),
            _ => Raylib.IsGamepadAvailable(Id),
        };
    }

    private unsafe string GetName()
    {
        if (!IsConnected)
            return DefaultName;
        return Platform.Current switch
        {
            Platform.Web => JSEngine.Eval($"navigator.getGamepads()[{Id}]?.id ?? {DefaultName.ToJson()}"),
            _ => Utf8Buffer.GetString(Raylib.GetGamepadName(Id), DefaultName),
        };
    }

    private static bool IsButtonDown(int id, GamepadButton button)
    {
        return Platform.Current switch
        {
            Platform.Web => JSEngine.Eval(
                $"navigator.getGamepads()[{id}]?.buttons[{button.JSValue}]?.pressed ?? false"
            ),
            _ => Raylib.IsGamepadButtonDown(id, (Raylib_cs.BleedingEdge.GamepadButton)button),
        };
    }

    private static float GetGamepadAxis(int id, GamepadAxis axis)
    {
        return Platform.Current switch
        {
            Platform.Web => JSEngine.Eval($"navigator.getGamepads()[{id}]?.axes[{axis.JSValue}] ?? 0"),
            _ => Raylib.GetGamepadAxisMovement(id, (Raylib_cs.BleedingEdge.GamepadAxis)axis),
        };
    }
}
