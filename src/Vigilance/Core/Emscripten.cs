using System.Runtime.InteropServices;

namespace Vigilance.Core;

internal static unsafe partial class Emscripten
{
    private const string LibraryName = "libc";

    [LibraryImport(
        LibraryName,
        StringMarshalling = StringMarshalling.Utf8,
        EntryPoint = "emscripten_run_script_string"
    )]
    public static partial nint RunScriptString(string script);

    [LibraryImport(LibraryName, EntryPoint = "emscripten_set_main_loop")]
    public static partial void SetMainLoop(delegate* unmanaged[Cdecl]<void> func, int fps, sbyte simulateInfiniteLoop);
}
