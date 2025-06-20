namespace Vigilance.Drawing;

public readonly struct RendererConfig
{
    public RenderingMode Mode { get; init; }
    public float Scale { get; init; }
    public Interpolation Interpolation { get; init; }

    public static RendererConfig Screen()
    {
        return new RendererConfig { Mode = RenderingMode.Screen };
    }

    public static RendererConfig Buffer(float scale = 1, Interpolation interpolation = Interpolation.Nearest)
    {
        return new RendererConfig
        {
            Mode = RenderingMode.Buffer,
            Scale = scale,
            Interpolation = interpolation,
        };
    }
}
