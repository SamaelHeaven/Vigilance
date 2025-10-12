namespace Vigilance.Input;

public enum GamepadAxis
{
    LeftX = Raylib_cs.BleedingEdge.GamepadAxis.LeftX,
    LeftY = Raylib_cs.BleedingEdge.GamepadAxis.LeftY,
    RightX = Raylib_cs.BleedingEdge.GamepadAxis.RightX,
    RightY = Raylib_cs.BleedingEdge.GamepadAxis.RightY,
    LeftTrigger = Raylib_cs.BleedingEdge.GamepadAxis.LeftTrigger,
    RightTrigger = Raylib_cs.BleedingEdge.GamepadAxis.RightTrigger,
}

public static class GamepadAxisExtensions
{
    extension(GamepadAxis axis)
    {
        public int JSValue => (int)axis;
    }
}
