using Vigilance.Core;
using Vigilance.Logging;

namespace Vigilance.Drawing;

public sealed class Circle : IFullCloneable
{
    public Circle() { }

    public Circle(Color fill)
    {
        Fill = fill;
    }

    public Color Fill { get; set; } = Color.White;
    public Color Stroke { get; set; } = Color.Transparent;
    public float StrokeWidth { get; set; } = 0;
    public CameraProvider Camera { get; set; } = Core.Camera.Scene;

    public override string ToString()
    {
        return ObjectPrinter.Print(this);
    }
}
