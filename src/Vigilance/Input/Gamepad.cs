using Raylib_cs;
using Vigilance.Core;

namespace Vigilance.Input;

public sealed class Gamepad
{
    private const int MaxGamepads = 4;
    private const string DefaultName = "Unknown gamepad";
    private static readonly Gamepad[] GamepadArray = GetGamepads();
    private static readonly GamepadButton[] ButtonValues = Enum.GetValues<GamepadButton>();
    private static readonly GamepadAxis[] AxisValues = Enum.GetValues<GamepadAxis>();
    private readonly Dictionary<GamepadAxis, float> _axes;
    private readonly List<GamepadButton> _currentButtons = [];
    private readonly List<GamepadButton> _downButtons = [];
    private readonly List<GamepadButton> _pressedButtons = [];
    private readonly List<GamepadButton> _releasedButtons = [];
    private readonly List<GamepadButton> _upButtons = [];

    private Gamepad(int id)
    {
        Id = id;
        Connected = false;
        Name = DefaultName;
        _axes = new Dictionary<GamepadAxis, float>();
        foreach (var axis in Enum.GetValues<GamepadAxis>())
            _axes.Add(axis, 0);
    }

    public static IReadOnlyList<Gamepad> Gamepads { get; } = GamepadArray.AsReadOnly();
    public int Id { get; }

    public static Gamepad First => GamepadArray[0];
    public static Gamepad Second => GamepadArray[1];
    public static Gamepad Third => GamepadArray[2];
    public static Gamepad Fourth => GamepadArray[3];

    public IReadOnlyList<GamepadButton> DownButtons => _downButtons.AsReadOnly();
    public IReadOnlyList<GamepadButton> UpButtons => _upButtons.AsReadOnly();
    public IReadOnlyList<GamepadButton> PressedButtons => _pressedButtons.AsReadOnly();
    public IReadOnlyList<GamepadButton> ReleasedButtons => _releasedButtons.AsReadOnly();
    public IReadOnlyDictionary<GamepadAxis, float> Axes => _axes.AsReadOnly();
    public bool Connected { get; private set; }
    public string Name { get; private set; }

    internal static void UpdateAll()
    {
        foreach (var gamepad in GamepadArray)
            gamepad.Update();
    }

    private static Gamepad[] GetGamepads()
    {
        var gamepads = new Gamepad[MaxGamepads];
        for (var i = 0; i < MaxGamepads; i++)
            gamepads[i] = new Gamepad(i);
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
        Connected = IsConnected();
        Name = GetName();
        if (!Game.Focused || !Connected)
        {
            Reset();
            return;
        }

        UpdateState();
    }

    private void Reset()
    {
        _upButtons.Clear();
        _upButtons.AddRange(ButtonValues);
        _downButtons.Clear();
        _pressedButtons.Clear();
        _releasedButtons.Clear();
        foreach (var axis in _axes)
            _axes[axis.Key] = 0;
    }

    private void UpdateState()
    {
        _currentButtons.Clear();
        foreach (var button in ButtonValues)
            if (IsButtonDown(Id, button))
                _currentButtons.Add(button);
        _pressedButtons.Clear();
        _pressedButtons.AddRange(_currentButtons);
        _pressedButtons.RemoveAll(button => _downButtons.Contains(button));
        _releasedButtons.Clear();
        _releasedButtons.AddRange(_downButtons);
        _releasedButtons.RemoveAll(button => _currentButtons.Contains(button));
        _downButtons.Clear();
        _downButtons.AddRange(_currentButtons);
        _upButtons.Clear();
        _upButtons.AddRange(ButtonValues);
        _upButtons.RemoveAll(button => _currentButtons.Contains(button));
        foreach (var axis in AxisValues)
            _axes[axis] = GetGamepadAxis(Id, axis);
    }

    private bool IsConnected()
    {
        return Platform.Web.IsCurrent()
            ? JSEngine.Run($"!!navigator.getGamepads()[{Id}]")
            : Raylib.IsGamepadAvailable(Id);
    }

    private string GetName()
    {
        if (!Connected)
            return DefaultName;
        return Platform.Web.IsCurrent()
            ? JSEngine.Run($"navigator.getGamepads()[{Id}]?.id ?? {DefaultName.ToJson()}")
            : Raylib.GetGamepadName_(Id);
    }

    private static bool IsButtonDown(int id, GamepadButton button)
    {
        return Platform.Web.IsCurrent()
            ? JSEngine.Run($"navigator.getGamepads()[{id}]?.buttons[{button.GetJSValue()}]?.pressed ?? false")
            : Raylib.IsGamepadButtonDown(id, (Raylib_cs.GamepadButton)button);
    }

    private static float GetGamepadAxis(int id, GamepadAxis axis)
    {
        return Platform.Web.IsCurrent()
            ? JSEngine.Run($"navigator.getGamepads()[{id}]?.axes[{axis.GetJSValue()}] ?? 0")
            : Raylib.GetGamepadAxisMovement(id, (Raylib_cs.GamepadAxis)axis);
    }
}
