using Vigilance.Core;
using Vigilance.Math;

namespace Vigilance.Systems;

public sealed class YSortSystem(float offset = 0) : GameSystem
{
    public float Offset { get; set; } = offset;

    public override void BeginRender()
    {
        foreach (var (entity, ySort) in Scene.Entries<YSort>())
            entity.ZIndex = (int)(entity.WorldPosition.Y + Offset + ySort.Offset).Round();
    }
}
