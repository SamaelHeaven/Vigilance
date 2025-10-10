using Vigilance.Core;
using Vigilance.Drawing;
using Vigilance.Math;

namespace Vigilance.UI;

public class UIRing : UIContainer
{
    private Ring _ring = new();

    public Unit InnerRadius { get; set; }

    public float StartAngle
    {
        get => _ring.StartAngle;
        set => _ring.StartAngle = value;
    }

    public float EndAngle
    {
        get => _ring.EndAngle;
        set => _ring.EndAngle = value;
    }

    public Color Fill
    {
        get => _ring.Fill;
        set => _ring.Fill = value;
    }

    public Color Stroke
    {
        get => _ring.Stroke;
        set => _ring.Stroke = value;
    }

    public float StrokeWidth
    {
        get => _ring.StrokeWidth;
        set => _ring.StrokeWidth = value;
    }

    public DrawOrder DrawOrder
    {
        get => _ring.DrawOrder;
        set => _ring.DrawOrder = value;
    }

    protected override void RenderSelf(Graphics graphics, CameraProvider camera)
    {
        var position = LayoutPosition;
        var size = LayoutSize;
        var outerRadius = size.X.Min(size.Y) * 0.5f;
        _ring.InnerRadius = InnerRadius.Calculate(outerRadius);
        _ring.OuterRadius = outerRadius;
        _ring.Camera = camera;
        graphics.DrawRing(new Transform(position + size * 0.5f), _ring);
    }

    protected override object DeepClone()
    {
        var result = (UIRing)base.DeepClone();
        result._ring = _ring.DeepClone();
        return result;
    }
}
