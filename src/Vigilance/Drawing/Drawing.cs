using Vigilance.Core;

namespace Vigilance.Drawing;

public static class Drawing
{
    private static DrawingConfig _config = new();

    public static Color DefaultFill
    {
        get => _config.DefaultFill;
        set => _config.DefaultFill = value;
    }

    public static Color DefaultStroke
    {
        get => _config.DefaultStroke;
        set => _config.DefaultStroke = value;
    }

    public static float DefaultStrokeWidth
    {
        get => _config.DefaultStrokeWidth;
        set => _config.DefaultStrokeWidth = value;
    }

    public static DrawOrder DefaultOrder
    {
        get => _config.DefaultOrder;
        set => _config.DefaultOrder = value;
    }

    public static float DefaultRadius
    {
        get => _config.DefaultRadius;
        set => _config.DefaultRadius = value;
    }

    public static TextureWrap DefaultTextureWrap
    {
        get => _config.DefaultTextureWrap;
        set => _config.DefaultTextureWrap = value;
    }

    public static Interpolation DefaultInterpolation
    {
        get => _config.DefaultInterpolation;
        set => _config.DefaultInterpolation = value;
    }

    public static CameraProvider DefaultCamera
    {
        get => _config.DefaultCamera;
        set => _config.DefaultCamera = value;
    }

    public static Texture DefaultTexture { get; set; } = null!;

    public static bool DefaultCulling
    {
        get => _config.DefaultCulling;
        set => _config.DefaultCulling = value;
    }

    public static BlendMode DefaultBlendMode
    {
        get => _config.DefaultBlendMode;
        set => _config.DefaultBlendMode = value;
    }

    public static Shader DefaultShader { get; set; } = null!;

    public static float SegmentsErrorRate
    {
        get => _config.SegmentsErrorRate;
        set => _config.SegmentsErrorRate = value;
    }

    public static TimeSpan RenderTexturePoolLifetime
    {
        get => _config.RenderTexturePoolLifetime;
        set => _config.RenderTexturePoolLifetime = value;
    }

    public static int CalculateSegments(float radius, float startAngle, float endAngle, int segments)
    {
        if (radius <= 0)
            radius = 0.1f;
        if (endAngle < startAngle)
            (startAngle, endAngle) = (endAngle, startAngle);
        var minSegments = (int)MathF.Ceiling((endAngle - startAngle) / 90f);
        if (segments >= minSegments)
            return segments;
        var th = MathF.Acos(2f * MathF.Pow(1f - SegmentsErrorRate / radius, 2f) - 1f);
        segments = (int)MathF.Ceiling((endAngle - startAngle) * (2f * MathF.PI / th) / 360f);
        if (segments <= 0)
            segments = minSegments;
        return segments;
    }

    internal static void Initialize()
    {
        _config = Game.Config.Take<DrawingConfig>() ?? _config;
        DefaultTexture = _config.DefaultTexture.Invoke();
        DefaultShader = _config.DefaultShader.Invoke();
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
    public Interpolation DefaultInterpolation { get; set; } = Interpolation.Nearest;
    public CameraProvider DefaultCamera { get; set; } = Camera.Scene;
    public Func<Texture> DefaultTexture { get; set; } = () => Texture.Empty;
    public bool DefaultCulling { get; set; } = false;
    public BlendMode DefaultBlendMode { get; set; } = BlendMode.Alpha;
    public Func<Shader> DefaultShader { get; set; } = () => Shader.Default;
    public float SegmentsErrorRate { get; set; } = 0.25f;
    public TimeSpan RenderTexturePoolLifetime { get; set; } = TimeSpan.FromSeconds(6);
}

public static class DrawingConfigExtensions
{
    public static ConfigBuilder Drawing(this ConfigBuilder builder, Action<DrawingConfig> config)
    {
        return builder.Add(config);
    }
}
