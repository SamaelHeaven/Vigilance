namespace Vigilance.Core;

public sealed class AssertionException : Exception
{
    public AssertionException(string message)
        : base(message) { }
}
