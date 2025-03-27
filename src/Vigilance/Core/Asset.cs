using System.Reflection;
using Vigilance.Drawing;

namespace Vigilance.Core;

public static class Asset
{
    private static readonly Dictionary<string, Texture> TextureFiles = new();
    private static readonly Dictionary<string, Texture> TextureResources = new();
    private static readonly Dictionary<(string, int, string), Font> FontFiles = new();
    private static readonly Dictionary<(string, int, string), Font> FontResources = new();

    public static Texture TextureFile(string path, bool cache = true)
    {
        path = FileSystem.FormatPath(FileSystem.WorkingDirectory + "/" + path);
        if (TextureFiles.TryGetValue(path, out var texture))
            return texture;
        if (!FileSystem.FileExists(path))
            throw new ArgumentException($"Could not find texture file '{path}'.");
        texture = new Texture(Path.GetExtension(path)[1..], FileSystem.ReadBytes(path));
        if (cache)
            TextureFiles[path] = texture;
        return texture;
    }

    public static Texture TextureResource(
        string resource,
        string? module = null,
        Assembly? assembly = null,
        bool cache = true
    )
    {
        resource = FileSystem.FormatResource(resource, module ?? FileSystem.WorkingModule);
        if (TextureResources.TryGetValue(resource, out var texture))
            return texture;
        if (!FileSystem.ResourceExists(resource, "", assembly))
            throw new ArgumentException($"Could not find texture resource '{resource}'.");
        texture = new Texture(Path.GetExtension(resource)[1..], FileSystem.ReadResourceBytes(resource, "", assembly));
        if (cache)
            TextureResources[resource] = texture;
        return texture;
    }

    public static Font FontFile(string path, int? quality = null, string? charset = null, bool cache = true)
    {
        path = FileSystem.FormatPath(FileSystem.WorkingDirectory + "/" + path);
        var args = (path, quality ?? Game.DefaultFontQuality, charset ?? Game.DefaultFontCharset);
        if (FontFiles.TryGetValue(args, out var font))
            return font;
        if (!FileSystem.FileExists(path))
            throw new ArgumentException($"Could not find font file '{path}'.");
        font = new Font(FileSystem.ReadBytes(path), quality, charset);
        if (cache)
            FontFiles[args] = font;
        return font;
    }

    public static Font FontResource(
        string resource,
        int? quality = null,
        string? charset = null,
        string? module = null,
        Assembly? assembly = null,
        bool cache = true
    )
    {
        resource = FileSystem.FormatResource(resource, module ?? FileSystem.WorkingModule);
        var args = (resource, quality ?? Game.DefaultFontQuality, charset ?? Game.DefaultFontCharset);
        if (FontResources.TryGetValue(args, out var font))
            return font;
        if (!FileSystem.ResourceExists(resource, "", assembly))
            throw new ArgumentException($"Could not find font resource '{resource}'.");
        font = new Font(FileSystem.ReadResourceBytes(resource, "", assembly), quality, charset);
        if (cache)
            FontResources[args] = font;
        return font;
    }
}
