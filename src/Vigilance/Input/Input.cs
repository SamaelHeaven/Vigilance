using Raylib_cs.BleedingEdge;
using Vigilance.Core;
using Vigilance.Math;

namespace Vigilance.Input;

public static class Input
{
    private static InputConfig _config = new();

    public static Key ExitKey
    {
        get => _config.ExitKey;
        set
        {
            _config.ExitKey = value;
            Raylib.SetExitKey((KeyboardKey)value);
        }
    }

    public static Key FullscreenKey
    {
        get => _config.FullscreenKey;
        set => _config.FullscreenKey = value;
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

    public static Vector2 Axes => new(HorizontalAxis.Value, VerticalAxis.Value);

    internal static void Initialize()
    {
        _config = Game.Config.Take<InputConfig>() ?? _config;
    }
}

public sealed class InputConfig
{
    public Key ExitKey { get; set; } = Key.Null;
    public Key FullscreenKey { get; set; } = Key.Null;

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
