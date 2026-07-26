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
        public int JSValue
        {
            get
            {
                return axis switch
                {
                    GamepadAxis.LeftX => 0,
                    GamepadAxis.LeftY => 1,
                    GamepadAxis.RightX => 2,
                    GamepadAxis.RightY => 3,
                    _ => -1,
                };
            }
        }

        public Axis AsAxis(Gamepads gamepads = Gamepads.All, float deadZone = 0)
        {
            return new Axis(axis, gamepads, deadZone);
        }
    }
}
