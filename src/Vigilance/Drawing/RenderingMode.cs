namespace Vigilance.Drawing;

public readonly struct RenderingMode
{
    public RenderingModeType ModeType { get; init; }
    public Interpolation Interpolation { get; init; }
    public float Scale { get; init; }

    public static RenderingMode Screen => new() { ModeType = RenderingModeType.Screen };

    public static RenderingMode Buffer(float scale = 1, Interpolation interpolation = Interpolation.Nearest)
    {
        return new RenderingMode
        {
            ModeType = RenderingModeType.Buffer,
            Scale = scale,
            Interpolation = interpolation,
        };
    }
}

public enum RenderingModeType
{
    Screen,
    Buffer,
}
