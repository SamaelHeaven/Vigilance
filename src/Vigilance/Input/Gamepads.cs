using System.Runtime.CompilerServices;

namespace Vigilance.Input;

[Flags]
public enum Gamepads : byte
{
    None = 0,
    First = 1 << 0,
    Second = 1 << 1,
    Third = 1 << 2,
    Fourth = 1 << 3,
    All = First | Second | Third | Fourth,
}

public static class GamepadIdsExtensions
{
    extension(Gamepads gamepads)
    {
        public InlineList<InlineArray4<Gamepad>, Gamepad> ToInlineList()
        {
            InlineList<InlineArray4<Gamepad>, Gamepad> list = [];
            if ((gamepads & Gamepads.First) != 0)
                list.Add(Gamepad.First);
            if ((gamepads & Gamepads.Second) != 0)
                list.Add(Gamepad.Second);
            if ((gamepads & Gamepads.Third) != 0)
                list.Add(Gamepad.Third);
            if ((gamepads & Gamepads.Fourth) != 0)
                list.Add(Gamepad.Fourth);
            return list;
        }

        public IEnumerable<Gamepad> AsEnumerable()
        {
            if ((gamepads & Gamepads.First) != 0)
                yield return Gamepad.First;
            if ((gamepads & Gamepads.Second) != 0)
                yield return Gamepad.Second;
            if ((gamepads & Gamepads.Third) != 0)
                yield return Gamepad.Third;
            if ((gamepads & Gamepads.Fourth) != 0)
                yield return Gamepad.Fourth;
        }

        public ValueEnumerable<InlineList<InlineArray4<Gamepad>, Gamepad>.Enumerator, Gamepad> AsValueEnumerable()
        {
            return gamepads.ToInlineList().AsValueEnumerable();
        }
    }
}
