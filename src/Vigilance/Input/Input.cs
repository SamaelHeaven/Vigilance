using Raylib_cs;
using Vigilance.Core;
using Vigilance.Math;

namespace Vigilance.Input;

public static class Input
{
    private static InputConfig _config = new();

    public static InputButton? ExitButton
    {
        get => _config.ExitButton;
        set => _config.ExitButton = value;
    }

    public static InputButton? FullscreenButton
    {
        get => _config.FullscreenButton;
        set => _config.FullscreenButton = value;
    }

    public static InputAxis HorizontalAxis
    {
        get => _config.HorizontalAxis;
        set => _config.HorizontalAxis = value;
    }

    public static InputAxis VerticalAxis
    {
        get => _config.VerticalAxis;
        set => _config.VerticalAxis = value;
    }

    public static Vector2 Direction => new(HorizontalAxis.Direction, VerticalAxis.Direction);

    public static Vector2 Axes => new(HorizontalAxis.Value, VerticalAxis.Value);

    public static Vector2 RawAxes => new(HorizontalAxis.RawValue, VerticalAxis.RawValue);

    internal static void Initialize()
    {
        _config = Game.Config.Take<InputConfig>() ?? _config;
        Raylib.SetExitKey(KeyboardKey.Null);
    }
}

public sealed class InputConfig
{
    public InputButton? ExitButton { get; set; } = null;
    public InputButton? FullscreenButton { get; set; } = null;

    public InputAxis HorizontalAxis { get; set; } =
        new()
        {
            NegativeKeys = [Key.Left, Key.A],
            PositiveKeys = [Key.Right, Key.D],
            GamepadAxes = [GamepadAxis.LeftX],
        };

    public InputAxis VerticalAxis { get; set; } =
        new()
        {
            NegativeKeys = [Key.Up, Key.W],
            PositiveKeys = [Key.Down, Key.S],
            GamepadAxes = [GamepadAxis.LeftY],
        };
}

public static class InputConfigExtensions
{
    public static ConfigBuilder Input(this ConfigBuilder builder, Action<InputConfig> config)
    {
        return builder.Add(config);
    }
}
