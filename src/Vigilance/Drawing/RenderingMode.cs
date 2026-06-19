namespace Vigilance.Drawing;

public readonly record struct RenderingMode
{
    public RenderingModeType Type { get; private init; }
    public Interpolation Interpolation { get; private init; }
    public float Scale { get; private init; }

    public static RenderingMode Native()
    {
        return new RenderingMode { Type = RenderingModeType.Native };
    }

    public static RenderingMode Buffer(float scale = 1, Interpolation interpolation = Interpolation.Nearest)
    {
        return new RenderingMode
        {
            Type = RenderingModeType.Buffer,
            Scale = scale,
            Interpolation = interpolation,
        };
    }
}

public enum RenderingModeType : byte
{
    Native,
    Buffer,
}
