using Vigilance.Math;

namespace Vigilance.Core;

public record struct RenderInterpolation(in Transform? Start, in Transform End)
    : IHiddenComponent,
        IWriteImmutableComponent,
        IRemoveImmutableComponent,
        ISkipAddEventComponent,
        ISkipRemoveEventComponent
{
    public RenderInterpolation()
        : this(null, new Transform()) { }
}
