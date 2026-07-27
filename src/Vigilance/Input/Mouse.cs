using Raylib_cs;

namespace Vigilance.Input;

public static class Mouse
{
    private static ValueList<MouseButton> _currentButtons = [];
    private static ValueList<MouseButton> _downButtons = [];
    private static ValueList<MouseButton> _pressedButtons = [];
    private static ValueList<MouseButton> _releasedButtons = [];
    private static ValueList<MouseButton> _upButtons = [];
    private static Vector2 _screenPosition = Vector2.Zero;
    private static Vector2 _scroll = Vector2.Zero;

    static Mouse()
    {
        Game.ThrowIfNotRunning();
    }

    public static ValueListView<MouseButton> DownButtons => _downButtons;
    public static ValueListView<MouseButton> UpButtons => _upButtons;
    public static ValueListView<MouseButton> PressedButtons => _pressedButtons;
    public static ValueListView<MouseButton> ReleasedButtons => _releasedButtons;

    public static bool OnScreen { get; private set; }

    public static Vector2 Scroll => _scroll;

    public static Vector2 Position
    {
        get => Coordinates.ScreenToLocal(_screenPosition).Clamp(Vector2.Zero, Display.Size);
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
        get => _screenPosition;
        set
        {
            if (!Display.Focused)
                return;
            value = value.Clamp(Vector2.Zero, Display.ScreenSize).Round();
            if (Precision.AreEqual(_screenPosition, value))
                return;
            _screenPosition = value;
            Raylib.SetMousePosition((int)value.X, (int)value.Y);
        }
    }

    public static Cursor Cursor
    {
        get;
        set
        {
            if (value == field)
                return;
            if (value == Cursor.None)
            {
                Raylib.HideCursor();
                field = value;
                return;
            }

            if (field == Cursor.None)
                Raylib.ShowCursor();
            Raylib.SetMouseCursor((MouseCursor)value);
            field = value;
        }
    } = Cursor.Default;

    public static bool IsButtonDown(MouseButton button)
    {
        return _downButtons.Contains(button);
    }

    public static bool IsButtonUp(MouseButton button)
    {
        return !_downButtons.Contains(button);
    }

    public static bool IsButtonPressed(MouseButton button)
    {
        return _pressedButtons.Contains(button);
    }

    public static bool IsButtonReleased(MouseButton button)
    {
        return _releasedButtons.Contains(button);
    }

    internal static void Update()
    {
        if (!Display.Focused)
        {
            Reset();
            return;
        }

        UpdateState();
    }

    private static void Reset()
    {
        _downButtons.Clear();
        _upButtons.Clear();
        _upButtons.AddRange(MouseButton.Values());
        _pressedButtons.Clear();
        _releasedButtons.Clear();
        _scroll = Vector2.Zero;
    }

    private static void UpdateState()
    {
        var mousePosition = Raylib.GetMousePosition();
        _screenPosition = ((Vector2)mousePosition).Clamp(Vector2.Zero, Display.ScreenSize).Round();
        OnScreen = Platform.Desktop.IsCurrent
            ? OperatingSystem.IsMacOS()
                ? mousePosition is { X: >= 0, Y: >= 0 }
                    && mousePosition.X <= Display.ScreenWidth
                    && mousePosition.Y <= Display.ScreenHeight
                : Raylib.IsCursorOnScreen()
            : Display.Focused;
        _scroll = Raylib.GetMouseWheelMoveV();
        if (Platform.Web.IsCurrent)
            _scroll.X = -_scroll.X;
        _currentButtons.Clear();
        foreach (var button in MouseButton.Values())
            if (Raylib.IsMouseButtonDown((Raylib_cs.MouseButton)button))
                _currentButtons.Add(button);
        _pressedButtons.Clear();
        _pressedButtons.AddRange(_currentButtons);
        _pressedButtons.RemoveAll(_downButtons);
        _releasedButtons.Clear();
        _releasedButtons.AddRange(_downButtons);
        _releasedButtons.RemoveAll(_currentButtons);
        _downButtons.Clear();
        _downButtons.AddRange(_currentButtons);
        _upButtons.Clear();
        _upButtons.AddRange(MouseButton.Values());
        _upButtons.RemoveAll(_currentButtons);
    }
}
