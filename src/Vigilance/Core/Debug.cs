using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Vigilance.Logging;

namespace Vigilance.Core;

public static class Debug
{
    [Conditional("DEBUG")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Assert(
        [DoesNotReturnIf(false)] bool condition,
        [CallerArgumentExpression(nameof(condition))] string message = ""
    )
    {
        if (!condition)
            Log.Fatal($"ASSERT: {message}\n{new StackTrace(true).ToString().TrimEnd()}");
    }
}
