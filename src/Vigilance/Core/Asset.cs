using System.Reflection;
using Vigilance.Drawing;

namespace Vigilance.Core;

public static class Asset
{
    private static readonly Container<string, Texture> TextureContainer = new();
    private static readonly Container<string, Image> ImageContainer = new();
    private static readonly Container<(string, int, string), Font> FontContainer = new();

    public static Texture TextureFile(string path)
    {
        return TextureContainer.File(ref path, () => path, bytes => new Texture(Path.GetExtension(path), bytes));
    }

    public static Texture TextureResource(string resource, string? module = null, Assembly? assembly = null)
    {
        return TextureContainer.Resource(
            ref resource,
            module,
            assembly,
            () => resource,
            bytes => new Texture(Path.GetExtension(resource), bytes)
        );
    }

    public static Image ImageFile(string path)
    {
        return ImageContainer.File(ref path, () => path, bytes => new Image(Path.GetExtension(path), bytes));
    }

    public static Image ImageResource(string resource, string? module = null, Assembly? assembly = null)
    {
        return ImageContainer.Resource(
            ref resource,
            module,
            assembly,
            () => resource,
            bytes => new Image(Path.GetExtension(resource), bytes)
        );
    }

    public static Font FontFile(string path, int? quality = null, string? charset = null)
    {
        return FontContainer.File(
            ref path,
            () => (path, quality ?? Game.DefaultFontQuality, charset ?? Game.DefaultFontCharset),
            bytes => new Font(bytes, quality, charset)
        );
    }

    public static Font FontResource(
        string resource,
        int? quality = null,
        string? charset = null,
        string? module = null,
        Assembly? assembly = null
    )
    {
        return FontContainer.Resource(
            ref resource,
            module,
            assembly,
            () => (resource, quality ?? Game.DefaultFontQuality, charset ?? Game.DefaultFontCharset),
            bytes => new Font(bytes, quality, charset)
        );
    }

    private readonly struct Container<TKey, TValue>
        where TKey : notnull
        where TValue : class
    {
        private readonly Dictionary<TKey, WeakReference<TValue>> _files = new();
        private readonly Dictionary<TKey, WeakReference<TValue>> _resources = new();

        public Container() { }

        public TValue File(ref string path, Func<TKey> getKey, Func<byte[], TValue> getValue)
        {
            var filePath = FileSystem.FormatPath(path);
            path = FileSystem.FormatPath(FileSystem.WorkingDirectory + "/" + path);
            var key = getKey.Invoke();
            if (_files.TryGetValue(key, out var reference) && reference.TryGetTarget(out var value))
                return value;
            if (!FileSystem.FileExists(filePath))
                throw new ArgumentException($"Could not find file '{path}'.");
            value = getValue.Invoke(FileSystem.ReadBytes(filePath));
            _files[key] = new WeakReference<TValue>(value);
            return value;
        }

        public TValue Resource(
            ref string resource,
            string? module,
            Assembly? assembly,
            Func<TKey> getKey,
            Func<byte[], TValue> getValue
        )
        {
            resource = FileSystem.FormatResource(resource, module ?? FileSystem.WorkingModule);
            var key = getKey.Invoke();
            if (_resources.TryGetValue(key, out var reference) && reference.TryGetTarget(out var value))
                return value;
            if (!FileSystem.ResourceExists(resource, "", assembly))
                throw new ArgumentException($"Could not find resource '{resource}'.");
            value = getValue.Invoke(FileSystem.ReadResourceBytes(resource, "", assembly));
            _resources[key] = new WeakReference<TValue>(value);
            return value;
        }
    }
}
