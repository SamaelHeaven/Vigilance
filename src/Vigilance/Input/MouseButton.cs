namespace Vigilance.Input;

public enum MouseButton : sbyte
{
    Left = Raylib_cs.MouseButton.Left,
    Middle = Raylib_cs.MouseButton.Middle,
    Right = Raylib_cs.MouseButton.Right,
    Side = Raylib_cs.MouseButton.Side,
    Extra = Raylib_cs.MouseButton.Extra,
    Forward = Raylib_cs.MouseButton.Forward,
    Back = Raylib_cs.MouseButton.Back,
}

public static class MouseButtonExtensions
{
    extension(MouseButton button)
    {
        public Button AsButton()
        {
            return Button.From(button);
        }
    }
}
