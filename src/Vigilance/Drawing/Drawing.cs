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

    public static float DefaultRoundness
    {
        get => _config.DefaultRoundness;
        set => _config.DefaultRoundness = value;
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

    internal static void Initialize()
    {
        if (Game.Config.TryTake(out DrawingConfig config))
            _config = config;
        DefaultTexture = _config.DefaultTexture.Invoke();
    }
}

public sealed class DrawingConfig
{
    public Color DefaultFill { get; set; } = Color.White;
    public Color DefaultStroke { get; set; } = Color.Transparent;
    public float DefaultStrokeWidth { get; set; } = 0;
    public float DefaultRoundness { get; set; } = 0;
    public Interpolation DefaultInterpolation { get; set; } = Interpolation.Nearest;
    public CameraProvider DefaultCamera { get; set; } = Camera.Scene;
    public Func<Texture> DefaultTexture { get; set; } = () => Texture.Empty;
}

public static class DrawingConfigExtensions
{
    public static ConfigBuilder Drawing(this ConfigBuilder builder, DrawingConfig config)
    {
        return builder.Add(config);
    }
}
