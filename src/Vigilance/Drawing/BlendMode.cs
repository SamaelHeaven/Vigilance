using System.ComponentModel;

namespace Vigilance.Drawing;

public enum BlendFactor : byte
{
    Zero,
    One,
    SrcColor,
    OneMinusSrcColor,
    SrcAlpha,
    OneMinusSrcAlpha,
    DstAlpha,
    OneMinusDstAlpha,
    DstColor,
    OneMinusDstColor,
}

public enum BlendEquation : byte
{
    Add,
    Subtract,
    ReverseSubtract,
    Min,
    Max,
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

    public static readonly BlendMode Replace = new(
        BlendFactor.One,
        BlendFactor.Zero,
        BlendFactor.One,
        BlendFactor.Zero,
        BlendEquation.Add,
        BlendEquation.Add
    );
}

public static class BlendModeExtensions
{
    extension(BlendFactor factor)
    {
        public int ToGL()
        {
            return factor switch
            {
                BlendFactor.Zero => 0,
                BlendFactor.One => 1,
                BlendFactor.SrcColor => 0x0300,
                BlendFactor.OneMinusSrcColor => 0x0301,
                BlendFactor.SrcAlpha => 0x0302,
                BlendFactor.OneMinusSrcAlpha => 0x0303,
                BlendFactor.DstAlpha => 0x0304,
                BlendFactor.OneMinusDstAlpha => 0x0305,
                BlendFactor.DstColor => 0x0306,
                BlendFactor.OneMinusDstColor => 0x0307,
                _ => throw new InvalidEnumArgumentException(nameof(factor), (int)factor, typeof(BlendFactor)),
            };
        }
    }

    extension(BlendEquation equation)
    {
        public int ToGL()
        {
            return equation switch
            {
                BlendEquation.Add => 0x8006,
                BlendEquation.Subtract => 0x800A,
                BlendEquation.ReverseSubtract => 0x800B,
                BlendEquation.Min => 0x8007,
                BlendEquation.Max => 0x8008,
                _ => throw new InvalidEnumArgumentException(nameof(equation), (int)equation, typeof(BlendEquation)),
            };
        }
    }
}
