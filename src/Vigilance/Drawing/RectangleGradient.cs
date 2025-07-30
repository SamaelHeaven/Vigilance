using Vigilance.Core;

namespace Vigilance.Drawing;

public sealed class RectangleGradient : IFullCloneable
{
    public Color TopLeftFill { get; set; } = Color.White;
    public Color BottomLeftFill { get; set; } = Color.White;
    public Color BottomRightFill { get; set; } = Color.White;
    public Color TopRightFill { get; set; } = Color.White;
    public Color Stroke { get; set; } = Color.Transparent;
    public float StrokeWidth { get; set; } = 0;
    public CameraProvider Camera { get; set; } = Core.Camera.Scene;

    public Color Fill
    {
        get => TopLeftFill.Blend(BottomLeftFill).Blend(BottomRightFill).Blend(TopRightFill);
        set
        {
            TopLeftFill = value;
            BottomLeftFill = value;
            BottomRightFill = value;
            TopRightFill = value;
        }
    }

    public Color TopFill
    {
        get => TopLeftFill.Blend(TopRightFill);
        set
        {
            TopLeftFill = value;
            TopRightFill = value;
        }
    }

    public Color BottomFill
    {
        get => BottomLeftFill.Blend(BottomRightFill);
        set
        {
            BottomLeftFill = value;
            BottomRightFill = value;
        }
    }

    public Color LeftFill
    {
        get => TopLeftFill.Blend(BottomLeftFill);
        set
        {
            TopLeftFill = value;
            BottomLeftFill = value;
        }
    }

    public Color RightFill
    {
        get => TopRightFill.Blend(BottomRightFill);
        set
        {
            TopRightFill = value;
            BottomRightFill = value;
        }
    }

    public override string ToString()
    {
        return Printer.Print(this);
    }
}
