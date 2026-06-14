using System.Runtime.InteropServices;
using System.Text;
using LinkDotNet.StringBuilder;
using Raylib_cs;
using Vigilance.Collections;
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

    public static string FormatPath(string path)
    {
        if (path.IsEmpty)
            return "";
        var trimmedPath = path.AsSpan().Trim();
        var initialCapacity = trimmedPath.Length;
        using var sb =
            initialCapacity <= 256
                ? new ValueStringBuilder(stackalloc char[initialCapacity])
                : new ValueStringBuilder(initialCapacity);
        char? lastChar = null;
        foreach (var c in trimmedPath)
        {
            var normalized = c == '\\' ? '/' : c;
            if (normalized == '/' && lastChar == '/')
                continue;
            sb.Append(normalized);
            lastChar = normalized;
        }

        return sb.AsSpan().Trim('/').ToString();
    }

    public static string NormalizePath(string path)
    {
        return FormatPath(Path.Combine(WorkingDirectory, path));
    }

    public static bool ChangeDirectory(string path)
    {
        path = FormatPath(path);
        using var buffer = path.ToUtf8Ptr();
        if (!Raylib.DirectoryExists(buffer))
            return false;
        return Raylib.ChangeDirectory(buffer);
    }

    public static bool FileExists(string path)
    {
        path = FormatPath(path);
        if (path.IsEmpty)
            return false;
        using var buffer = path.ToUtf8Ptr();
        return Raylib.FileExists(buffer);
    }

    public static bool DirectoryExists(string path)
    {
        path = FormatPath(path);
        if (path.IsEmpty)
            return false;
        using var buffer = path.ToUtf8Ptr();
        return Raylib.DirectoryExists(buffer);
    }

    public static DateTime FileModTime(string path)
    {
        path = FormatPath(path);
        using var buffer = path.ToUtf8Ptr();
        return !Raylib.FileExists(buffer)
            ? DateTime.MinValue
            : DateTimeOffset.FromUnixTimeSeconds(GetFileModTime(buffer)).UtcDateTime;
    }

    public static int GetFileSize(string path)
    {
        path = FormatPath(path);
        using var buffer = path.ToUtf8Ptr();
        return !Raylib.FileExists(path) ? 0 : Raylib.GetFileLength(buffer);
    }

    public static bool TryReadText(string path, out string text)
    {
        var result = TryReadBytes(path, out var bytes);
        text = Encoding.UTF8.GetString(bytes);
        return result;
    }

    public static string ReadText(string path)
    {
        if (!TryReadText(path, out var text))
            Log.Warning($"FILEIO: [{FormatPath(path)}] Failed to read text file");
        return text;
    }

    public static bool WriteText(string path, string text)
    {
        path = FormatPath(path);
        using var pathBuffer = path.ToUtf8Ptr();
        using var textBuffer = text.ToUtf8Ptr();
        return Raylib.SaveFileText(pathBuffer, textBuffer);
    }

    public static bool TryReadBytes(string path, out byte[] bytes)
    {
        path = FormatPath(path);
        using var pathBuffer = path.ToUtf8Ptr();
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

    public static byte[] ReadBytes(string path)
    {
        if (!TryReadBytes(path, out var bytes))
            Log.Warning($"FILEIO: [{FormatPath(path)}] Failed to read file");
        return bytes;
    }

    public static bool WriteBytes(string path, IEnumerable<byte> bytes)
    {
        return WriteBytesSpan(path, bytes.AsSpan());
    }

    public static bool WriteBytesSpan(string path, in ReadOnlySpan<byte> bytes)
    {
        path = FormatPath(path);
        using var pathBuffer = path.ToUtf8Ptr();
        fixed (byte* byteBuffer = bytes)
        {
            return Raylib.SaveFileData(pathBuffer, byteBuffer, bytes.Length);
        }
    }

    public static string[] ScanDirectory(string path, bool recursive = false)
    {
        path = FormatPath(path);
        using var pathBuffer = path.ToUtf8Ptr();
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
