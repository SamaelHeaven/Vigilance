namespace Vigilance.Input;

public enum GamepadButton
{
    DPadUp = Raylib_cs.BleedingEdge.GamepadButton.LeftFaceUp,
    DPadRight = Raylib_cs.BleedingEdge.GamepadButton.LeftFaceRight,
    DPadDown = Raylib_cs.BleedingEdge.GamepadButton.LeftFaceDown,
    DPadLeft = Raylib_cs.BleedingEdge.GamepadButton.LeftFaceLeft,
    Y = Raylib_cs.BleedingEdge.GamepadButton.RightFaceUp,
    X = Raylib_cs.BleedingEdge.GamepadButton.RightFaceLeft,
    A = Raylib_cs.BleedingEdge.GamepadButton.RightFaceDown,
    B = Raylib_cs.BleedingEdge.GamepadButton.RightFaceRight,
    LeftBumper = Raylib_cs.BleedingEdge.GamepadButton.LeftTrigger1,
    LeftTrigger = Raylib_cs.BleedingEdge.GamepadButton.LeftTrigger2,
    RightBumper = Raylib_cs.BleedingEdge.GamepadButton.RightTrigger1,
    RightTrigger = Raylib_cs.BleedingEdge.GamepadButton.RightTrigger2,
    Select = Raylib_cs.BleedingEdge.GamepadButton.MiddleLeft,
    Start = Raylib_cs.BleedingEdge.GamepadButton.MiddleRight,
    LeftThumb = Raylib_cs.BleedingEdge.GamepadButton.LeftThumb,
    RightThumb = Raylib_cs.BleedingEdge.GamepadButton.RightThumb,
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
