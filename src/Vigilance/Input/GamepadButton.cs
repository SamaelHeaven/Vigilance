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
