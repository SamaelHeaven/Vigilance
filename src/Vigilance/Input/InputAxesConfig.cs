using Vigilance.Core;

namespace Vigilance.Input;

public sealed class InputAxesConfig
{
    public InputAxis Horizontal { get; set; } =
        new()
        {
            NegativeKeys = [Key.Left, Key.A],
            PositiveKeys = [Key.Right, Key.D],
            GamepadAxes = [GamepadAxis.LeftX]
        };

    public InputAxis Vertical { get; set; } =
        new()
        {
            NegativeKeys = [Key.Up, Key.W],
            PositiveKeys = [Key.Down, Key.S],
            GamepadAxes = [GamepadAxis.LeftY]
        };
}

public static class InputAxesConfigExtensions
{
    public static ConfigsBuilder InputAxes(this ConfigsBuilder builder, InputAxesConfig config)
    {
        return builder.AddConfig(config);
    }
}
