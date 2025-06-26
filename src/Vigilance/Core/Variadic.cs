using System.Runtime.InteropServices;

namespace Vigilance.Core;

public static partial class Variadic
{
    private const string Msvcrt = "msvcrt";
    private const string Libc = "libc";
    private const string LibSystem = "libSystem";

    public static string FormatString(nint format, nint args)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            return AppleFormatString(format, args);
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) && nint.Size == 8)
            return LinuxX64FormatString(format, args);
        var byteLength = VsnPrintf(nint.Zero, nuint.Zero, format, args) + 1;
        if (byteLength <= 1)
            return string.Empty;
        var buffer = Marshal.AllocHGlobal(byteLength);
        VsPrintf(buffer, format, args);
        var result = Marshal.PtrToStringUTF8(buffer) ?? "";
        Marshal.FreeHGlobal(buffer);
        return result;
    }

    private static string AppleFormatString(nint format, nint args)
    {
        var buffer = nint.Zero;
        try
        {
            var count = VasPrintfApple(ref buffer, format, args);
            if (count == -1)
                return string.Empty;
            return Marshal.PtrToStringUTF8(buffer) ?? string.Empty;
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }

    private static string LinuxX64FormatString(nint format, nint args)
    {
        var listStructure = Marshal.PtrToStructure<VaListLinuxX64>(args);
        var listPointer = Marshal.AllocHGlobal(Marshal.SizeOf(listStructure));
        Marshal.StructureToPtr(listStructure, listPointer, false);
        var byteLength = VsnPrintfLinux(nint.Zero, nuint.Zero, format, listPointer) + 1;
        Marshal.StructureToPtr(listStructure, listPointer, false);
        var utf8Buffer = Marshal.AllocHGlobal(byteLength);
        VsPrintfLinux(utf8Buffer, format, listPointer);
        var result = Marshal.PtrToStringUTF8(utf8Buffer) ?? "";
        Marshal.FreeHGlobal(listPointer);
        Marshal.FreeHGlobal(utf8Buffer);
        return result;
    }

    private static int VsnPrintf(nint buffer, nuint size, nint format, nint args)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return VsnPrintfWindows(buffer, size, format, args);
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            return VsnPrintfLinux(buffer, size, format, args);
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Create("ANDROID")))
            return VsPrintfLinux(buffer, format, args);
        return -1;
    }

    private static void VsPrintf(nint buffer, nint format, nint args)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            VsPrintfWindows(buffer, format, args);
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            VsPrintfLinux(buffer, format, args);
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Create("ANDROID")))
            VsPrintfLinux(buffer, format, args);
    }

    [LibraryImport(LibSystem, EntryPoint = "vasprintf")]
    private static partial int VasPrintfApple(ref nint buffer, nint format, nint args);

    [LibraryImport(Libc, EntryPoint = "vsprintf")]
    private static partial int VsPrintfLinux(nint buffer, nint format, nint args);

    [LibraryImport(Msvcrt, EntryPoint = "vsprintf")]
    private static partial void VsPrintfWindows(nint buffer, nint format, nint args);

    [LibraryImport(Libc, EntryPoint = "vsnprintf")]
    private static partial int VsnPrintfLinux(nint buffer, nuint size, nint format, nint args);

    [LibraryImport(Msvcrt, EntryPoint = "vsnprintf")]
    private static partial int VsnPrintfWindows(nint buffer, nuint size, nint format, nint args);

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    private struct VaListLinuxX64
    {
        private uint _gpOffset;
        private uint _fpOffset;
        private nint _overflowArgArea;
        private nint _regSaveArea;
    }
}
