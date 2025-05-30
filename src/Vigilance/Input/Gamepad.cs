using Raylib_cs;
using Vigilance.Core;

namespace Vigilance.Input;

public sealed class Gamepad
{
    private const int NbGamepads = 8;
    private const string DefaultName = "Unknown gamepad";
    private static readonly GamepadButton[] ButtonValues = Enum.GetValues<GamepadButton>().ToArray();
    private static readonly GamepadAxis[] AxisValues = Enum.GetValues<GamepadAxis>().ToArray();
    private readonly Dictionary<GamepadAxis, float> _axes;
    private readonly List<GamepadButton> _currentButtons = [];
    private readonly List<GamepadButton> _downButtons = [];
    private readonly List<GamepadButton> _pressedButtons = [];
    private readonly List<GamepadButton> _releasedButtons = [];
    private readonly List<GamepadButton> _upButtons = [];

    private Gamepad(int id)
    {
        Id = id;
        Connected = Game.Running ? Raylib.IsGamepadAvailable(Id) : false;
        _axes = new Dictionary<GamepadAxis, float>();
        foreach (var axis in Enum.GetValues<GamepadAxis>())
            _axes.Add(axis, 0);
    }

    public static IReadOnlyList<Gamepad> Gamepads { get; } = GetGamepads();
    public int Id { get; }

    public static Gamepad First => Gamepads[0];
    public static Gamepad Second => Gamepads[1];
    public static Gamepad Third => Gamepads[2];
    public static Gamepad Fourth => Gamepads[3];
    public static Gamepad Fifth => Gamepads[4];
    public static Gamepad Sixth => Gamepads[5];
    public static Gamepad Seventh => Gamepads[6];
    public static Gamepad Eighth => Gamepads[7];

    public IReadOnlyList<GamepadButton> DownButtons => _downButtons.AsReadOnly();
    public IReadOnlyList<GamepadButton> UpButtons => _upButtons.AsReadOnly();
    public IReadOnlyList<GamepadButton> PressedButtons => _pressedButtons.AsReadOnly();
    public IReadOnlyList<GamepadButton> ReleasedButtons => _releasedButtons.AsReadOnly();
    public IReadOnlyDictionary<GamepadAxis, float> Axes => _axes.AsReadOnly();
    public bool Connected { get; private set; }

    public string Name => !Connected ? DefaultName : Raylib.GetGamepadName_(Id);

    internal static void UpdateAll()
    {
        foreach (var gamepad in Gamepads)
            gamepad.Update();
    }

    private static Gamepad[] GetGamepads()
    {
        var gamepads = new Gamepad[NbGamepads];
        for (var i = 0; i < NbGamepads; i++)
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
        Connected = Raylib.IsGamepadAvailable(Id);
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
            if (Raylib.IsGamepadButtonDown(Id, (Raylib_cs.GamepadButton)button))
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
            _axes[axis] = Raylib.GetGamepadAxisMovement(Id, (Raylib_cs.GamepadAxis)axis);
    }
}
