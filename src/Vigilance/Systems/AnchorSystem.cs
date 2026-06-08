using Vigilance.Core;

namespace Vigilance.Systems;

public class AnchorSystem : GameSystem
{
    public override void PostUpdate()
    {
        foreach (var (entity, anchor) in Entries<Anchor>())
            entity.Position = anchor.Position - (anchor.Scale ?? entity.Scale) * 0.5f * anchor.Origin;
    }
}
