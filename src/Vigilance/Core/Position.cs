namespace Vigilance.Core;

public record struct Position(Vector2 Value)
    : IWriteImmutableComponent,
        IRemoveImmutableComponent,
        ISkipAddEventComponent,
        ISkipRemoveEventComponent
{
    public static implicit operator Vector2(Position position)
    {
        return position.Value;
    }

    public static implicit operator Position(Vector2 position)
    {
        return new Position(position);
    }
}
