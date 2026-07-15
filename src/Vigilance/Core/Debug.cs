using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Vigilance.Core;

public static class Debug
{
    [Conditional("VIGILANCE_ASSERTS")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Assert(
        [DoesNotReturnIf(false)] bool condition,
        [CallerArgumentExpression(nameof(condition))] string message = ""
    )
    {
        if (!condition)
            throw new AssertionException(message);
    }
}
