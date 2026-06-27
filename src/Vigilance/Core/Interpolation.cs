using Vigilance.Math;

namespace Vigilance.Core;

public record struct Interpolation(in Transform? Start, in Transform End)
    : IHiddenComponent,
        IWriteImmutableComponent,
        IRemoveImmutableComponent,
        ISkipAddEventComponent,
        ISkipRemoveEventComponent
{
    public Interpolation()
        : this(null, new Transform()) { }
}
