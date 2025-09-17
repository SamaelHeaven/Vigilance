using System.Text;
using Raylib_cs.BleedingEdge;
using Vigilance.Core;

namespace Vigilance.Input;

public sealed class Keyboard
{
    private static readonly Key[] KeyValues;
    private static Keyboard? _keyboard;
    private readonly List<Key> _currentKeys = [];
    private readonly List<Key> _downKeys = [];
    private readonly List<Key> _pressedKeys = [];
    private readonly List<Key> _releasedKeys = [];
    private readonly StringBuilder _typedString = new();
    private readonly List<Key> _upKeys = [];

    static Keyboard()
    {
        Game.EnsureRunning();
        KeyValues = Enum.GetValues<Key>().Where(key => key != Key.Null).ToArray();
    }

    private Keyboard() { }

    public static string TypedString => GetKeyboard()._typedString.ToString();
    public static EnumerableList<Key> DownKeys => GetKeyboard()._downKeys;
    public static EnumerableList<Key> UpKeys => GetKeyboard()._upKeys;
    public static EnumerableList<Key> PressedKeys => GetKeyboard()._pressedKeys;
    public static EnumerableList<Key> ReleasedKeys => GetKeyboard()._releasedKeys;

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
        if (!Game.Focused)
        {
            keyboard.Reset();
            return;
        }

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
        _typedString.Clear();
        for (var c = (char)Raylib.GetCharPressed(); c != 0; c = (char)Raylib.GetCharPressed())
            _typedString.Append(c);
        _currentKeys.Clear();
        foreach (var key in KeyValues)
            if (Raylib.IsKeyDown((KeyboardKey)key))
                _currentKeys.Add(key);
        _pressedKeys.Clear();
        _pressedKeys.AddRange(_currentKeys);
        _pressedKeys.RemoveAll(key => _downKeys.Contains(key));
        _releasedKeys.Clear();
        _releasedKeys.AddRange(_downKeys);
        _releasedKeys.RemoveAll(key => _currentKeys.Contains(key));
        _downKeys.Clear();
        _downKeys.AddRange(_currentKeys);
        _upKeys.Clear();
        _upKeys.AddRange(KeyValues);
        _upKeys.RemoveAll(key => _currentKeys.Contains(key));
    }
}
