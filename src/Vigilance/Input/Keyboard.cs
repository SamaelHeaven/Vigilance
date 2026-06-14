using System.Text;
using Raylib_cs;
using Vigilance.Collections;
using Vigilance.Core;
using ZLinq;

namespace Vigilance.Input;

public static class Keyboard
{
    private static readonly Key[] _keyValues;
    private static ValueList<Key> _currentKeys = [];
    private static ValueList<Key> _downKeys = [];
    private static ValueList<Key> _pressedKeys = [];
    private static ValueList<Key> _releasedKeys = [];
    private static readonly StringBuilder _typedString = new();
    private static ValueList<Key> _upKeys = [];

    static Keyboard()
    {
        Game.ThrowIfNotRunning();
        _keyValues = Enum.GetValues<Key>().AsValueEnumerable().Where(key => key != Key.Null).ToArray();
    }

    public static string TypedString { get; private set; } = "";
    public static ValueListView<Key> DownKeys => _downKeys;
    public static ValueListView<Key> UpKeys => _upKeys;
    public static ValueListView<Key> PressedKeys => _pressedKeys;
    public static ValueListView<Key> ReleasedKeys => _releasedKeys;

    public static bool IsKeyDown(Key key)
    {
        return _downKeys.Contains(key);
    }

    public static bool IsKeyUp(Key key)
    {
        return !_downKeys.Contains(key);
    }

    public static bool IsKeyPressed(Key key)
    {
        return _pressedKeys.Contains(key);
    }

    public static bool IsKeyReleased(Key key)
    {
        return _releasedKeys.Contains(key);
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
        _typedString.Clear();
        _downKeys.Clear();
        _upKeys.Clear();
        _upKeys.AddRange(_keyValues);
        _pressedKeys.Clear();
        _releasedKeys.Clear();
    }

    private static void UpdateState()
    {
        _typedString.Clear();
        for (var c = (char)Raylib.GetCharPressed(); c != 0; c = (char)Raylib.GetCharPressed())
            _typedString.Append(c);
        TypedString = _typedString.ToString();
        _currentKeys.Clear();
        foreach (var key in _keyValues)
            if (Raylib.IsKeyDown((KeyboardKey)key))
                _currentKeys.Add(key);
        _pressedKeys.Clear();
        _pressedKeys.AddRange(_currentKeys);
        _pressedKeys.RemoveAll(_downKeys.Contains);
        _releasedKeys.Clear();
        _releasedKeys.AddRange(_downKeys);
        _releasedKeys.RemoveAll(_currentKeys.Contains);
        _downKeys.Clear();
        _downKeys.AddRange(_currentKeys);
        _upKeys.Clear();
        _upKeys.AddRange(_keyValues);
        _upKeys.RemoveAll(_currentKeys.Contains);
    }
}
