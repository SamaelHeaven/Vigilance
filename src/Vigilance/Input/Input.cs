using Raylib_cs;
using Vigilance.Core;
using Vigilance.Math;

namespace Vigilance.Input;

public static class Input
{
    private static InputConfig _config = new();

    public static ButtonInputs ExitInputs { get; set; } = _config.ExitInputs;

    public static ButtonInputs FullscreenInputs { get; set; } = _config.FullscreenInputs;

    public static AxisInputs HorizontalInputs { get; set; } = _config.HorizontalInputs;

    public static AxisInputs VerticalInputs { get; set; } = _config.VerticalInputs;

    public static Vector2 Position => new(HorizontalInputs.Position, VerticalInputs.Position);

    public static Vector2 Magnitude => new(HorizontalInputs.Magnitude, VerticalInputs.Magnitude);

    public static Vector2 RawMagnitude => new(HorizontalInputs.RawMagnitude, VerticalInputs.RawMagnitude);

    internal static void Initialize()
    {
        _config = Game.Config.Take<InputConfig>() ?? _config;
        ExitInputs = _config.ExitInputs;
        FullscreenInputs = _config.FullscreenInputs;
        HorizontalInputs = _config.HorizontalInputs;
        VerticalInputs = _config.VerticalInputs;
        Raylib.SetExitKey(KeyboardKey.Null);
    }
}

public sealed class InputConfig
{
    public ButtonInputs ExitInputs { get; set; } = [];
    public ButtonInputs FullscreenInputs { get; set; } = [];
    public AxisInputs HorizontalInputs { get; set; } = [(Key.Left, Key.Right), (Key.A, Key.D), GamepadAxis.LeftX];
    public AxisInputs VerticalInputs { get; set; } = [(Key.Up, Key.Down), (Key.W, Key.S), GamepadAxis.LeftY];
}

public static class InputConfigExtensions
{
    public static ConfigBuilder Input(this ConfigBuilder builder, Action<InputConfig> config)
    {
        return builder.Add(config);
    }
}
