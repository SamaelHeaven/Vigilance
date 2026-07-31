namespace Vigilance.Core;

public sealed class Query : GameSystem
{
    public Query(Scene scene, bool withDisabled = false, bool deferred = true)
        : base(queryWithDisabled: withDisabled, queryDeferred: deferred)
    {
        Scene = scene;
    }

    public new bool QueryDeferred
    {
        get => base.QueryDeferred;
        set => base.QueryDeferred = value;
    }
}
