using Raylib_cs;
using Vigilance.Core;
using Vigilance.Math;
using Vector2 = Vigilance.Math.Vector2;

namespace Vigilance.Input;

public sealed class Mouse
{
    private static readonly MouseButton[] ButtonValues = Enum.GetValues<MouseButton>().ToArray();
    private static Mouse? _mouse;
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

    public static IReadOnlyList<MouseButton> DownButtons => GetMouse()._downButtons.AsReadOnly();
    public static IReadOnlyList<MouseButton> UpButtons => GetMouse()._upButtons.AsReadOnly();
    public static IReadOnlyList<MouseButton> PressedButtons => GetMouse()._pressedButtons.AsReadOnly();
    public static IReadOnlyList<MouseButton> ReleasedButtons => GetMouse()._releasedButtons.AsReadOnly();

    public static bool OnScreen => Raylib.IsCursorOnScreen();

    public static Vector2 Scroll => GetMouse()._scroll;

    public static Vector2 Position
    {
        get => Coordinates.ScreenToLocal(GetMouse()._screenPosition).Clamp(Vector2.Zero, Game.Size).Round();
        set
        {
            var position = value.Clamp(Vector2.Zero, Game.Size).Round();
            if (Position == position)
                return;
            ScreenPosition = Coordinates.LocalToScreen(position);
        }
    }

    public static Vector2 ScreenPosition
    {
        get => GetMouse()._screenPosition;
        set
        {
            var mouse = GetMouse();
            if (!Game.Focused)
                return;
            var position = value.Clamp(Vector2.Zero, Game.ScreenSize).Round();
            if (mouse._screenPosition == position)
                return;
            mouse._screenPosition = position;
            Raylib.SetMousePosition((int)position.X, (int)position.Y);
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
        mouse.Reset();
        if (!Game.Focused)
            return;
        mouse.UpdateState();
    }

    private void Reset()
    {
        _downButtons.Clear();
        _upButtons.Clear();
        _upButtons.AddRange(ButtonValues);
        _pressedButtons.Clear();
        _releasedButtons.Clear();
    }

    private void UpdateState()
    {
        var position = Raylib.GetMousePosition();
        var scroll = Raylib.GetMouseWheelMoveV();
        _screenPosition = new Vector2(position.X, position.Y).Clamp(Vector2.Zero, Game.ScreenSize).Round();
        _scroll = new Vector2(scroll.X, scroll.Y);
        foreach (var button in ButtonValues)
        {
            if (Raylib.IsMouseButtonDown((Raylib_cs.MouseButton)button))
            {
                _downButtons.Add(button);
                _upButtons.Remove(button);
            }

            if (Raylib.IsMouseButtonPressed((Raylib_cs.MouseButton)button))
                _pressedButtons.Add(button);
            if (Raylib.IsMouseButtonReleased((Raylib_cs.MouseButton)button))
                _releasedButtons.Add(button);
        }
    }
}
