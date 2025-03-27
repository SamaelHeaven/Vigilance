using Vigilance.Drawing;

namespace Vigilance.Core;

public static class Asset
{
    private static readonly Dictionary<string, Texture> TextureFiles = new();
    private static readonly Dictionary<string, Texture> TextureResources = new();
    private static readonly Dictionary<(string, int, string), Font> FontFiles = new();
    private static readonly Dictionary<(string, int, string), Font> FontResources = new();

    public static Texture TextureFile(string path)
    {
        path = FileSystem.FormatPath(FileSystem.WorkingDirectory + "/" + path);
        if (TextureFiles.TryGetValue(path, out var texture))
            return texture;
        if (!FileSystem.FileExists(path))
            throw new ArgumentException($"Could not find texture file '{path}'.");
        texture = new Texture(Path.GetExtension(path)[1..], FileSystem.ReadBytes(path));
        TextureFiles[path] = texture;
        return texture;
    }

    public static Texture TextureResource(string resource)
    {
        resource = FileSystem.WorkingNamespace == "" ? resource : FileSystem.WorkingNamespace + "." + resource;
        if (TextureResources.TryGetValue(resource, out var texture))
            return texture;
        if (!FileSystem.ResourceExists(resource, ""))
            throw new ArgumentException($"Could not find texture resource '{resource}'.");
        texture = new Texture(Path.GetExtension(resource)[1..], FileSystem.ReadResourceBytes(resource, ""));
        TextureResources[resource] = texture;
        return texture;
    }

    public static Font FontFile(string path, int quality = Font.DefaultQuality, string charset = Font.DefaultCharset)
    {
        path = FileSystem.FormatPath(FileSystem.WorkingDirectory + "/" + path);
        if (FontFiles.TryGetValue((path, quality, charset), out var font))
            return font;
        if (!FileSystem.FileExists(path))
            throw new ArgumentException($"Could not find font file '{path}'.");
        font = new Font(FileSystem.ReadBytes(path), quality, charset);
        FontFiles[(path, quality, charset)] = font;
        return font;
    }

    public static Font FontResource(
        string resource,
        int quality = Font.DefaultQuality,
        string charset = Font.DefaultCharset
    )
    {
        resource = FileSystem.WorkingNamespace == "" ? resource : FileSystem.WorkingNamespace + "." + resource;
        if (FontResources.TryGetValue((resource, quality, charset), out var font))
            return font;
        if (!FileSystem.ResourceExists(resource, ""))
            throw new ArgumentException($"Could not find font resource '{resource}'.");
        font = new Font(FileSystem.ReadResourceBytes(resource, ""), quality, charset);
        FontResources[(resource, quality, charset)] = font;
        return font;
    }
}
