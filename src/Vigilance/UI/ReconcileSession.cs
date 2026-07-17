using Vigilance.Collections;

namespace Vigilance.UI;

internal sealed class ReconcileSession
{
    [ThreadStatic]
    private static ReconcileSession? _pool;

    private ValueList<UIParent> _parents = [];
    private ReconcileSession? _previous;

    [field: ThreadStatic]
    public static ReconcileSession? Current { get; private set; }

    public static ReconcileSession Begin()
    {
        var session = _pool ?? new ReconcileSession();
        _pool = null;
        session._previous = Current;
        Current = session;
        return session;
    }

    public void Register(UIParent parent)
    {
        _parents.Add(parent);
    }

    public void End()
    {
        foreach (var parent in _parents)
            parent.EndReconcile();
        _parents.Clear();
        Current = _previous;
        _previous = null;
        _pool = this;
    }
}
