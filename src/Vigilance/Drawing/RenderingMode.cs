namespace Vigilance.Drawing;

public readonly record struct RenderingMode
{
    public float Scale { get; private init; }
    public RenderingModeType Type { get; private init; }
    public TextureFilter TextureFilter { get; private init; }
    public bool Pool { get; private init; }

    public static RenderingMode Native()
    {
        return new RenderingMode { Type = RenderingModeType.Native };
    }

    public static RenderingMode Buffer(
        float scale = 1,
        TextureFilter textureFilter = TextureFilter.Nearest,
        bool pool = false
    )
    {
        return new RenderingMode
        {
            Type = RenderingModeType.Buffer,
            Scale = scale,
            TextureFilter = textureFilter,
            Pool = pool,
        };
    }
}

public enum RenderingModeType : sbyte
{
    Native,
    Buffer,
}
