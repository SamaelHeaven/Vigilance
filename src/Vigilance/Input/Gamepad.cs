using System.Runtime.InteropServices;
using Raylib_cs.BleedingEdge;
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

    public static EnumerableArray<Gamepad> Gamepads => GamepadArray;
    public int Id { get; }

    public static Gamepad First => GamepadArray[0];
    public static Gamepad Second => GamepadArray[1];
    public static Gamepad Third => GamepadArray[2];
    public static Gamepad Fourth => GamepadArray[3];

    public EnumerableList<GamepadButton> DownButtons => _downButtons;
    public EnumerableList<GamepadButton> UpButtons => _upButtons;
    public EnumerableList<GamepadButton> PressedButtons => _pressedButtons;
    public EnumerableList<GamepadButton> ReleasedButtons => _releasedButtons;
    public EnumerableDictionary<GamepadAxis, float> Axes => _axes;
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
        if (!Display.Focused || !Connected)
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
        _pressedButtons.RemoveAll(_downButtons.Contains);
        _releasedButtons.Clear();
        _releasedButtons.AddRange(_downButtons);
        _releasedButtons.RemoveAll(_currentButtons.Contains);
        _downButtons.Clear();
        _downButtons.AddRange(_currentButtons);
        _upButtons.Clear();
        _upButtons.AddRange(ButtonValues);
        _upButtons.RemoveAll(_currentButtons.Contains);
        foreach (var axis in AxisValues)
            _axes[axis] = GetGamepadAxis(Id, axis);
    }

    private bool IsConnected()
    {
        return Game.Platform switch
        {
            Platform.Web => JSEngine.Eval($"!!navigator.getGamepads()[{Id}]"),
            _ => Raylib.IsGamepadAvailable(Id),
        };
    }

    private unsafe string GetName()
    {
        if (!Connected)
            return DefaultName;
        return Game.Platform switch
        {
            Platform.Web => JSEngine.Eval($"navigator.getGamepads()[{Id}]?.id ?? {DefaultName.ToJson()}"),
            _ => Marshal.PtrToStringUTF8((nint)Raylib.GetGamepadName(Id)) ?? DefaultName,
        };
    }

    private static bool IsButtonDown(int id, GamepadButton button)
    {
        return Game.Platform switch
        {
            Platform.Web => JSEngine.Eval(
                $"navigator.getGamepads()[{id}]?.buttons[{button.GetJSValue()}]?.pressed ?? false"
            ),
            _ => Raylib.IsGamepadButtonDown(id, (Raylib_cs.BleedingEdge.GamepadButton)button),
        };
    }

    private static float GetGamepadAxis(int id, GamepadAxis axis)
    {
        return Game.Platform switch
        {
            Platform.Web => JSEngine.Eval($"navigator.getGamepads()[{id}]?.axes[{axis.GetJSValue()}] ?? 0"),
            _ => Raylib.GetGamepadAxisMovement(id, (Raylib_cs.BleedingEdge.GamepadAxis)axis),
        };
    }
}
