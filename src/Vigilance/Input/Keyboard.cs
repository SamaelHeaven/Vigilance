using System.Text;
using Raylib_cs;
using Vigilance.Core;

namespace Vigilance.Input;

public sealed class Keyboard
{
    private static readonly Key[] KeyValues;
    private static Keyboard? _keyboard;
    private readonly List<Key> _downKeys = [];
    private readonly List<Key> _pressedKeys = [];
    private readonly List<Key> _releasedKeys = [];
    private readonly StringBuilder _typedString = new();
    private readonly List<Key> _upKeys = [];

    static Keyboard()
    {
        Game.EnsureRunning();
        KeyValues = Enum.GetValues<Key>().ToArray();
    }

    private Keyboard() { }

    public static string TypedString => GetKeyboard()._typedString.ToString();
    public static IReadOnlyList<Key> DownKeys => GetKeyboard()._downKeys.AsReadOnly();
    public static IReadOnlyList<Key> UpKeys => GetKeyboard()._upKeys.AsReadOnly();
    public static IReadOnlyList<Key> PressedKeys => GetKeyboard()._pressedKeys.AsReadOnly();
    public static IReadOnlyList<Key> ReleasedKeys => GetKeyboard()._releasedKeys.AsReadOnly();

    public static bool IsKeyDown(Key key)
    {
        return GetKeyboard()._downKeys.Contains(key);
    }

    public static bool IsKeyUp(Key key)
    {
        return GetKeyboard()._upKeys.Contains(key);
    }

    public static bool IsKeyPressed(Key key)
    {
        return GetKeyboard()._pressedKeys.Contains(key);
    }

    public static bool IsKeyReleased(Key key)
    {
        return GetKeyboard()._releasedKeys.Contains(key);
    }

    private static Keyboard GetKeyboard()
    {
        return _keyboard ??= new Keyboard();
    }

    internal static void Update()
    {
        var keyboard = GetKeyboard();
        keyboard.Reset();
        if (!Game.Focused)
            return;
        keyboard.UpdateState();
    }

    private void Reset()
    {
        _typedString.Clear();
        _downKeys.Clear();
        _upKeys.Clear();
        _upKeys.AddRange(KeyValues);
        _pressedKeys.Clear();
        _releasedKeys.Clear();
    }

    private void UpdateState()
    {
        for (var c = (char)Raylib.GetCharPressed(); c != 0; c = (char)Raylib.GetCharPressed())
            _typedString.Append(c);
        foreach (var key in KeyValues)
        {
            if (Raylib.IsKeyDown((KeyboardKey)key))
            {
                _downKeys.Add(key);
                _upKeys.Remove(key);
            }

            if (Raylib.IsKeyPressed((KeyboardKey)key))
                _pressedKeys.Add(key);
            if (Raylib.IsKeyReleased((KeyboardKey)key))
                _releasedKeys.Add(key);
        }
    }
}
