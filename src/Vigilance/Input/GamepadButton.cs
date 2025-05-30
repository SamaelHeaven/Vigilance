namespace Vigilance.Input;

public enum GamepadButton
{
    DPadUp = Raylib_cs.GamepadButton.LeftFaceUp,
    DPadRight = Raylib_cs.GamepadButton.LeftFaceRight,
    DPadDown = Raylib_cs.GamepadButton.LeftFaceDown,
    DPadLeft = Raylib_cs.GamepadButton.LeftFaceLeft,
    Y = Raylib_cs.GamepadButton.RightFaceUp,
    X = Raylib_cs.GamepadButton.RightFaceLeft,
    A = Raylib_cs.GamepadButton.RightFaceDown,
    B = Raylib_cs.GamepadButton.RightFaceRight,
    LeftBumper = Raylib_cs.GamepadButton.LeftTrigger1,
    LeftTrigger = Raylib_cs.GamepadButton.LeftTrigger2,
    RightBumper = Raylib_cs.GamepadButton.RightTrigger1,
    RightTrigger = Raylib_cs.GamepadButton.RightTrigger2,
    Select = Raylib_cs.GamepadButton.MiddleLeft,
    Start = Raylib_cs.GamepadButton.MiddleRight,
    LeftThumb = Raylib_cs.GamepadButton.LeftThumb,
    RightThumb = Raylib_cs.GamepadButton.RightThumb,
}

public static class GamepadButtonExtensions
{
    public static int GetJSValue(this GamepadButton button)
    {
        return button switch
        {
            GamepadButton.A => 0,
            GamepadButton.B => 1,
            GamepadButton.X => 2,
            GamepadButton.Y => 3,
            GamepadButton.LeftBumper => 4,
            GamepadButton.RightBumper => 5,
            GamepadButton.LeftTrigger => 6,
            GamepadButton.RightTrigger => 7,
            GamepadButton.Select => 8,
            GamepadButton.Start => 9,
            GamepadButton.LeftThumb => 10,
            GamepadButton.RightThumb => 11,
            GamepadButton.DPadUp => 12,
            GamepadButton.DPadDown => 13,
            GamepadButton.DPadLeft => 14,
            GamepadButton.DPadRight => 15,
            _ => -1,
        };
    }
}
