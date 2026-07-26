using Raylib_cs;
using Vigilance.Core;

namespace Vigilance.Input;

public enum Key : short
{
    Null = KeyboardKey.Null,
    Apostrophe = KeyboardKey.Apostrophe,
    Comma = KeyboardKey.Comma,
    Minus = KeyboardKey.Minus,
    Period = KeyboardKey.Period,
    Slash = KeyboardKey.Slash,
    Zero = KeyboardKey.Zero,
    One = KeyboardKey.One,
    Two = KeyboardKey.Two,
    Three = KeyboardKey.Three,
    Four = KeyboardKey.Four,
    Five = KeyboardKey.Five,
    Six = KeyboardKey.Six,
    Seven = KeyboardKey.Seven,
    Eight = KeyboardKey.Eight,
    Nine = KeyboardKey.Nine,
    Semicolon = KeyboardKey.Semicolon,
    Equal = KeyboardKey.Equal,
    A = KeyboardKey.A,
    B = KeyboardKey.B,
    C = KeyboardKey.C,
    D = KeyboardKey.D,
    E = KeyboardKey.E,
    F = KeyboardKey.F,
    G = KeyboardKey.G,
    H = KeyboardKey.H,
    I = KeyboardKey.I,
    J = KeyboardKey.J,
    K = KeyboardKey.K,
    L = KeyboardKey.L,
    M = KeyboardKey.M,
    N = KeyboardKey.N,
    O = KeyboardKey.O,
    P = KeyboardKey.P,
    Q = KeyboardKey.Q,
    R = KeyboardKey.R,
    S = KeyboardKey.S,
    T = KeyboardKey.T,
    U = KeyboardKey.U,
    V = KeyboardKey.V,
    W = KeyboardKey.W,
    X = KeyboardKey.X,
    Y = KeyboardKey.Y,
    Z = KeyboardKey.Z,
    Space = KeyboardKey.Space,
    Escape = KeyboardKey.Escape,
    Enter = KeyboardKey.Enter,
    Tab = KeyboardKey.Tab,
    Backspace = KeyboardKey.Backspace,
    Insert = KeyboardKey.Insert,
    Delete = KeyboardKey.Delete,
    Right = KeyboardKey.Right,
    Left = KeyboardKey.Left,
    Down = KeyboardKey.Down,
    Up = KeyboardKey.Up,
    PageUp = KeyboardKey.PageUp,
    PageDown = KeyboardKey.PageDown,
    Home = KeyboardKey.Home,
    End = KeyboardKey.End,
    CapsLock = KeyboardKey.CapsLock,
    ScrollLock = KeyboardKey.ScrollLock,
    NumLock = KeyboardKey.NumLock,
    PrintScreen = KeyboardKey.PrintScreen,
    Pause = KeyboardKey.Pause,
    F1 = KeyboardKey.F1,
    F2 = KeyboardKey.F2,
    F3 = KeyboardKey.F3,
    F4 = KeyboardKey.F4,
    F5 = KeyboardKey.F5,
    F6 = KeyboardKey.F6,
    F7 = KeyboardKey.F7,
    F8 = KeyboardKey.F8,
    F9 = KeyboardKey.F9,
    F10 = KeyboardKey.F10,
    F11 = KeyboardKey.F11,
    F12 = KeyboardKey.F12,
    LeftShift = KeyboardKey.LeftShift,
    LeftControl = KeyboardKey.LeftControl,
    LeftAlt = KeyboardKey.LeftAlt,
    LeftSuper = KeyboardKey.LeftSuper,
    RightShift = KeyboardKey.RightShift,
    RightControl = KeyboardKey.RightControl,
    RightAlt = KeyboardKey.RightAlt,
    RightSuper = KeyboardKey.RightSuper,
    Menu = KeyboardKey.KeyboardMenu,
    LeftBracket = KeyboardKey.LeftBracket,
    Backslash = KeyboardKey.Backslash,
    RightBracket = KeyboardKey.RightBracket,
    Grave = KeyboardKey.Grave,
    Kp0 = KeyboardKey.Kp0,
    Kp1 = KeyboardKey.Kp1,
    Kp2 = KeyboardKey.Kp2,
    Kp3 = KeyboardKey.Kp3,
    Kp4 = KeyboardKey.Kp4,
    Kp5 = KeyboardKey.Kp5,
    Kp6 = KeyboardKey.Kp6,
    Kp7 = KeyboardKey.Kp7,
    Kp8 = KeyboardKey.Kp8,
    Kp9 = KeyboardKey.Kp9,
    KpDecimal = KeyboardKey.KpDecimal,
    KpDivide = KeyboardKey.KpDivide,
    KpMultiply = KeyboardKey.KpMultiply,
    KpSubtract = KeyboardKey.KpSubtract,
    KpAdd = KeyboardKey.KpAdd,
    KpEnter = KeyboardKey.KpEnter,
    KpEqual = KeyboardKey.KpEqual,
}

public static class KeyExtensions
{
    extension(Key key)
    {
        public unsafe string Name
        {
            get
            {
                Game.ThrowIfNotRunning();
                if (Platform.Web.IsCurrent)
                    return key.ToString();
                var name = Utf8Ptr.GetString(Raylib.GetKeyName((KeyboardKey)key));
                return name.IsEmpty ? key.ToString() : name;
            }
        }

        public Button AsButton()
        {
            return key;
        }
    }
}
