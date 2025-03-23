using Raylib_cs;
using Vigilance.Core;

namespace Vigilance.Input;

public sealed class Gamepad
{
    private const int NbGamepads = 8;
    private const string DefaultName = "Unknown gamepad";
    private static readonly GamepadButton[] ButtonValues = Enum.GetValues<GamepadButton>().ToArray();
    private static readonly GamepadAxis[] AxisValues = Enum.GetValues<GamepadAxis>().ToArray();
    public static readonly IReadOnlyList<Gamepad> Gamepads = GetGamepads();
    private readonly Dictionary<GamepadAxis, float> _axes;
    private readonly List<GamepadButton> _downButtons = [];
    private readonly List<GamepadButton> _pressedButtons = [];
    private readonly List<GamepadButton> _releasedButtons = [];
    private readonly List<GamepadButton> _upButtons = [];
    public readonly int Id;

    static Gamepad()
    {
        Game.EnsureRunning();
    }

    private Gamepad(int id)
    {
        Id = id;
        Connected = Raylib.IsGamepadAvailable(Id);
        _axes = new Dictionary<GamepadAxis, float>();
        foreach (var axis in Enum.GetValues<GamepadAxis>())
            _axes.Add(axis, 0);
    }

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

    public string Name
    {
        get
        {
            unsafe
            {
                return !Connected ? DefaultName : new string(Raylib.GetGamepadName(Id));
            }
        }
    }

    internal static void UpdateAll()
    {
        foreach (var gamepad in Gamepads)
            gamepad.Update();
    }

    private static Gamepad[] GetGamepads()
    {
        var gamepads = new List<Gamepad>();
        for (var i = 0; i < NbGamepads; i++)
            gamepads.Add(new Gamepad(i));

        return gamepads.ToArray();
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
        Reset();
        if (!Game.Focused || !Connected)
            return;
        UpdateButtons();
        UpdateAxes();
    }

    private void Reset()
    {
        Connected = Raylib.IsGamepadAvailable(Id);
        _upButtons.Clear();
        _upButtons.AddRange(ButtonValues);
        _downButtons.Clear();
        _pressedButtons.Clear();
        _releasedButtons.Clear();
        foreach (var axis in _axes)
            _axes[axis.Key] = 0;
    }

    private void UpdateButtons()
    {
        foreach (var button in ButtonValues)
        {
            if (Raylib.IsGamepadButtonDown(Id, (Raylib_cs.GamepadButton)button))
            {
                _downButtons.Add(button);
                _upButtons.Remove(button);
            }

            if (Raylib.IsGamepadButtonPressed(Id, (Raylib_cs.GamepadButton)button))
                _pressedButtons.Add(button);
            if (Raylib.IsGamepadButtonReleased(Id, (Raylib_cs.GamepadButton)button))
                _releasedButtons.Add(button);
        }
    }

    private void UpdateAxes()
    {
        foreach (var axis in AxisValues)
            _axes[axis] = Raylib.GetGamepadAxisMovement(Id, (Raylib_cs.GamepadAxis)axis);
    }
}
