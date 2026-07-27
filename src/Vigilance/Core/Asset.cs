using System.Diagnostics.CodeAnalysis;

namespace Vigilance.Core;

public static class Asset
{
    private static AssetConfig _config = new();

    public static CacheType DefaultCacheType { get; set; } = _config.DefaultCacheType;

    internal static void Initialize()
    {
        _config = Game.Config.Take<AssetConfig>() ?? _config;
        DefaultCacheType = _config.DefaultCacheType;
    }

    public struct Container<TKey, TValue>
        where TKey : notnull
        where TValue : class
    {
        private ValueDictionary<TKey, TValue> _strongValues = [];
        private ValueDictionary<TKey, WeakReference<TValue>> _weakValues = [];

        public Container() { }

        public bool File(
            ref string path,
            Func<TKey> keyFunc,
            Func<byte[], TValue> valueFunc,
            CacheType? cacheType,
            [MaybeNullWhen(false)] out TValue value
        )
        {
            var filePath = path;
            var normalizedPath = FileSystem.NormalizePath(path);
            path = normalizedPath;
            return File(
                keyFunc,
                () => FileSystem.TryReadBytes(filePath, out var bytes) ? valueFunc.SafeInvoke(bytes) : null,
                cacheType,
                out value
            );
        }

        public bool File(
            Func<TKey> keyFunc,
            Func<TValue?> valueFunc,
            CacheType? cacheType,
            [MaybeNullWhen(false)] out TValue value
        )
        {
            return Get(keyFunc, valueFunc, cacheType, out value);
        }

        public bool Resource(
            Resource resource,
            Func<TKey> keyFunc,
            Func<byte[], TValue> valueFunc,
            CacheType? cacheType,
            [MaybeNullWhen(false)] out TValue value
        )
        {
            return Resource(
                keyFunc,
                () => Core.Resource.TryReadBytes(resource, out var bytes) ? valueFunc.SafeInvoke(bytes) : null,
                cacheType,
                out value
            );
        }

        public bool Resource(
            Func<TKey> keyFunc,
            Func<TValue?> valueFunc,
            CacheType? cacheType,
            [MaybeNullWhen(false)] out TValue value
        )
        {
            return Get(keyFunc, valueFunc, cacheType, out value);
        }

        public bool Raw(TKey key, Func<TValue> valueFunc, CacheType? cacheType, [MaybeNullWhen(false)] out TValue value)
        {
            return Get(() => key, valueFunc, cacheType, out value);
        }

        private bool Get(
            Func<TKey> keyFunc,
            Func<TValue?> valueFunc,
            CacheType? cacheType,
            [MaybeNullWhen(false)] out TValue value
        )
        {
            try
            {
                var key = keyFunc.SafeInvoke();
                var cacheTypeValue = cacheType ?? DefaultCacheType;
                var weak = cacheTypeValue == CacheType.Weak;
                var strong = cacheTypeValue == CacheType.Strong;
                if (_weakValues.TryGetValue(key, out var reference))
                    if (reference.TryGetTarget(out value!))
                        return true;
                if (_strongValues.TryGetValue(key, out value!))
                    return true;
                value = valueFunc.SafeInvoke();
                if (value is null)
                    return false;
                if (weak)
                    _weakValues[key] = new WeakReference<TValue>(value);
                if (strong)
                    _strongValues[key] = value;
                return true;
            }
            catch (Exception e)
            {
                Log.Error(e);
                value = null;
                return false;
            }
        }
    }
}

public sealed class AssetConfig
{
    public CacheType DefaultCacheType { get; set; } = CacheType.Weak;
}

public static class AssetConfigExtensions
{
    public static ConfigBuilder Asset(this ConfigBuilder builder, Action<AssetConfig> config)
    {
        return builder.Add(config);
    }
}

public class AssetException : Exception
{
    public AssetException(string message)
        : base(message) { }
}
