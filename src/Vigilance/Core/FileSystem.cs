using System.Runtime.InteropServices;
using System.Text;
using LinkDotNet.StringBuilder;
using Raylib_cs;
using Vigilance.Logging;

namespace Vigilance.Core;

public static unsafe partial class FileSystem
{
    public static string ApplicationDirectory { get; } =
        FormatPath(Utf8Ptr.GetString(Raylib.GetApplicationDirectory()));

    public static string WorkingDirectory => FormatPath(Utf8Ptr.GetString(Raylib.GetWorkingDirectory()));

    public static string[] DroppedFiles => !Raylib.IsFileDropped() ? [] : Raylib.GetDroppedFiles();

    internal static void Initialize()
    {
        var config = Game.Config.Take<FileSystemConfig>() ?? new FileSystemConfig();
        ChangeDirectory(config.WorkingDirectory);
    }

    public static string FormatPath(in ReadOnlySpan<char> path)
    {
        if (path.IsEmpty)
            return "";
        var trimmedPath = path.Trim();
        var initialCapacity = trimmedPath.Length;
        var sb =
            initialCapacity <= 256
                ? new ValueStringBuilder(stackalloc char[initialCapacity])
                : new ValueStringBuilder(initialCapacity);
        try
        {
            char? lastChar = null;
            FormatPathAppend(ref sb, ref lastChar, trimmedPath);
            return sb.ToString();
        }
        finally
        {
            sb.Dispose();
        }
    }

    public static Utf8Ptr FormatPathPtr(scoped in ReadOnlySpan<char> path)
    {
        if (path.IsEmpty)
            return "".ToUtf8Ptr();
        var trimmedPath = path.Trim();
        var initialCapacity = trimmedPath.Length;
        var sb =
            initialCapacity <= 256
                ? new ValueStringBuilder(stackalloc char[initialCapacity])
                : new ValueStringBuilder(initialCapacity);
        try
        {
            char? lastChar = null;
            FormatPathAppend(ref sb, ref lastChar, trimmedPath);
#pragma warning disable CS9080 // Use of variable in this context may expose referenced variables outside of their declaration scope
            return sb.AsSpan().ToUtf8Ptr();
#pragma warning restore CS9080 // Use of variable in this context may expose referenced variables outside of their declaration scope
        }
        finally
        {
            sb.Dispose();
        }
    }

    private static void FormatPathAppend(ref ValueStringBuilder sb, ref char? lastChar, in ReadOnlySpan<char> span)
    {
        foreach (var c in span)
        {
            var normalized = c == '\\' ? '/' : c;
            sb.Append(normalized);
            lastChar = normalized;
        }
    }

    public static string NormalizePath(string path)
    {
        return Path.IsPathFullyQualified(path)
            ? FormatPath(path)
            : FormatPath(Path.Combine(Utf8Ptr.GetString(Raylib.GetWorkingDirectory()), path));
    }

    public static bool ChangeDirectory(in ReadOnlySpan<char> path)
    {
        using var buffer = FormatPathPtr(path);
        if (!Raylib.DirectoryExists(buffer))
            return false;
        return Raylib.ChangeDirectory(buffer);
    }

    public static bool FileExists(in ReadOnlySpan<char> path)
    {
        if (path.IsEmpty)
            return false;
        using var buffer = FormatPathPtr(path);
        return Raylib.FileExists(buffer);
    }

    public static bool DirectoryExists(in ReadOnlySpan<char> path)
    {
        if (path.IsEmpty)
            return false;
        using var buffer = FormatPathPtr(path);
        return Raylib.DirectoryExists(buffer);
    }

    public static DateTime FileModTime(in ReadOnlySpan<char> path)
    {
        using var buffer = FormatPathPtr(path);
        return !Raylib.FileExists(buffer)
            ? DateTime.MinValue
            : DateTimeOffset.FromUnixTimeSeconds(GetFileModTime(buffer)).UtcDateTime;
    }

    public static int GetFileSize(in ReadOnlySpan<char> path)
    {
        using var buffer = FormatPathPtr(path);
        return !Raylib.FileExists(buffer) ? 0 : Raylib.GetFileLength(buffer);
    }

    public static bool TryReadText(in ReadOnlySpan<char> path, out string text)
    {
        var result = TryReadBytes(path, out var bytes);
        text = Encoding.UTF8.GetString(bytes);
        return result;
    }

    public static string ReadText(in ReadOnlySpan<char> path)
    {
        if (!TryReadText(path, out var text))
            Log.Warning($"FILEIO: [{FormatPath(path)}] Failed to read text file");
        return text;
    }

    public static bool WriteText(in ReadOnlySpan<char> path, in ReadOnlySpan<char> text)
    {
        using var pathBuffer = FormatPathPtr(path);
        using var textBuffer = text.ToUtf8Ptr();
        return Raylib.SaveFileText(pathBuffer, textBuffer);
    }

    public static bool TryReadBytes(in ReadOnlySpan<char> path, out byte[] bytes)
    {
        using var pathBuffer = FormatPathPtr(path);
        if (!Raylib.FileExists(pathBuffer))
        {
            bytes = [];
            return false;
        }

        int bytesRead;
        var data = Raylib.LoadFileData(pathBuffer, &bytesRead);
        if (data is null)
        {
            bytes = [];
            return false;
        }

        bytes = new byte[bytesRead];
        Marshal.Copy((nint)data, bytes, 0, bytesRead);
        Raylib.UnloadFileData(data);
        return true;
    }

    public static byte[] ReadBytes(in ReadOnlySpan<char> path)
    {
        if (!TryReadBytes(path, out var bytes))
            Log.Warning($"FILEIO: [{FormatPath(path)}] Failed to read file");
        return bytes;
    }

    public static bool WriteBytes(in ReadOnlySpan<char> path, in ReadOnlySpan<byte> bytes)
    {
        using var pathBuffer = FormatPathPtr(path);
        fixed (byte* byteBuffer = bytes)
        {
            return Raylib.SaveFileData(pathBuffer, byteBuffer, bytes.Length);
        }
    }

    public static string[] ScanDirectory(in ReadOnlySpan<char> path, bool recursive = false)
    {
        using var pathBuffer = FormatPathPtr(path);
        var filePathList = Raylib.LoadDirectoryFilesEx(pathBuffer, null, recursive);
        var count = filePathList.Count;
        var result = new string[count];
        for (var i = 0; i < count; i++)
            result[i] = FormatPath(Utf8Ptr.GetString(filePathList.Paths[i]));
        Raylib.UnloadDirectoryFiles(filePathList);
        return result;
    }

    [LibraryImport("raylib")]
    private static partial long GetFileModTime(sbyte* path);
}
