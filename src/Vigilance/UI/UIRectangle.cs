using Vigilance.Core;
using Vigilance.Drawing;
using Vigilance.Math;

namespace Vigilance.UI;

public class UIRectangle : UIContainer
{
    private readonly Rectangle _rectangle = new();

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

    public override void Render(Graphics graphics, CameraFunc? camera)
    {
        _rectangle.Camera = camera;
        graphics.DrawRectangle(new Transform(LayoutPosition + LayoutSize * 0.5f, LayoutSize), _rectangle);
        base.Render(graphics, camera);
    }
}
