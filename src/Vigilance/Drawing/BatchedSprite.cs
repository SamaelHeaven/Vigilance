namespace Vigilance.Drawing;

public record struct BatchedSprite(SpriteBatch Batch, SpriteInstance Instance)
    : IWriteImmutableComponent,
        ISkipSetEventIfEqualComponent;
