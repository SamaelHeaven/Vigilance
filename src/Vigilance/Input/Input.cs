using Raylib_cs;
using Vigilance.Core;
using Vigilance.Math;

namespace Vigilance.Input;

public static class Input
{
    private static InputConfig _config = new();

    public static InputButton? ExitButton { get; set; } = _config.ExitButton;

    public static InputButton? FullscreenButton { get; set; } = _config.FullscreenButton;

    public static InputAxis HorizontalAxis { get; set; } = _config.HorizontalAxis;

    public static InputAxis VerticalAxis { get; set; } = _config.VerticalAxis;

    public static Vector2 Direction => new(HorizontalAxis.Direction, VerticalAxis.Direction);

    public static Vector2 Axes => new(HorizontalAxis.Value, VerticalAxis.Value);

    public static Vector2 RawAxes => new(HorizontalAxis.RawValue, VerticalAxis.RawValue);

    internal static void Initialize()
    {
        _config = Game.Config.Take<InputConfig>() ?? _config;
        ExitButton = _config.ExitButton;
        FullscreenButton = _config.FullscreenButton;
        HorizontalAxis = _config.HorizontalAxis;
        VerticalAxis = _config.VerticalAxis;
        Raylib.SetExitKey(KeyboardKey.Null);
    }
}

public sealed class InputConfig
{
    public InputButton ExitButton { get; set; } = new();
    public InputButton FullscreenButton { get; set; } = new();

    public InputAxis HorizontalAxis { get; set; } =
        new([Key.Left, Key.A], [Key.Right, Key.D], gamepadAxes: [GamepadAxis.LeftX]);

    public InputAxis VerticalAxis { get; set; } =
        new([Key.Up, Key.W], [Key.Down, Key.S], gamepadAxes: [GamepadAxis.LeftY]);
}

public static class InputConfigExtensions
{
    public static ConfigBuilder Input(this ConfigBuilder builder, Action<InputConfig> config)
    {
        return builder.Add(config);
    }
}
