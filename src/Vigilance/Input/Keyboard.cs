using System.Text;
using LinkDotNet.StringBuilder;
using Raylib_cs;

namespace Vigilance.Input;

public static unsafe class Keyboard
{
    private static readonly Key[] _keyValues;
    private static ValueList<Key> _currentKeys = [];
    private static ValueList<Key> _downKeys = [];
    private static ValueList<Key> _pressedKeys = [];
    private static ValueList<Key> _releasedKeys = [];
    private static ValueList<Key> _upKeys = [];

    static Keyboard()
    {
        Game.ThrowIfNotRunning();
        _keyValues = Key.Values().AsValueEnumerable().Where(key => key != Key.Null).ToArray();
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
        _downKeys.Clear();
        _upKeys.Clear();
        _upKeys.AddRange(_keyValues);
        _pressedKeys.Clear();
        _releasedKeys.Clear();
    }

    private static void UpdateState()
    {
        using var typedString = new ValueStringBuilder(stackalloc char[32]);
        Span<char> utf16 = stackalloc char[2];
        for (var unicode = Raylib.GetCharPressed(); unicode != 0; unicode = Raylib.GetCharPressed())
        {
            if (!Rune.IsValid(unicode))
                continue;
            var rune = new Rune(unicode);
            if (!rune.TryEncodeToUtf16(utf16, out var written))
                continue;
            for (var i = 0; i < written; i++)
                typedString.Append(utf16[i]);
        }

        TypedString = typedString.ToString();
        _currentKeys.Clear();
        foreach (var key in _keyValues)
            if (Raylib.IsKeyDown((KeyboardKey)key))
                _currentKeys.Add(key);
        _pressedKeys.Clear();
        _pressedKeys.AddRange(_currentKeys);
        _pressedKeys.RemoveAll(_downKeys);
        _releasedKeys.Clear();
        _releasedKeys.AddRange(_downKeys);
        _releasedKeys.RemoveAll(_currentKeys);
        _downKeys.Clear();
        _downKeys.AddRange(_currentKeys);
        _upKeys.Clear();
        _upKeys.AddRange(_keyValues);
        _upKeys.RemoveAll(_currentKeys);
    }
}
