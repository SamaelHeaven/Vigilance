using System.Runtime.InteropServices;
using System.Text;
using Raylib_cs;
using Vigilance.Logging;

namespace Vigilance.Core;

public static unsafe partial class FileSystem
{
    public static string ApplicationDirectory { get; } = Utf8Ptr.GetString(Raylib.GetApplicationDirectory());

    public static string WorkingDirectory => Utf8Ptr.GetString(Raylib.GetWorkingDirectory());

    public static string[] DroppedFiles
    {
        get
        {
            if (!Raylib.IsFileDropped())
                return [];
            var filePathList = Raylib.LoadDroppedFiles();
            var files = new string[filePathList.Count];
            for (uint i = 0; i < filePathList.Count; i++)
                files[i] = filePathList[i];
            Raylib.UnloadDroppedFiles(filePathList);
            return files;
        }
    }

    internal static void Initialize()
    {
        var config = Game.Config.Take<FileSystemConfig>() ?? new FileSystemConfig();
        ChangeDirectory(config.WorkingDirectory);
    }

    public static string NormalizePath(string path)
    {
        return Path.GetFullPath(path, WorkingDirectory);
    }

    public static bool ChangeDirectory(string path)
    {
        if (path.IsEmpty)
            return false;
        using var buffer = path.ToUtf8Ptr();
        if (!Raylib.DirectoryExists(buffer))
            return false;
        return Raylib.ChangeDirectory(buffer);
    }

    public static bool FileExists(string path)
    {
        if (path.IsEmpty)
            return false;
        using var buffer = path.ToUtf8Ptr();
        return Raylib.FileExists(buffer);
    }

    public static bool DirectoryExists(string path)
    {
        if (path.IsEmpty)
            return false;
        using var buffer = path.ToUtf8Ptr();
        return Raylib.DirectoryExists(buffer);
    }

    public static DateTime FileModTime(string path)
    {
        if (path.IsEmpty)
            return DateTime.MinValue;
        using var buffer = path.ToUtf8Ptr();
        return !Raylib.FileExists(buffer)
            ? DateTime.MinValue
            : DateTimeOffset.FromUnixTimeSeconds(GetFileModTime(buffer)).UtcDateTime;
    }

    public static int GetFileSize(string path)
    {
        if (path.IsEmpty)
            return 0;
        using var buffer = path.ToUtf8Ptr();
        return !Raylib.FileExists(buffer) ? 0 : Raylib.GetFileLength(buffer);
    }

    public static bool TryReadText(string path, out string text)
    {
        var result = TryReadBytes(path, out var bytes);
        text = result ? Encoding.UTF8.GetString(bytes) : null!;
        return result;
    }

    public static string ReadText(string path)
    {
        if (!TryReadText(path, out var text))
            Log.Warning($"FILEIO: [{NormalizePath(path)}] Failed to read text file");
        return text;
    }

    public static bool WriteText(string path, string text)
    {
        if (path.IsEmpty)
            return false;
        using var pathBuffer = path.ToUtf8Ptr();
        using var textBuffer = text.ToUtf8Ptr();
        return Raylib.SaveFileText(pathBuffer, textBuffer);
    }

    public static bool TryReadBytes(string path, out byte[] bytes)
    {
        if (path.IsEmpty)
        {
            bytes = [];
            return false;
        }

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
            Log.Warning($"FILEIO: [{NormalizePath(path)}] Failed to read file");
        return bytes;
    }

    public static bool WriteBytes(string path, in ReadOnlySpan<byte> bytes)
    {
        if (path.IsEmpty)
            return false;
        using var pathBuffer = path.ToUtf8Ptr();
        fixed (byte* byteBuffer = bytes)
        {
            return Raylib.SaveFileData(pathBuffer, byteBuffer, bytes.Length);
        }
    }

    public static string[] ScanDirectory(string path, bool recursive = false)
    {
        if (path.IsEmpty)
            return [];
        using var pathBuffer = path.ToUtf8Ptr();
        var filePathList = Raylib.LoadDirectoryFilesEx(pathBuffer, null, recursive);
        var count = filePathList.Count;
        var result = new string[count];
        for (var i = 0; i < count; i++)
            result[i] = Utf8Ptr.GetString(filePathList.Paths[i]);
        Raylib.UnloadDirectoryFiles(filePathList);
        return result;
    }

    [LibraryImport("raylib")]
    private static partial long GetFileModTime(sbyte* path);
}
