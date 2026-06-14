using Vigilance.Core;
using Vigilance.Math;

namespace Vigilance.Systems;

public sealed class YSortSystem(float offset = 0) : GameSystem(queryWithDisabled: true)
{
    public float Offset { get; set; } = offset;

    public override void PreRender()
    {
        foreach (var (entity, ySort) in Entries<YSort>())
            entity.ZIndex = (int)(entity.WorldPosition.Y + Offset + ySort.Offset).Floor();
    }
}
