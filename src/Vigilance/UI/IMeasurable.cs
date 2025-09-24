using Vigilance.Math;

namespace Vigilance.UI;

public interface IMeasurable
{
    Vector2 Measure(float width, MeasureMode widthMode, float height, MeasureMode heightMode);
}
