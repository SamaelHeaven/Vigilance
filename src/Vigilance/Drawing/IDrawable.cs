using Vigilance.Math;

namespace Vigilance.Drawing;

public interface IDrawable
{
    void Render(Transform transform, Graphics graphics);
}
