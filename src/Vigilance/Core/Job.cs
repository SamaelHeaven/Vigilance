using System.Runtime.CompilerServices;

namespace Vigilance.Core;

internal readonly struct Job((object? Data, Delegate Action)? context, Delegate invoke)
{
    public static Job From(Action action)
    {
        return new Job(null, action);
    }

    public static Job From<T>(in T context, Action<T> action)
    {
        return new Job(
            (context, action),
            static (object context, Delegate action) => Unsafe.As<Delegate, Action<T>>(ref action).Invoke((T)context)
        );
    }

    public void Invoke()
    {
        if (context.HasValue)
        {
            var (data, action) = context.Value;
            Unsafe.As<Delegate, Action<object, Delegate>>(ref Unsafe.AsRef(in invoke)).Invoke(data!, action);
            return;
        }

        Unsafe.As<Delegate, Action>(ref Unsafe.AsRef(in invoke)).Invoke();
    }
}
