namespace Vigilance.Core;

public record struct Rotation(float Value)
    : IWriteImmutableComponent,
        IRemoveImmutableComponent,
        ISkipAddEventComponent,
        ISkipRemoveEventComponent
{
    public static implicit operator float(Rotation rotation)
    {
        return rotation.Value;
    }

    public static implicit operator Rotation(float rotation)
    {
        return new Rotation(rotation);
    }
}
