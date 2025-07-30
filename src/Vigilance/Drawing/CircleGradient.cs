using Vigilance.Core;

namespace Vigilance.Drawing;

public sealed record CircleGradient : IFullCloneable
{
    public Color InnerFill { get; set; } = Color.White;
    public Color OuterFill { get; set; } = Color.White;
    public Color Stroke { get; set; } = Color.Transparent;
    public float StrokeWidth { get; set; } = 0;
    public CameraFunc? Camera { get; set; } = Core.Camera.Default;

    public Color Fill
    {
        get => InnerFill.Blend(OuterFill);
        set
        {
            InnerFill = value;
            OuterFill = value;
        }
    }
}
