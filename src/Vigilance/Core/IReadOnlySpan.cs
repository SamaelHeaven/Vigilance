namespace Vigilance.Core;

public interface IReadOnlySpan<T> : IEnumerable<T>
{
    public ReadOnlySpan<T> AsSpan();
}
