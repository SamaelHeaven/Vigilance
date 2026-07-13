namespace Vigilance.Drawing;

public readonly record struct RenderingMode
{
    public RenderingModeType Type { get; private init; }
    public TextureFilter TextureFilter { get; private init; }
    public float Scale { get; private init; }

    public static RenderingMode Native()
    {
        return new RenderingMode { Type = RenderingModeType.Native };
    }

    public static RenderingMode Buffer(float scale = 1, TextureFilter textureFilter = TextureFilter.Nearest)
    {
        return new RenderingMode
        {
            Type = RenderingModeType.Buffer,
            Scale = scale,
            TextureFilter = textureFilter,
        };
    }
}

public enum RenderingModeType : sbyte
{
    Native,
    Buffer,
}
