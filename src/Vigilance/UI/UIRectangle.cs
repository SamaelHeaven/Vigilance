using Vigilance.Core;
using Vigilance.Drawing;

namespace Vigilance.UI;

public class UIRectangle : UIContainer
{
    private Rectangle _rectangle = new();

    public Color Fill
    {
        get => _rectangle.Fill;
        set => _rectangle.Fill = value;
    }

    public Color Stroke
    {
        get => _rectangle.Stroke;
        set => _rectangle.Stroke = value;
    }

    public float StrokeWidth
    {
        get => _rectangle.StrokeWidth;
        set => _rectangle.StrokeWidth = value;
    }

    public float Roundness
    {
        get => _rectangle.Roundness;
        set => _rectangle.Roundness = value;
    }

    public override object DeepClone()
    {
        var result = (UIRectangle)base.DeepClone();
        result._rectangle = _rectangle.DeepClone();
        return result;
    }

    public override void Render(Graphics graphics, CameraFunc? camera)
    {
        _rectangle.Camera = camera;
        graphics.DrawRectangle(LayoutPosition, LayoutSize, _rectangle);
        base.Render(graphics, camera);
    }
}
