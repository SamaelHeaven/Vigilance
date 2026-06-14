namespace Vigilance.Drawing;

public enum BlendFactor
{
    Zero = 0,
    One = 1,
    SrcColor = 0x0300,
    OneMinusSrcColor = 0x0301,
    SrcAlpha = 0x0302,
    OneMinusSrcAlpha = 0x0303,
    DstAlpha = 0x0304,
    OneMinusDstAlpha = 0x0305,
    DstColor = 0x0306,
    OneMinusDstColor = 0x0307,
}

public enum BlendEquation
{
    Add = 0x8006,
    Subtract = 0x800A,
    ReverseSubtract = 0x800B,
    Min = 0x8007,
    Max = 0x8008,
}

public record struct BlendMode(
    BlendFactor SrcRgb,
    BlendFactor DstRgb,
    BlendFactor SrcAlpha,
    BlendFactor DstAlpha,
    BlendEquation EqRgb,
    BlendEquation EqAlpha
)
{
    public static readonly BlendMode Alpha = new(
        BlendFactor.SrcAlpha,
        BlendFactor.OneMinusSrcAlpha,
        BlendFactor.One,
        BlendFactor.OneMinusSrcAlpha,
        BlendEquation.Add,
        BlendEquation.Add
    );

    public static readonly BlendMode Additive = new(
        BlendFactor.SrcAlpha,
        BlendFactor.One,
        BlendFactor.One,
        BlendFactor.One,
        BlendEquation.Add,
        BlendEquation.Add
    );

    public static readonly BlendMode Multiply = new(
        BlendFactor.DstColor,
        BlendFactor.Zero,
        BlendFactor.One,
        BlendFactor.Zero,
        BlendEquation.Add,
        BlendEquation.Add
    );

    public static readonly BlendMode Screen = new(
        BlendFactor.One,
        BlendFactor.OneMinusSrcColor,
        BlendFactor.One,
        BlendFactor.OneMinusSrcAlpha,
        BlendEquation.Add,
        BlendEquation.Add
    );

    public static readonly BlendMode Subtract = new(
        BlendFactor.SrcAlpha,
        BlendFactor.OneMinusSrcAlpha,
        BlendFactor.One,
        BlendFactor.OneMinusSrcAlpha,
        BlendEquation.Subtract,
        BlendEquation.Subtract
    );

    public static readonly BlendMode PremultipliedAlpha = new(
        BlendFactor.One,
        BlendFactor.OneMinusSrcAlpha,
        BlendFactor.One,
        BlendFactor.OneMinusSrcAlpha,
        BlendEquation.Add,
        BlendEquation.Add
    );

    public static readonly BlendMode ReverseSubtract = new(
        BlendFactor.SrcAlpha,
        BlendFactor.OneMinusSrcAlpha,
        BlendFactor.One,
        BlendFactor.OneMinusSrcAlpha,
        BlendEquation.ReverseSubtract,
        BlendEquation.ReverseSubtract
    );

    public static readonly BlendMode Min = new(
        BlendFactor.One,
        BlendFactor.One,
        BlendFactor.One,
        BlendFactor.One,
        BlendEquation.Min,
        BlendEquation.Min
    );

    public static readonly BlendMode Max = new(
        BlendFactor.One,
        BlendFactor.One,
        BlendFactor.One,
        BlendFactor.One,
        BlendEquation.Max,
        BlendEquation.Max
    );
}
