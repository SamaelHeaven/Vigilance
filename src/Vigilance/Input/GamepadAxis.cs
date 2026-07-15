namespace Vigilance.Input;

public enum GamepadAxis : sbyte
{
    LeftX = Raylib_cs.GamepadAxis.LeftX,
    LeftY = Raylib_cs.GamepadAxis.LeftY,
    RightX = Raylib_cs.GamepadAxis.RightX,
    RightY = Raylib_cs.GamepadAxis.RightY,
    LeftTrigger = Raylib_cs.GamepadAxis.LeftTrigger,
    RightTrigger = Raylib_cs.GamepadAxis.RightTrigger,
}

public static class GamepadAxisExtensions
{
    extension(GamepadAxis axis)
    {
        public int JSValue => (int)axis;
    }
}
