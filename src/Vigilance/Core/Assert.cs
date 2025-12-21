using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Vigilance.Logging;

namespace Vigilance.Core;

public static class Assert
{
    [Conditional("DEBUG")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Ensure(
        [DoesNotReturnIf(false)] bool condition,
        [CallerArgumentExpression(nameof(condition))] string message = ""
    )
    {
        if (!condition)
            Log.Fatal($"ASSERT: {message}\n{new StackTrace(true).ToString().TrimEnd()}");
    }
}
