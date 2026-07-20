namespace Vigilance.Core;

public record struct Parent(ulong FirstChildId, ulong LastChildId) : IImmutableComponent, ISkipAddEventComponent;
