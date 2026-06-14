namespace Vigilance.Core;

public record struct ZIndex(int Value)
    : IWriteImmutableComponent,
        IRemoveImmutableComponent,
        ISkipAddEventComponent,
        ISkipRemoveEventComponent
{
    public static implicit operator int(ZIndex zIndex)
    {
        return zIndex.Value;
    }

    public static implicit operator ZIndex(int zIndex)
    {
        return new ZIndex(zIndex);
    }
}
