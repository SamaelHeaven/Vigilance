using System.Reflection;
using Vigilance.Audio;
using Vigilance.Drawing;
using ZLinq;

namespace Vigilance.Core;

public static class Asset
{
    private static readonly Container<string, Texture> _textureContainer = new();
    private static readonly Container<string, Image> _imageContainer = new();
    private static readonly Container<(string Key, int Quality, string Charset), Font> _fontContainer = new();
    private static readonly Container<string, Music> _musicContainer = new();
    private static readonly Container<(string Key, int MaxAliases), Sound> _soundContainer = new();
    private static AssetConfig _config = new();

    public static CacheType DefaultCacheType
    {
        get => _config.DefaultCacheType;
        set => _config.DefaultCacheType = value;
    }

    internal static void Initialize()
    {
        if (Game.Config.TryTake(out AssetConfig config))
            _config = config;
    }

    public static Texture TextureFile(string path, CacheType? cacheType = null)
    {
        return _textureContainer.File(
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
        return _textureContainer.Resource(
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
        _textureContainer.Invalidate(texture);
    }

    public static Image ImageFile(string path, CacheType? cacheType = null)
    {
        return _imageContainer.File(
            ref path,
            () => path,
            bytes => new Image(Path.GetExtension(path), bytes),
            cacheType
        );
    }

    public static Image ImageResource(
        string resource,
        string? @namespace = null,
        Assembly? assembly = null,
        CacheType? cacheType = null
    )
    {
        return _imageContainer.Resource(
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
        _imageContainer.Invalidate(image);
    }

    public static Font FontFile(string path, int? quality = null, string? charset = null, CacheType? cacheType = null)
    {
        return _fontContainer.File(
            ref path,
            () => (path, quality ?? Font.DefaultQuality, charset ?? Font.DefaultCharset),
            bytes => new Font(bytes, quality ?? Font.DefaultQuality, charset ?? Font.DefaultCharset),
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
        return _fontContainer.Resource(
            ref resource,
            @namespace,
            assembly,
            () => (resource, quality ?? Font.DefaultQuality, charset ?? Font.DefaultCharset),
            bytes => new Font(bytes, quality ?? Font.DefaultQuality, charset ?? Font.DefaultCharset),
            cacheType
        );
    }

    public static void InvalidateFont(Font font)
    {
        _fontContainer.Invalidate(font);
    }

    public static Music MusicFile(string path, CacheType? cacheType = null)
    {
        return _musicContainer.File(
            ref path,
            () => path,
            bytes => new Music(Path.GetExtension(path), bytes),
            cacheType
        );
    }

    public static Music MusicResource(
        string resource,
        string? @namespace = null,
        Assembly? assembly = null,
        CacheType? cacheType = null
    )
    {
        return _musicContainer.Resource(
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
        _musicContainer.Invalidate(music);
    }

    public static Sound SoundFile(string path, int? maxAliases = null, CacheType? cacheType = null)
    {
        return _soundContainer.File(
            ref path,
            () => (path, maxAliases ?? Audio.Audio.DefaultSoundMaxAliases),
            bytes => new Sound(Path.GetExtension(path), bytes, maxAliases ?? Audio.Audio.DefaultSoundMaxAliases),
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
        return _soundContainer.Resource(
            ref resource,
            @namespace,
            assembly,
            () => (resource, maxAliases ?? Audio.Audio.DefaultSoundMaxAliases),
            bytes => new Sound(Path.GetExtension(resource), bytes, maxAliases ?? Audio.Audio.DefaultSoundMaxAliases),
            cacheType
        );
    }

    public static void InvalidateSound(Sound sound)
    {
        _soundContainer.Invalidate(sound);
    }

    private sealed class Container<TKey, TValue>
        where TKey : notnull
        where TValue : class
    {
        private readonly Dictionary<TKey, TValue> _strongFiles = new();
        private readonly Dictionary<TKey, TValue> _strongResources = new();
        private readonly Dictionary<TKey, WeakReference<TValue>> _weakFiles = new();
        private readonly Dictionary<TKey, WeakReference<TValue>> _weakResources = new();

        public TValue File(
            ref string path,
            Func<TKey> keyFunc,
            Func<byte[], TValue> valueFunc,
            CacheType? cacheType = null
        )
        {
            var filePath = FileSystem.FormatPath(path);
            path = FileSystem.FormatPath(Path.Combine(FileSystem.WorkingDirectory, path));
            var key = keyFunc.Invoke();
            var fCacheType = cacheType ?? DefaultCacheType;
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
            var rCacheType = cacheType ?? DefaultCacheType;
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
                .AsValueEnumerable()
                .Where(kvp =>
                    kvp.Value.TryGetTarget(out var target) && EqualityComparer<TValue>.Default.Equals(target, value)
                )
                .Select(kvp => kvp.Key)
                .ToList();
            foreach (var key in weakFilesKeys)
                _weakFiles.Remove(key);
            var weakResourcesKeys = _weakResources
                .AsValueEnumerable()
                .Where(kvp =>
                    kvp.Value.TryGetTarget(out var target) && EqualityComparer<TValue>.Default.Equals(target, value)
                )
                .Select(kvp => kvp.Key)
                .ToList();
            foreach (var key in weakResourcesKeys)
                _weakResources.Remove(key);
            var strongFilesKeys = _strongFiles
                .AsValueEnumerable()
                .Where(kvp => EqualityComparer<TValue>.Default.Equals(kvp.Value, value))
                .Select(kvp => kvp.Key)
                .ToList();
            foreach (var key in strongFilesKeys)
                _strongFiles.Remove(key);
            var strongResourcesKeys = _strongResources
                .AsValueEnumerable()
                .Where(kvp => EqualityComparer<TValue>.Default.Equals(kvp.Value, value))
                .Select(kvp => kvp.Key)
                .ToList();
            foreach (var key in strongResourcesKeys)
                _strongResources.Remove(key);
        }
    }
}
