namespace Vigilance.Systems;

public class AnchorSystem() : GameSystem(queryWithDisabled: true)
{
    public override void PreRender()
    {
        foreach (var (entity, anchorRef) in RefEntries<Anchor>())
        {
            var anchor = anchorRef.Read;
            entity.Position = anchor.Position - (anchor.Scale ?? entity.Scale) * 0.5f * anchor.Origin;
        }
    }
}
