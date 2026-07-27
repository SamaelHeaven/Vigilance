namespace Vigilance.Systems;

public sealed class YSortSystem(float offset) : GameSystem(queryWithDisabled: true)
{
    public YSortSystem()
        : this(0) { }

    public float Offset { get; set; } = offset;

    public override void PreRender()
    {
        foreach (var (entity, ySort) in Entries<YSort>())
            entity.ZIndex = (int)(entity.WorldPosition.Y + Offset + ySort.Offset).Floor();
    }
}
