using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Raylib_cs;

namespace Vigilance.Core;

public static unsafe partial class FileSystem
{
    public static readonly string ApplicationDirectory = FormatPath(new string(Raylib.GetApplicationDirectory()));

    static FileSystem()
    {
        if (!Game.Running)
            Raylib.SetTraceLogLevel(TraceLogLevel.Error);
    }

    public static string WorkingDirectory => FormatPath(new string(Raylib.GetWorkingDirectory()));

    public static string FormatPath(string path)
    {
        return DuplicatedSlashRegex().Replace(path.Replace('\\', '/'), "/").Trim('/');
    }

    public static bool ChangeDirectory(string path)
    {
        path = FormatPath(path);
        if (!DirectoryExists(path))
            return false;
        using var buffer = path.ToUtf8Buffer();
        return Raylib.ChangeDirectory(buffer.AsPointer());
    }

    public static bool FileExists(string path)
    {
        path = FormatPath(path);
        if (path == "")
            return false;
        using var buffer = path.ToUtf8Buffer();
        return Raylib.FileExists(buffer.AsPointer());
    }

    public static bool DirectoryExists(string path)
    {
        path = FormatPath(path);
        if (path == "")
            return false;
        using var buffer = path.ToUtf8Buffer();
        return Raylib.DirectoryExists(buffer.AsPointer());
    }

    public static DateTime FileModTime(string path)
    {
        path = FormatPath(path);
        return !FileExists(path)
            ? DateTime.MinValue
            : DateTimeOffset.FromUnixTimeSeconds(Raylib.GetFileModTime(path)).UtcDateTime;
    }

    public static int GetFileSize(string path)
    {
        path = FormatPath(path);
        if (!FileExists(path))
            return 0;
        using var buffer = path.ToUtf8Buffer();
        return Raylib.GetFileLength(buffer.AsPointer());
    }

    public static string ReadText(string path)
    {
        path = FormatPath(path);
        if (!FileExists(path))
            return "";
        using var buffer = path.ToUtf8Buffer();
        var bytes = Raylib.LoadFileText(buffer.AsPointer());
        var result = new string(bytes);
        Raylib.UnloadFileText(bytes);
        return result;
    }

    public static bool WriteText(string path, string text)
    {
        path = FormatPath(path);
        using var pathBuffer = path.ToUtf8Buffer();
        using var textBuffer = text.ToUtf8Buffer();
        return Raylib.SaveFileText(pathBuffer.AsPointer(), textBuffer.AsPointer());
    }

    public static byte[] ReadBytes(string path)
    {
        path = FormatPath(path);
        if (!FileExists(path))
            return Array.Empty<byte>();
        var bytesRead = 0;
        var data = Raylib.LoadFileData(path, ref bytesRead);
        var bytes = new byte[bytesRead];
        Marshal.Copy((IntPtr)data, bytes, 0, bytesRead);
        Raylib.UnloadFileData(data);
        return bytes;
    }

    public static bool WriteBytes(string path, byte[] bytes)
    {
        path = FormatPath(path);
        using var pathBuffer = path.ToUtf8Buffer();
        fixed (byte* byteBuffer = bytes)
        {
            return Raylib.SaveFileData(pathBuffer.AsPointer(), byteBuffer, bytes.Length);
        }
    }

    public static string[] ScanDirectory(string path, bool recursive = false)
    {
        path = FormatPath(path);
        using var pathBuffer = path.ToUtf8Buffer();
        var filePathList = Raylib.LoadDirectoryFilesEx(pathBuffer.AsPointer(), null, recursive);
        var count = filePathList.Count;
        var result = new string[count];
        for (var i = 0; i < count; i++)
            result[i] = FormatPath(new string((sbyte*)filePathList.Paths[i]));
        Raylib.UnloadDirectoryFiles(filePathList);
        return result;
    }

    [GeneratedRegex(@"(\/{2,})")]
    private static partial Regex DuplicatedSlashRegex();
}
