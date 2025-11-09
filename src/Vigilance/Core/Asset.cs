using System.Reflection;
using System.Text;
using Vigilance.Audio;
using Vigilance.Drawing;
using ZLinq;

namespace Vigilance.Core;

public static class Asset
{
    private static AssetConfig _config = new();

    public static CacheType DefaultCacheType
    {
        get => _config.DefaultCacheType;
        set => _config.DefaultCacheType = value;
    }

    internal static void Initialize()
    {
        _config = Game.Config.Take<AssetConfig>() ?? _config;
    }

    public static class Helper
    {
        public static string NormalizePath(string path)
        {
            return FileSystem.FormatPath(Path.Combine(FileSystem.WorkingDirectory, path));
        }

        public static byte[] ReadFile(string path, string normalizedPath)
        {
            return !FileSystem.FileExists(path)
                ? throw new ArgumentException($"Could not find file '{normalizedPath}'.")
                : FileSystem.ReadBytes(path);
        }

        public static string NormalizeResource(string resource, string? @namespace)
        {
            return FileSystem.FormatResource(resource, @namespace ?? FileSystem.WorkingNamespace);
        }

        public static byte[] ReadResource(string resource, Assembly? assembly)
        {
            return !FileSystem.ResourceExists(resource, "", assembly)
                ? throw new ArgumentException($"Could not find resource '{resource}'.")
                : FileSystem.ReadResourceBytes(resource, "", assembly);
        }
    }

    public sealed class Container<TKey, TValue>
        where TKey : notnull
        where TValue : class
    {
        private readonly Dictionary<TKey, TValue> _strongFiles = new();
        private readonly Dictionary<TKey, TValue> _strongResources = new();
        private readonly Dictionary<TKey, TValue> _strongValues = new();
        private readonly Dictionary<TKey, WeakReference<TValue>> _weakFiles = new();
        private readonly Dictionary<TKey, WeakReference<TValue>> _weakResources = new();
        private readonly Dictionary<TKey, WeakReference<TValue>> _weakValues = new();

        public TValue File(
            ref string path,
            Func<TKey> keyFunc,
            Func<byte[], TValue> valueFunc,
            CacheType? cacheType = null
        )
        {
            var filePath = FileSystem.FormatPath(path);
            var normalizedPath = Helper.NormalizePath(path);
            path = normalizedPath;
            return File(keyFunc, () => valueFunc.Invoke(Helper.ReadFile(filePath, normalizedPath)), cacheType);
        }

        public TValue File(Func<TKey> keyFunc, Func<TValue> valueFunc, CacheType? cacheType = null)
        {
            return Get(_weakFiles, _strongFiles, keyFunc, valueFunc, cacheType);
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
            resource = Helper.NormalizeResource(resource, @namespace);
            var resourceValue = resource;
            resource = FileSystem.FormatResource(resource, assembly?.FullName ?? "");
            return Resource(keyFunc, () => valueFunc.Invoke(Helper.ReadResource(resourceValue, assembly)), cacheType);
        }

        public TValue Resource(Func<TKey> keyFunc, Func<TValue> valueFunc, CacheType? cacheType = null)
        {
            return Get(_weakResources, _strongResources, keyFunc, valueFunc, cacheType);
        }

        public TValue Raw(TKey key, Func<TValue> valueFunc, CacheType? cacheType = null)
        {
            return Get(_weakValues, _strongValues, () => key, valueFunc, cacheType);
        }

        public void Invalidate(TValue value)
        {
            Invalidate(value, _weakFiles);
            Invalidate(value, _weakResources);
            Invalidate(value, _weakValues);
            Invalidate(value, _strongFiles);
            Invalidate(value, _strongResources);
            Invalidate(value, _strongValues);
        }

        private static TValue Get(
            Dictionary<TKey, WeakReference<TValue>> weakValues,
            Dictionary<TKey, TValue> strongValues,
            Func<TKey> keyFunc,
            Func<TValue> valueFunc,
            CacheType? cacheType = null
        )
        {
            var key = keyFunc.Invoke();
            var rCacheType = cacheType ?? DefaultCacheType;
            var weak = rCacheType == CacheType.Weak;
            var strong = rCacheType == CacheType.Strong;
            TValue? value;
            if (weak || strong)
            {
                if (weakValues.TryGetValue(key, out var reference))
                    if (reference.TryGetTarget(out value))
                        return value;
                if (strongValues.TryGetValue(key, out value))
                    return value;
            }

            value = valueFunc.Invoke();
            if (weak)
                weakValues[key] = new WeakReference<TValue>(value);
            if (strong)
                strongValues[key] = value;
            return value;
        }

        private static void Invalidate(TValue value, Dictionary<TKey, WeakReference<TValue>> values)
        {
            var keys = values
                .AsValueEnumerable()
                .Where(kvp =>
                    kvp.Value.TryGetTarget(out var target) && EqualityComparer<TValue>.Default.Equals(target, value)
                )
                .Select(kvp => kvp.Key);
            foreach (var key in keys)
                values.Remove(key);
        }

        private static void Invalidate(TValue value, Dictionary<TKey, TValue> values)
        {
            var keys = values
                .AsValueEnumerable()
                .Where(kvp => EqualityComparer<TValue>.Default.Equals(kvp.Value, value))
                .Select(kvp => kvp.Key);
            foreach (var key in keys)
                values.Remove(key);
        }
    }
}

public static class TextureAssetExtensions
{
    private static readonly Asset.Container<string, Texture> _container = new();

    extension(Texture)
    {
        public static Texture File(string path, CacheType? cacheType = null)
        {
            return _container.File(
                ref path,
                () => path,
                bytes => new Texture(Path.GetExtension(path), bytes),
                cacheType
            );
        }

        public static Texture Resource(
            string resource,
            string? @namespace = null,
            Assembly? assembly = null,
            CacheType? cacheType = null
        )
        {
            return _container.Resource(
                ref resource,
                @namespace,
                assembly,
                () => resource,
                bytes => new Texture(Path.GetExtension(resource), bytes),
                cacheType
            );
        }

        public static void Invalidate(Texture texture)
        {
            _container.Invalidate(texture);
        }
    }
}

public static class ImageAssetExtensions
{
    private static readonly Asset.Container<string, Image> _container = new();

    extension(Image)
    {
        public static Image File(string path, CacheType? cacheType = null)
        {
            return _container.File(ref path, () => path, bytes => new Image(Path.GetExtension(path), bytes), cacheType);
        }

        public static Image Resource(
            string resource,
            string? @namespace = null,
            Assembly? assembly = null,
            CacheType? cacheType = null
        )
        {
            return _container.Resource(
                ref resource,
                @namespace,
                assembly,
                () => resource,
                bytes => new Image(Path.GetExtension(resource), bytes),
                cacheType
            );
        }

        public static void Invalidate(Image image)
        {
            _container.Invalidate(image);
        }
    }
}

public static class FontAssetExtensions
{
    private static readonly Asset.Container<(string Key, int Quality, string Charset), Font> _container = new();

    extension(Font)
    {
        public static Font File(string path, int? quality = null, string? charset = null, CacheType? cacheType = null)
        {
            return _container.File(
                ref path,
                () => (path, quality ?? Font.DefaultQuality, charset ?? Font.DefaultCharset),
                bytes => new Font(bytes, quality ?? Font.DefaultQuality, charset ?? Font.DefaultCharset),
                cacheType
            );
        }

        public static Font Resource(
            string resource,
            int? quality = null,
            string? charset = null,
            string? @namespace = null,
            Assembly? assembly = null,
            CacheType? cacheType = null
        )
        {
            return _container.Resource(
                ref resource,
                @namespace,
                assembly,
                () => (resource, quality ?? Font.DefaultQuality, charset ?? Font.DefaultCharset),
                bytes => new Font(bytes, quality ?? Font.DefaultQuality, charset ?? Font.DefaultCharset),
                cacheType
            );
        }

        public static void Invalidate(Font font)
        {
            _container.Invalidate(font);
        }
    }
}

public static class MusicAssetExtensions
{
    private static readonly Asset.Container<string, Music> _container = new();

    extension(Music)
    {
        public static Music File(string path, CacheType? cacheType = null)
        {
            return _container.File(ref path, () => path, bytes => new Music(Path.GetExtension(path), bytes), cacheType);
        }

        public static Music Resource(
            string resource,
            string? @namespace = null,
            Assembly? assembly = null,
            CacheType? cacheType = null
        )
        {
            return _container.Resource(
                ref resource,
                @namespace,
                assembly,
                () => resource,
                bytes => new Music(Path.GetExtension(resource), bytes),
                cacheType
            );
        }

        public static void Invalidate(Music music)
        {
            _container.Invalidate(music);
        }
    }
}

public static class SoundAssetExtensions
{
    private static readonly Asset.Container<(string Key, int MaxAliases), Sound> _container = new();

    extension(Sound)
    {
        public static Sound File(string path, int? maxAliases = null, CacheType? cacheType = null)
        {
            return _container.File(
                ref path,
                () => (path, maxAliases ?? Audio.Audio.DefaultSoundMaxAliases),
                bytes => new Sound(Path.GetExtension(path), bytes, maxAliases ?? Audio.Audio.DefaultSoundMaxAliases),
                cacheType
            );
        }

        public static Sound Resource(
            string resource,
            int? maxAliases = null,
            string? @namespace = null,
            Assembly? assembly = null,
            CacheType? cacheType = null
        )
        {
            return _container.Resource(
                ref resource,
                @namespace,
                assembly,
                () => (resource, maxAliases ?? Audio.Audio.DefaultSoundMaxAliases),
                bytes => new Sound(
                    Path.GetExtension(resource),
                    bytes,
                    maxAliases ?? Audio.Audio.DefaultSoundMaxAliases
                ),
                cacheType
            );
        }

        public static void Invalidate(Sound sound)
        {
            _container.Invalidate(sound);
        }
    }
}

public static class ShaderAssetExtensions
{
    private static readonly Asset.Container<(string? VertexKey, string? FragmentKey), Shader> _container = new();

    extension(Shader)
    {
        public static Shader File(string? vertexPath, string? fragmentPath, CacheType? cacheType = null)
        {
            var normalizedVertexPath = vertexPath is null ? null : Asset.Helper.NormalizePath(vertexPath);
            var normalizedFragmentPath = fragmentPath is null ? null : Asset.Helper.NormalizePath(fragmentPath);
            return _container.File(
                () => (normalizedVertexPath, normalizedFragmentPath),
                () =>
                {
                    var vertex = vertexPath is null
                        ? null
                        : Encoding.UTF8.GetString(Asset.Helper.ReadFile(vertexPath, normalizedVertexPath!));
                    var fragment = fragmentPath is null
                        ? null
                        : Encoding.UTF8.GetString(Asset.Helper.ReadFile(fragmentPath, normalizedFragmentPath!));
                    return new Shader(vertex, fragment);
                },
                cacheType
            );
        }

        public static Shader Resource(
            string? vertexResource,
            string? fragmentResource,
            string? @namespace = null,
            Assembly? assembly = null,
            CacheType? cacheType = null
        )
        {
            return Resource(vertexResource, fragmentResource, @namespace, @namespace, assembly, assembly, cacheType);
        }

        public static Shader Resource(
            string? vertexResource,
            string? fragmentResource,
            string? vertexNamespace,
            string? fragmentNamespace,
            Assembly? vertexAssembly,
            Assembly? fragmentAssembly,
            CacheType? cacheType = null
        )
        {
            var normalizedVertexPath = vertexResource is null
                ? null
                : Asset.Helper.NormalizeResource(vertexResource, vertexNamespace);
            var normalizedFragmentPath = fragmentResource is null
                ? null
                : Asset.Helper.NormalizeResource(fragmentResource, fragmentNamespace);
            return _container.Resource(
                () =>
                    (
                        normalizedVertexPath is null
                            ? null
                            : FileSystem.FormatResource(normalizedVertexPath, vertexAssembly?.FullName ?? ""),
                        normalizedFragmentPath is null
                            ? null
                            : FileSystem.FormatResource(normalizedFragmentPath, fragmentAssembly?.FullName ?? "")
                    ),
                () =>
                {
                    var vertex = normalizedVertexPath is null
                        ? null
                        : Encoding.UTF8.GetString(Asset.Helper.ReadResource(normalizedVertexPath, vertexAssembly));
                    var fragment = normalizedFragmentPath is null
                        ? null
                        : Encoding.UTF8.GetString(Asset.Helper.ReadResource(normalizedFragmentPath, fragmentAssembly!));
                    return new Shader(vertex, fragment);
                },
                cacheType
            );
        }

        public static Shader Raw(string? vertex, string? fragment, CacheType? cacheType = null)
        {
            return _container.Raw((vertex, fragment), () => new Shader(vertex, fragment), cacheType);
        }

        public static void Invalidate(Shader shader)
        {
            _container.Invalidate(shader);
        }
    }
}

public static class VertexShaderAssetExtensions
{
    extension(Shader.Vertex)
    {
        public static Shader File(string path, CacheType? cacheType = null)
        {
            return Shader.File(path, null, cacheType);
        }

        public static Shader Resource(
            string resource,
            string? @namespace = null,
            Assembly? assembly = null,
            CacheType? cacheType = null
        )
        {
            return Shader.Resource(resource, null, @namespace, assembly, cacheType);
        }

        public static Shader Raw(string vertex, CacheType? cacheType = null)
        {
            return Shader.Raw(vertex, null, cacheType);
        }
    }
}

public static class FragmentShaderAssetExtensions
{
    extension(Shader.Fragment)
    {
        public static Shader File(string path, CacheType? cacheType = null)
        {
            return Shader.File(null, path, cacheType);
        }

        public static Shader Resource(
            string resource,
            string? @namespace = null,
            Assembly? assembly = null,
            CacheType? cacheType = null
        )
        {
            return Shader.Resource(null, resource, @namespace, assembly, cacheType);
        }

        public static Shader Raw(string fragment, CacheType? cacheType = null)
        {
            return Shader.Raw(null, fragment, cacheType);
        }
    }
}
