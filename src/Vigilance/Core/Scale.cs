namespace Vigilance.Core;

public record struct Scale(Vector2 Value)
    : IWriteImmutableComponent,
        IRemoveImmutableComponent,
        ISkipAddEventComponent,
        ISkipRemoveEventComponent
{
    public Scale()
        : this(Vector2.One) { }

    public static implicit operator Vector2(Scale scale)
    {
        return scale.Value;
    }

    public static implicit operator Scale(Vector2 scale)
    {
        return new Scale(scale);
    }
}
