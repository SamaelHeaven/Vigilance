namespace Vigilance.Core;

public record struct Parent(EntityId FirstChildId, EntityId LastChildId) : IImmutableComponent, ISkipAddEventComponent;
