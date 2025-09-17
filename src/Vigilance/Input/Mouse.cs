using Raylib_cs.BleedingEdge;
using Vigilance.Core;
using Vigilance.Math;
using Vector2 = Vigilance.Math.Vector2;

namespace Vigilance.Input;

public sealed class Mouse
{
    private static readonly MouseButton[] ButtonValues = Enum.GetValues<MouseButton>();
    private static Mouse? _mouse;
    private readonly List<MouseButton> _currentButtons = [];
    private readonly List<MouseButton> _downButtons = [];
    private readonly List<MouseButton> _pressedButtons = [];
    private readonly List<MouseButton> _releasedButtons = [];
    private readonly List<MouseButton> _upButtons = [];
    private Cursor _cursor = Cursor.Default;
    private Vector2 _screenPosition = Vector2.Zero;
    private Vector2 _scroll = Vector2.Zero;

    static Mouse()
    {
        Game.EnsureRunning();
    }

    private Mouse() { }

    public static EnumerableList<MouseButton> DownButtons => GetMouse()._downButtons;
    public static EnumerableList<MouseButton> UpButtons => GetMouse()._upButtons;
    public static EnumerableList<MouseButton> PressedButtons => GetMouse()._pressedButtons;
    public static EnumerableList<MouseButton> ReleasedButtons => GetMouse()._releasedButtons;

    public static bool OnScreen => Raylib.IsCursorOnScreen();

    public static Vector2 Scroll => GetMouse()._scroll;

    public static Vector2 Position
    {
        get => Coordinates.ScreenToLocal(GetMouse()._screenPosition).Clamp(Vector2.Zero, Display.Size);
        set
        {
            value = value.Clamp(Vector2.Zero, Display.Size);
            ScreenPosition = Coordinates.LocalToScreen(value);
        }
    }

    public static Vector2 WorldPosition
    {
        get => Coordinates.LocalToWorld(Position);
        set => ScreenPosition = Coordinates.WorldToScreen(value);
    }

    public static Vector2 ScreenPosition
    {
        get => GetMouse()._screenPosition;
        set
        {
            var mouse = GetMouse();
            if (!Display.Focused)
                return;
            value = value.Clamp(Vector2.Zero, Display.ScreenSize).Round();
            if (Precision.AreEqual(mouse._screenPosition, value))
                return;
            mouse._screenPosition = value;
            Raylib.SetMousePosition((int)value.X, (int)value.Y);
        }
    }

    public static Cursor Cursor
    {
        get => GetMouse()._cursor;
        set
        {
            var mouse = GetMouse();
            if (value == mouse._cursor)
                return;
            if (value == Cursor.None)
            {
                Raylib.HideCursor();
                mouse._cursor = value;
                return;
            }

            if (mouse._cursor == Cursor.None)
                Raylib.ShowCursor();
            Raylib.SetMouseCursor((MouseCursor)value);
            mouse._cursor = value;
        }
    }

    public static bool IsButtonDown(MouseButton button)
    {
        return GetMouse()._downButtons.Contains(button);
    }

    public static bool IsButtonUp(MouseButton button)
    {
        return GetMouse()._upButtons.Contains(button);
    }

    public static bool IsButtonPressed(MouseButton button)
    {
        return GetMouse()._pressedButtons.Contains(button);
    }

    public static bool IsButtonReleased(MouseButton button)
    {
        return GetMouse()._releasedButtons.Contains(button);
    }

    private static Mouse GetMouse()
    {
        return _mouse ??= new Mouse();
    }

    internal static void Update()
    {
        var mouse = GetMouse();
        if (!Display.Focused)
        {
            mouse.Reset();
            return;
        }

        mouse.UpdateState();
    }

    private void Reset()
    {
        _downButtons.Clear();
        _upButtons.Clear();
        _upButtons.AddRange(ButtonValues);
        _pressedButtons.Clear();
        _releasedButtons.Clear();
        _scroll = Vector2.Zero;
    }

    private void UpdateState()
    {
        _screenPosition = ((Vector2)Raylib.GetMousePosition()).Clamp(Vector2.Zero, Display.ScreenSize).Round();
        _scroll = Raylib.GetMouseWheelMoveV();
        if (Platform.Web.IsCurrent())
            _scroll.X = -_scroll.X;
        _currentButtons.Clear();
        foreach (var button in ButtonValues)
            if (Raylib.IsMouseButtonDown((Raylib_cs.BleedingEdge.MouseButton)button))
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
    }
}
