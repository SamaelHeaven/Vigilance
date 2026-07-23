using Vigilance.Core;

namespace Vigilance.Drawing;

public static class Drawing
{
    private static DrawingConfig _config = new();

    public static Color DefaultFill { get; set; } = _config.DefaultFill;

    public static Color DefaultStroke { get; set; } = _config.DefaultStroke;

    public static float DefaultStrokeWidth { get; set; } = _config.DefaultStrokeWidth;

    public static DrawOrder DefaultOrder { get; set; } = _config.DefaultOrder;

    public static float DefaultRadius { get; set; } = _config.DefaultRadius;

    public static TextureWrap DefaultTextureWrap { get; set; } = _config.DefaultTextureWrap;

    public static TextureFilter DefaultTextureFilter { get; set; } = _config.DefaultTextureFilter;

    public static CameraProvider DefaultCamera { get; set; } = _config.DefaultCamera;

    public static Texture DefaultTexture { get; set; } = null!;

    public static bool DefaultCulling { get; set; } = _config.DefaultCulling;

    public static BlendMode DefaultBlendMode { get; set; } = _config.DefaultBlendMode;

    public static Shader DefaultShader { get; set; } = null!;

    public static float SegmentsErrorRate { get; set; } = _config.SegmentsErrorRate;

    public static TimeSpan RenderTexturePoolLifetime { get; set; } = _config.RenderTexturePoolLifetime;

    public static int RenderTexturePoolRoundUpToMultipleOf { get; set; } = _config.RenderTexturePoolRoundUpToMultipleOf;

    public static int CalculateSegments(
        float radius,
        float startAngle,
        float endAngle,
        int segments,
        float? errorRate = null
    )
    {
        if (radius <= 0)
            radius = 0.1f;
        if (endAngle < startAngle)
            (startAngle, endAngle) = (endAngle, startAngle);
        var minSegments = (int)MathF.Ceiling((endAngle - startAngle) / 90f);
        if (segments >= minSegments)
            return segments;
        var th = MathF.Acos(2f * MathF.Pow(1f - (errorRate ?? SegmentsErrorRate) / radius, 2f) - 1f);
        segments = (int)MathF.Ceiling((endAngle - startAngle) * (2f * MathF.PI / th) / 360f);
        if (segments <= 0)
            segments = minSegments;
        return segments;
    }

    internal static void Initialize()
    {
        _config = Game.Config.Take<DrawingConfig>() ?? _config;
        DefaultFill = _config.DefaultFill;
        DefaultStroke = _config.DefaultStroke;
        DefaultStrokeWidth = _config.DefaultStrokeWidth;
        DefaultOrder = _config.DefaultOrder;
        DefaultRadius = _config.DefaultRadius;
        DefaultTextureWrap = _config.DefaultTextureWrap;
        DefaultTextureFilter = _config.DefaultTextureFilter;
        DefaultCamera = _config.DefaultCamera;
        DefaultCulling = _config.DefaultCulling;
        DefaultBlendMode = _config.DefaultBlendMode;
        SegmentsErrorRate = _config.SegmentsErrorRate;
        RenderTexturePoolLifetime = _config.RenderTexturePoolLifetime;
        RenderTexturePoolRoundUpToMultipleOf = _config.RenderTexturePoolRoundUpToMultipleOf;
        DefaultTexture = _config.DefaultTexture.SafeInvoke();
        DefaultShader = _config.DefaultShader.SafeInvoke();
    }
}

public sealed class DrawingConfig
{
    public Color DefaultFill { get; set; } = Color.White;
    public Color DefaultStroke { get; set; } = Color.Transparent;
    public float DefaultStrokeWidth { get; set; } = 0;
    public DrawOrder DefaultOrder { get; set; } = DrawOrder.FillThenStroke;
    public float DefaultRadius { get; set; } = 0;
    public TextureWrap DefaultTextureWrap { get; set; } = TextureWrap.Repeat;
    public TextureFilter DefaultTextureFilter { get; set; } = TextureFilter.Nearest;
    public CameraProvider DefaultCamera { get; set; } = Camera.Scene;
    public Func<Texture> DefaultTexture { get; set; } = () => Texture.Empty;
    public bool DefaultCulling { get; set; } = false;
    public BlendMode DefaultBlendMode { get; set; } = BlendMode.Alpha;
    public Func<Shader> DefaultShader { get; set; } = () => Shader.Default;
    public float SegmentsErrorRate { get; set; } = 0.5f;
    public TimeSpan RenderTexturePoolLifetime { get; set; } = TimeSpan.FromSeconds(5);
    public int RenderTexturePoolRoundUpToMultipleOf { get; set; } = 128;
}

public static class DrawingConfigExtensions
{
    public static ConfigBuilder Drawing(this ConfigBuilder builder, Action<DrawingConfig> config)
    {
        return builder.Add(config);
    }
}
