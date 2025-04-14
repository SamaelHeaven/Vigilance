using Vigilance.Core;
using Vigilance.Math;

namespace Vigilance.Input;

public struct InputAxis
{
    public IEnumerable<Key> NegativeKeys = [];
    public IEnumerable<Key> PositiveKeys = [];
    public IEnumerable<GamepadButton> NegativeGamepadButtons = [];
    public IEnumerable<GamepadButton> PositiveGamepadButtons = [];
    public IEnumerable<GamepadAxis> GamepadAxes = [];
    public Gamepad Gamepad = Gamepad.First;

    public static InputAxis Horizontal => Game.HorizontalInputAxis;

    public static InputAxis Vertical => Game.VerticalInputAxis;

    public static Vector2 Both()
    {
        return new Vector2(Horizontal.Get(), Vertical.Get());
    }

    public InputAxis() { }

    public readonly int Get()
    {
        var gamepad = Gamepad;
        var negative =
            NegativeKeys.Any(Keyboard.IsKeyDown)
            || NegativeGamepadButtons.Any(button => gamepad.IsButtonDown(button))
            || GamepadAxes.Any(axis => (int)MathF.Round(gamepad.GetAxis(axis)) == -1);
        var positive =
            PositiveKeys.Any(Keyboard.IsKeyDown)
            || PositiveGamepadButtons.Any(button => gamepad.IsButtonDown(button))
            || GamepadAxes.Any(axis => (int)MathF.Round(gamepad.GetAxis(axis)) == 1);
        if (negative && !positive)
            return -1;
        if (positive && !negative)
            return 1;
        return 0;
    }
}
