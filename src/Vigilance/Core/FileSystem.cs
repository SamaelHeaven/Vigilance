using System.Buffers;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Raylib_cs.BleedingEdge;
using Vigilance.Logging;

namespace Vigilance.Core;

public static unsafe partial class FileSystem
{
    private static readonly Dictionary<Assembly, string[]> _resourceNames = new();

    static FileSystem()
    {
        WorkingNamespace = new FileSystemConfig().WorkingNamespace;
    }

    public static string ApplicationDirectory { get; } =
        FormatPath(Utf8Buffer.GetString(Raylib.GetApplicationDirectory()));

    public static string WorkingNamespace { get; set; }

    public static string WorkingDirectory => FormatPath(Utf8Buffer.GetString(Raylib.GetWorkingDirectory()));

    public static string[] DroppedFiles => !Raylib.IsFileDropped() ? Array.Empty<string>() : Raylib.GetDroppedFiles();

    internal static void Initialize()
    {
        var config = Game.Config.Take<FileSystemConfig>() ?? new FileSystemConfig();
        WorkingNamespace = config.WorkingNamespace;
        ChangeDirectory(config.WorkingDirectory);
    }

    public static string FormatPath(string path)
    {
        if (string.IsNullOrEmpty(path))
            return "";
        var span = path.AsSpan();
        var start = 0;
        var end = span.Length - 1;
        while (start <= end && char.IsWhiteSpace(span[start]))
            start++;
        while (end >= start && char.IsWhiteSpace(span[end]))
            end--;
        if (start > end)
            return "";
        var length = end - start + 1;
        var useStack = length <= 256;
        char[] bufferArray = null!;
        var buffer = useStack ? stackalloc char[length] : bufferArray = ArrayPool<char>.Shared.Rent(length);
        var count = 0;
        var lastWasSlash = false;
        for (var i = start; i <= end; i++)
        {
            var c = span[i];
            if (c == '\\')
                c = '/';
            if (c == '/')
            {
                if (lastWasSlash)
                    continue;
                lastWasSlash = true;
            }
            else
            {
                lastWasSlash = false;
            }

            buffer[count++] = c;
        }

        var trimStart = 0;
        var trimEnd = count - 1;
        while (trimStart <= trimEnd && buffer[trimStart] == '/')
            trimStart++;
        while (trimEnd >= trimStart && buffer[trimEnd] == '/')
            trimEnd--;
        var finalLength = trimEnd - trimStart + 1;
        if (finalLength <= 0)
        {
            if (!useStack)
                ArrayPool<char>.Shared.Return(bufferArray);
            return "";
        }

        var result = new string(buffer.Slice(trimStart, finalLength));
        if (!useStack)
            ArrayPool<char>.Shared.Return(bufferArray);
        return result;
    }

    public static string NormalizePath(string path)
    {
        return FormatPath(Path.Combine(WorkingDirectory, path));
    }

    public static string FormatResource(string resource, string? @namespace = null)
    {
        @namespace ??= WorkingNamespace;
        return @namespace == "" ? resource : $"{@namespace}.{resource}";
    }

    public static string FormatResource(string resource, string? @namespace, Assembly? assembly)
    {
        @namespace ??= WorkingNamespace;
        assembly ??= Assemblies.Game;
        return $"{assembly.GetName().Name}.{@namespace}{(@namespace == "" ? "" : ".")}{resource}";
    }

    public static bool ChangeDirectory(string path)
    {
        path = FormatPath(path);
        using var buffer = path.ToUtf8Buffer();
        if (!Raylib.DirectoryExists(buffer))
            return false;
        return Raylib.ChangeDirectory(buffer);
    }

    public static bool FileExists(string path)
    {
        path = FormatPath(path);
        if (path == "")
            return false;
        using var buffer = path.ToUtf8Buffer();
        return Raylib.FileExists(buffer);
    }

    public static bool DirectoryExists(string path)
    {
        path = FormatPath(path);
        if (path == "")
            return false;
        using var buffer = path.ToUtf8Buffer();
        return Raylib.DirectoryExists(buffer);
    }

    public static bool ResourceExists(string resource, string? @namespace = null, Assembly? assembly = null)
    {
        assembly ??= Assemblies.Game;
        resource = FormatResource(resource, @namespace, assembly);
        if (_resourceNames.TryGetValue(assembly, out var names))
            return names.Contains(resource);
        names = assembly.GetManifestResourceNames();
        _resourceNames[assembly] = names;
        return names.Contains(resource);
    }

    public static DateTime FileModTime(string path)
    {
        path = FormatPath(path);
        using var buffer = path.ToUtf8Buffer();
        return !Raylib.FileExists(buffer)
            ? DateTime.MinValue
            : DateTimeOffset.FromUnixTimeSeconds(GetFileModTime(buffer)).UtcDateTime;
    }

    public static int GetFileSize(string path)
    {
        path = FormatPath(path);
        using var buffer = path.ToUtf8Buffer();
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
            Logger.Warning($"FILEIO: [{FormatPath(path)}] Failed to read text file");
        return text;
    }

    public static bool TryReadResourceText(
        string resource,
        out string text,
        string? @namespace = null,
        Assembly? assembly = null
    )
    {
        var result = TryReadResourceBytes(resource, out var bytes, @namespace, assembly);
        text = Encoding.UTF8.GetString(bytes);
        return result;
    }

    public static string ReadResourceText(string resource, string? @namespace = null, Assembly? assembly = null)
    {
        if (!TryReadResourceText(resource, out var text, @namespace, assembly))
            Logger.Warning($"FILEIO: [{FormatResource(resource, @namespace)}] Failed to read resource text");
        return text;
    }

    public static bool WriteText(string path, string text)
    {
        path = FormatPath(path);
        using var pathBuffer = path.ToUtf8Buffer();
        using var textBuffer = text.ToUtf8Buffer();
        return Raylib.SaveFileText(pathBuffer, textBuffer);
    }

    public static bool TryReadBytes(string path, out byte[] bytes)
    {
        path = FormatPath(path);
        using var pathBuffer = path.ToUtf8Buffer();
        if (!Raylib.FileExists(pathBuffer))
        {
            bytes = Array.Empty<byte>();
            return false;
        }

        int bytesRead;
        var data = Raylib.LoadFileData(pathBuffer, &bytesRead);
        if (data == null)
        {
            bytes = Array.Empty<byte>();
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
            Logger.Warning($"FILEIO: [{FormatPath(path)}] Failed to read file");
        return bytes;
    }

    public static bool TryReadResourceBytes(
        string resource,
        out byte[] bytes,
        string? @namespace = null,
        Assembly? assembly = null
    )
    {
        assembly ??= Assemblies.Game;
        resource = FormatResource(resource, @namespace, assembly);
        using var stream = assembly.GetManifestResourceStream(resource);
        if (stream is null)
        {
            bytes = Array.Empty<byte>();
            return false;
        }

        if (stream.CanSeek)
        {
            var length = (int)stream.Length;
            bytes = new byte[length];
            stream.ReadExactly(bytes);
            return true;
        }

        using var ms = new MemoryStream();
        stream.CopyTo(ms);
        bytes = ms.ToArray();
        return true;
    }

    public static byte[] ReadResourceBytes(string resource, string? @namespace = null, Assembly? assembly = null)
    {
        if (!TryReadResourceBytes(resource, out var bytes, @namespace, assembly))
            Logger.Warning($"FILEIO: [{FormatResource(resource, @namespace)}] Failed to read resource");
        return bytes;
    }

    public static bool WriteBytes(string path, IEnumerable<byte> bytes)
    {
        return WriteBytesSpan(path, bytes.AsSpan());
    }

    public static bool WriteBytesSpan(string path, ReadOnlySpan<byte> bytes)
    {
        path = FormatPath(path);
        using var pathBuffer = path.ToUtf8Buffer();
        fixed (byte* byteBuffer = bytes)
        {
            return Raylib.SaveFileData(pathBuffer, byteBuffer, bytes.Length);
        }
    }

    public static string[] ScanDirectory(string path, bool recursive = false)
    {
        path = FormatPath(path);
        using var pathBuffer = path.ToUtf8Buffer();
        var filePathList = Raylib.LoadDirectoryFilesEx(pathBuffer, null, recursive);
        var count = filePathList.Count;
        var result = new string[count];
        for (var i = 0; i < count; i++)
            result[i] = FormatPath(Utf8Buffer.GetString(filePathList.Paths[i]));
        Raylib.UnloadDirectoryFiles(filePathList);
        return result;
    }

    [LibraryImport("raylib")]
    private static partial long GetFileModTime(sbyte* path);
}
