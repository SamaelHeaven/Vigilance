using System.Runtime.InteropServices;

namespace Vigilance.Core;

public static partial class JSEngine
{
    public static string Run(string script)
    {
        if (!Platform.Web.IsCurrent())
            throw new PlatformNotSupportedException();
        var ptr = emscripten_run_script_string(script);
        return Marshal.PtrToStringUTF8(ptr) ?? "";
    }

    [LibraryImport("libc", StringMarshalling = StringMarshalling.Utf8)]
    private static partial nint emscripten_run_script_string(string script);
}
