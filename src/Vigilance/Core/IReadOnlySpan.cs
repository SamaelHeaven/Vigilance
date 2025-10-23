namespace Vigilance.Core;

public interface IReadOnlySpan<T>
{
    ReadOnlySpan<T> AsSpan();
}
