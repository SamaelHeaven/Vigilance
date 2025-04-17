using System.Reflection;
using Vigilance.Audio;
using Vigilance.Drawing;

namespace Vigilance.Core;

public static class Asset
{
    private static readonly Container<string, Texture> TextureContainer = new();
    private static readonly Container<string, Image> ImageContainer = new();
    private static readonly Container<(string Key, int Quality, string Charset), Font> FontContainer = new();
    private static readonly Container<string, Music> MusicContainer = new();
    private static readonly Container<(string Key, int MaxAliases), Sound> SoundContainer = new();

    public static Texture TextureFile(string path, CacheType? cacheType = null)
    {
        return TextureContainer.File(
            ref path,
            () => path,
            bytes => new Texture(Path.GetExtension(path), bytes),
            cacheType
        );
    }

    public static Texture TextureResource(
        string resource,
        string? @namespace = null,
        Assembly? assembly = null,
        CacheType? cacheType = null
    )
    {
        return TextureContainer.Resource(
            ref resource,
            @namespace,
            assembly,
            () => resource,
            bytes => new Texture(Path.GetExtension(resource), bytes),
            cacheType
        );
    }

    public static void InvalidateTexture(Texture texture)
    {
        TextureContainer.Invalidate(texture);
    }

    public static Image ImageFile(string path, CacheType? cacheType = null)
    {
        return ImageContainer.File(ref path, () => path, bytes => new Image(Path.GetExtension(path), bytes), cacheType);
    }

    public static Image ImageResource(
        string resource,
        string? @namespace = null,
        Assembly? assembly = null,
        CacheType? cacheType = null
    )
    {
        return ImageContainer.Resource(
            ref resource,
            @namespace,
            assembly,
            () => resource,
            bytes => new Image(Path.GetExtension(resource), bytes),
            cacheType
        );
    }

    public static void InvalidateImage(Image image)
    {
        ImageContainer.Invalidate(image);
    }

    public static Font FontFile(string path, int? quality = null, string? charset = null, CacheType? cacheType = null)
    {
        return FontContainer.File(
            ref path,
            () => (path, quality ?? Game.DefaultFontQuality, charset ?? Game.DefaultFontCharset),
            bytes => new Font(bytes, quality ?? Game.DefaultFontQuality, charset ?? Game.DefaultFontCharset),
            cacheType
        );
    }

    public static Font FontResource(
        string resource,
        int? quality = null,
        string? charset = null,
        string? @namespace = null,
        Assembly? assembly = null,
        CacheType? cacheType = null
    )
    {
        return FontContainer.Resource(
            ref resource,
            @namespace,
            assembly,
            () => (resource, quality ?? Game.DefaultFontQuality, charset ?? Game.DefaultFontCharset),
            bytes => new Font(bytes, quality ?? Game.DefaultFontQuality, charset ?? Game.DefaultFontCharset),
            cacheType
        );
    }

    public static void InvalidateFont(Font font)
    {
        FontContainer.Invalidate(font);
    }

    public static Music MusicFile(string path, CacheType? cacheType = null)
    {
        return MusicContainer.File(ref path, () => path, bytes => new Music(Path.GetExtension(path), bytes), cacheType);
    }

    public static Music MusicResource(
        string resource,
        string? @namespace = null,
        Assembly? assembly = null,
        CacheType? cacheType = null
    )
    {
        return MusicContainer.Resource(
            ref resource,
            @namespace,
            assembly,
            () => resource,
            bytes => new Music(Path.GetExtension(resource), bytes),
            cacheType
        );
    }

    public static void InvalidateMusic(Music music)
    {
        MusicContainer.Invalidate(music);
    }

    public static Sound SoundFile(string path, int? maxAliases = null, CacheType? cacheType = null)
    {
        return SoundContainer.File(
            ref path,
            () => (path, maxAliases ?? Game.DefaultSoundMaxAliases),
            bytes => new Sound(Path.GetExtension(path), bytes, maxAliases ?? Game.DefaultSoundMaxAliases),
            cacheType
        );
    }

    public static Sound SoundResource(
        string resource,
        int? maxAliases = null,
        string? @namespace = null,
        Assembly? assembly = null,
        CacheType? cacheType = null
    )
    {
        return SoundContainer.Resource(
            ref resource,
            @namespace,
            assembly,
            () => (resource, maxAliases ?? Game.DefaultSoundMaxAliases),
            bytes => new Sound(Path.GetExtension(resource), bytes, maxAliases ?? Game.DefaultSoundMaxAliases),
            cacheType
        );
    }

    public static void InvalidateSound(Sound sound)
    {
        SoundContainer.Invalidate(sound);
    }

    private readonly struct Container<TKey, TValue>
        where TKey : notnull
        where TValue : class
    {
        private readonly Dictionary<TKey, WeakReference<TValue>> _weakFiles = new();
        private readonly Dictionary<TKey, WeakReference<TValue>> _weakResources = new();
        private readonly Dictionary<TKey, TValue> _strongFiles = new();
        private readonly Dictionary<TKey, TValue> _strongResources = new();

        public Container() { }

        public TValue File(
            ref string path,
            Func<TKey> keyFunc,
            Func<byte[], TValue> valueFunc,
            CacheType? cacheType = null
        )
        {
            var filePath = FileSystem.FormatPath(path);
            path = FileSystem.FormatPath(FileSystem.WorkingDirectory + "/" + path);
            var key = keyFunc.Invoke();
            var fCacheType = cacheType ?? Game.DefaultAssetCacheType;
            var weak = fCacheType == CacheType.Weak;
            var strong = fCacheType == CacheType.Strong;
            TValue? value;
            if (weak || strong)
            {
                if (_weakFiles.TryGetValue(key, out var reference))
                    if (reference.TryGetTarget(out value))
                        return value;
                if (_strongFiles.TryGetValue(key, out value))
                    return value;
            }

            if (!FileSystem.FileExists(filePath))
                throw new ArgumentException($"Could not find file '{path}'.");
            value = valueFunc.Invoke(FileSystem.ReadBytes(filePath));
            if (weak)
                _weakFiles[key] = new WeakReference<TValue>(value);
            if (strong)
                _strongFiles[key] = value;
            return value;
        }

        public TValue Resource(
            ref string resource,
            string? @namespace,
            Assembly? assembly,
            Func<TKey> keyFunc,
            Func<byte[], TValue> valueFunc,
            CacheType? cacheType = null
        )
        {
            resource = FileSystem.FormatResource(resource, @namespace ?? FileSystem.WorkingNamespace);
            var key = keyFunc.Invoke();
            var rCacheType = cacheType ?? Game.DefaultAssetCacheType;
            var weak = rCacheType == CacheType.Weak;
            var strong = rCacheType == CacheType.Strong;
            TValue? value;
            if (weak || strong)
            {
                if (_weakResources.TryGetValue(key, out var reference))
                    if (reference.TryGetTarget(out value))
                        return value;
                if (_strongResources.TryGetValue(key, out value))
                    return value;
            }

            if (!FileSystem.ResourceExists(resource, "", assembly))
                throw new ArgumentException($"Could not find resource '{resource}'.");
            value = valueFunc.Invoke(FileSystem.ReadResourceBytes(resource, "", assembly));
            if (weak)
                _weakResources[key] = new WeakReference<TValue>(value);
            if (strong)
                _strongResources[key] = value;
            return value;
        }

        public void Invalidate(TValue value)
        {
            var weakFilesKeys = _weakFiles
                .Where(kvp =>
                    kvp.Value.TryGetTarget(out var target) && EqualityComparer<TValue>.Default.Equals(target, value)
                )
                .Select(kvp => kvp.Key)
                .ToList();
            foreach (var key in weakFilesKeys)
                _weakFiles.Remove(key);
            var weakResourcesKeys = _weakResources
                .Where(kvp =>
                    kvp.Value.TryGetTarget(out var target) && EqualityComparer<TValue>.Default.Equals(target, value)
                )
                .Select(kvp => kvp.Key)
                .ToList();
            foreach (var key in weakResourcesKeys)
                _weakResources.Remove(key);
            var strongFilesKeys = _strongFiles
                .Where(kvp => EqualityComparer<TValue>.Default.Equals(kvp.Value, value))
                .Select(kvp => kvp.Key)
                .ToList();
            foreach (var key in strongFilesKeys)
                _strongFiles.Remove(key);
            var strongResourcesKeys = _strongResources
                .Where(kvp => EqualityComparer<TValue>.Default.Equals(kvp.Value, value))
                .Select(kvp => kvp.Key)
                .ToList();
            foreach (var key in strongResourcesKeys)
                _strongResources.Remove(key);
        }
    }
}
