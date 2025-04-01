using System.Reflection;
using Vigilance.Drawing;

namespace Vigilance.Core;

public static class Asset
{
    private static readonly Container<string, Texture> TextureContainer = new();
    private static readonly Container<string, Image> ImageContainer = new();
    private static readonly Container<(string, int, string), Font> FontContainer = new();

    public static Texture TextureFile(string path, bool cache = true)
    {
        return TextureContainer.File(
            ref path,
            cache,
            () => path,
            bytes => new Texture(Path.GetExtension(path), bytes)
        );
    }

    public static Texture TextureResource(
        string resource,
        string? module = null,
        Assembly? assembly = null,
        bool cache = true
    )
    {
        return TextureContainer.Resource(
            ref resource,
            module,
            assembly,
            cache,
            () => resource,
            bytes => new Texture(Path.GetExtension(resource), bytes)
        );
    }

    public static Image ImageFile(string path, bool cache = true)
    {
        return ImageContainer.File(
            ref path,
            cache,
            () => path,
            bytes => new Image(Path.GetExtension(path), bytes)
        );
    }

    public static Image ImageResource(
        string resource,
        string? module = null,
        Assembly? assembly = null,
        bool cache = true
    )
    {
        return ImageContainer.Resource(
            ref resource,
            module,
            assembly,
            cache,
            () => resource,
            bytes => new Image(Path.GetExtension(resource), bytes)
        );
    }

    public static Font FontFile(string path, int? quality = null, string? charset = null, bool cache = true)
    {
        return FontContainer.File(
            ref path,
            cache,
            () => (path, quality ?? Game.DefaultFontQuality, charset ?? Game.DefaultFontCharset),
            bytes => new Font(bytes, quality, charset)
        );
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
        return FontContainer.Resource(
            ref resource,
            module,
            assembly,
            cache,
            () => (resource, quality ?? Game.DefaultFontQuality, charset ?? Game.DefaultFontCharset),
            bytes => new Font(bytes, quality, charset)
        );
    }

    private readonly struct Container<TKey, TValue>
        where TKey : notnull
    {
        private readonly Dictionary<TKey, TValue> _files = new();
        private readonly Dictionary<TKey, TValue> _resources = new();

        public Container() { }

        public TValue File(ref string path, bool cache, Func<TKey> getKey, Func<byte[], TValue> getValue)
        {
            var filePath = FileSystem.FormatPath(path);
            path = FileSystem.FormatPath(FileSystem.WorkingDirectory + "/" + path);
            var key = getKey.Invoke();
            if (_files.TryGetValue(key, out var value))
                return value;
            if (!FileSystem.FileExists(filePath))
                throw new ArgumentException($"Could not find file '{path}'.");
            value = getValue.Invoke(FileSystem.ReadBytes(filePath));
            if (cache)
                _files[key] = value;
            return value;
        }

        public TValue Resource(
            ref string resource,
            string? module,
            Assembly? assembly,
            bool cache,
            Func<TKey> getKey,
            Func<byte[], TValue> getValue
        )
        {
            resource = FileSystem.FormatResource(resource, module ?? FileSystem.WorkingModule);
            var key = getKey.Invoke();
            if (_resources.TryGetValue(key, out var value))
                return value;
            if (!FileSystem.ResourceExists(resource, "", assembly))
                throw new ArgumentException($"Could not find resource '{resource}'.");
            value = getValue.Invoke(FileSystem.ReadResourceBytes(resource, "", assembly));
            if (cache)
                _resources[key] = value;
            return value;
        }
    }
}
