using Vigilance.Core;
using Vigilance.Logging;
using Vigilance.Math;

namespace Vigilance.Drawing;

public sealed class CircleGradient : Drawable<CircleGradient>, IFullCloneable
{
    public Color InnerFill { get; set; } = Drawing.DefaultFill;
    public Color OuterFill { get; set; } = Drawing.DefaultFill;
    public Color Stroke { get; set; } = Drawing.DefaultStroke;
    public float StrokeWidth { get; set; } = Drawing.DefaultStrokeWidth;
    public int Segments { get; set; } = 0;
    public DrawOrder DrawOrder { get; set; } = Drawing.DefaultOrder;

    public Color Fill
    {
        get => InnerFill.Blend(OuterFill);
        set
        {
            InnerFill = value;
            OuterFill = value;
        }
    }

    public override string ToString()
    {
        return ObjectPrinter.Print(this, ObjectPrinter.Exclude(nameof(Transform), nameof(Fill)), true);
    }

    public override void Render(Transform transform, Graphics graphics)
    {
        graphics.DrawCircleGradient(transform, this);
    }
}
