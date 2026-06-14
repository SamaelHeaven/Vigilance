using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Vigilance.Audio;
using Vigilance.Drawing;

namespace Vigilance.Core;

public static class TextureAssetManager
{
    private static readonly Asset.Container<string, Texture> _container = new();

    public static Action<Texture>? OnInvalidate { get; set; }

    extension(Texture)
    {
        public static Texture File(string path, CacheType? cacheType = null)
        {
            return !Texture.File(path, out var texture, cacheType)
                ? throw new AssetException($"Failed to load texture from file: {FileSystem.NormalizePath(path)}")
                : texture;
        }

        public static bool File(string path, [MaybeNullWhen(false)] out Texture texture, CacheType? cacheType = null)
        {
            return _container.File(
                ref path,
                () => path,
                bytes => new Texture(Path.GetExtension(path), bytes),
                cacheType,
                out texture
            );
        }

        public static Texture Resource(
            string resource,
            string? @namespace = null,
            Assembly? assembly = null,
            CacheType? cacheType = null
        )
        {
            return !Texture.Resource(resource, out var texture, @namespace, assembly, cacheType)
                ? throw new AssetException(
                    $"Failed to load texture from resource: {Core.Resource.Format(resource, @namespace, assembly)}"
                )
                : texture;
        }

        public static bool Resource(
            string resource,
            [MaybeNullWhen(false)] out Texture texture,
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
                cacheType,
                out texture
            );
        }

        public static void Invalidate(Texture texture)
        {
            _container.Invalidate(texture);
            OnInvalidate?.Invoke(texture);
        }
    }
}

public static class ImageAssetManager
{
    private static readonly Asset.Container<string, Image> _container = new();

    public static Action<Image>? OnInvalidate { get; set; }

    extension(Image)
    {
        public static Image File(string path, CacheType? cacheType = null)
        {
            return !Image.File(path, out var image, cacheType)
                ? throw new AssetException($"Failed to load image from file: {FileSystem.NormalizePath(path)}")
                : image;
        }

        public static bool File(string path, [MaybeNullWhen(false)] out Image image, CacheType? cacheType = null)
        {
            return _container.File(
                ref path,
                () => path,
                bytes => new Image(Path.GetExtension(path), bytes),
                cacheType,
                out image
            );
        }

        public static Image Resource(
            string resource,
            string? @namespace = null,
            Assembly? assembly = null,
            CacheType? cacheType = null
        )
        {
            return !Image.Resource(resource, out var image, @namespace, assembly, cacheType)
                ? throw new AssetException(
                    $"Failed to load image from resource: {Core.Resource.Format(resource, @namespace, assembly)}"
                )
                : image;
        }

        public static bool Resource(
            string resource,
            [MaybeNullWhen(false)] out Image image,
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
                cacheType,
                out image
            );
        }

        public static void Invalidate(Image image)
        {
            _container.Invalidate(image);
            OnInvalidate?.Invoke(image);
        }
    }
}

public static class FontAssetManager
{
    private static readonly Asset.Container<(string Key, int Quality, string Charset), Font> _container = new();

    public static Action<Font>? OnInvalidate { get; set; }

    extension(Font)
    {
        public static Font File(string path, int? quality = null, string? charset = null, CacheType? cacheType = null)
        {
            return !Font.File(path, out var font, quality, charset, cacheType)
                ? throw new AssetException($"Failed to load font from file: {FileSystem.NormalizePath(path)}")
                : font;
        }

        public static bool File(
            string path,
            [MaybeNullWhen(false)] out Font font,
            int? quality = null,
            string? charset = null,
            CacheType? cacheType = null
        )
        {
            return _container.File(
                ref path,
                () => (path, quality ?? Font.DefaultQuality, charset ?? Font.DefaultCharset),
                bytes => new Font(bytes, quality ?? Font.DefaultQuality, charset ?? Font.DefaultCharset),
                cacheType,
                out font
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
            return !Font.Resource(resource, out var font, quality, charset, @namespace, assembly, cacheType)
                ? throw new AssetException(
                    $"Failed to load font from resource: {Core.Resource.Format(resource, @namespace, assembly)}"
                )
                : font;
        }

        public static bool Resource(
            string resource,
            [MaybeNullWhen(false)] out Font font,
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
                cacheType,
                out font
            );
        }

        public static void Invalidate(Font font)
        {
            _container.Invalidate(font);
            OnInvalidate?.Invoke(font);
        }
    }
}

public static class MusicAssetManager
{
    private static readonly Asset.Container<string, Music> _container = new();

    public static Action<Music>? OnInvalidate { get; set; }

    extension(Music)
    {
        public static Music File(string path, CacheType? cacheType = null)
        {
            return !Music.File(path, out var music, cacheType)
                ? throw new AssetException($"Failed to load music from file: {FileSystem.NormalizePath(path)}")
                : music;
        }

        public static bool File(string path, [MaybeNullWhen(false)] out Music music, CacheType? cacheType = null)
        {
            return _container.File(
                ref path,
                () => path,
                bytes => new Music(Path.GetExtension(path), bytes),
                cacheType,
                out music
            );
        }

        public static Music Resource(
            string resource,
            string? @namespace = null,
            Assembly? assembly = null,
            CacheType? cacheType = null
        )
        {
            return !Music.Resource(resource, out var music, @namespace, assembly, cacheType)
                ? throw new AssetException(
                    $"Failed to load music from resource: {Core.Resource.Format(resource, @namespace, assembly)}"
                )
                : music;
        }

        public static bool Resource(
            string resource,
            [MaybeNullWhen(false)] out Music music,
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
                cacheType,
                out music
            );
        }

        public static void Invalidate(Music music)
        {
            _container.Invalidate(music);
            OnInvalidate?.Invoke(music);
        }
    }
}

public static class SoundAssetManager
{
    private static readonly Asset.Container<(string Key, int MaxAliases), Sound> _container = new();

    public static Action<Sound>? OnInvalidate { get; set; }

    extension(Sound)
    {
        public static Sound File(string path, int? maxAliases = null, CacheType? cacheType = null)
        {
            return !Sound.File(path, out var sound, maxAliases, cacheType)
                ? throw new AssetException($"Failed to load sound from file: {FileSystem.NormalizePath(path)}")
                : sound;
        }

        public static bool File(
            string path,
            [MaybeNullWhen(false)] out Sound sound,
            int? maxAliases = null,
            CacheType? cacheType = null
        )
        {
            return _container.File(
                ref path,
                () => (path, maxAliases ?? Audio.Audio.DefaultSoundMaxAliases),
                bytes => new Sound(Path.GetExtension(path), bytes, maxAliases ?? Audio.Audio.DefaultSoundMaxAliases),
                cacheType,
                out sound
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
            return !Sound.Resource(resource, out var sound, maxAliases, @namespace, assembly, cacheType)
                ? throw new AssetException(
                    $"Failed to load sound from resource: {Core.Resource.Format(resource, @namespace, assembly)}"
                )
                : sound;
        }

        public static bool Resource(
            string resource,
            [MaybeNullWhen(false)] out Sound sound,
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
                cacheType,
                out sound
            );
        }

        public static void Invalidate(Sound sound)
        {
            _container.Invalidate(sound);
            OnInvalidate?.Invoke(sound);
        }
    }
}

public static class ShaderAssetManager
{
    private static readonly Asset.Container<(string? VertexKey, string? FragmentKey), Shader> _container = new();

    public static Action<Shader>? OnInvalidate { get; set; }

    extension(Shader)
    {
        public static Shader File(string? vertexPath, string? fragmentPath, CacheType? cacheType = null)
        {
            return !Shader.File(vertexPath, fragmentPath, out var shader, cacheType)
                ? throw new AssetException(
                    $"Failed to load shader from file{(vertexPath is not null && fragmentPath is not null ? "s" : "")}: {(vertexPath is null ? "" : FileSystem.NormalizePath(vertexPath))}{(vertexPath is null ? "" : ", ")}{(fragmentPath is null ? "" : FileSystem.NormalizePath(fragmentPath))}"
                )
                : shader;
        }

        public static bool File(
            string? vertexPath,
            string? fragmentPath,
            [MaybeNullWhen(false)] out Shader shader,
            CacheType? cacheType = null
        )
        {
            var normalizedVertexPath = vertexPath is null ? null : FileSystem.NormalizePath(vertexPath);
            var normalizedFragmentPath = fragmentPath is null ? null : FileSystem.NormalizePath(fragmentPath);
            return _container.File(
                () => (normalizedVertexPath, normalizedFragmentPath),
                () =>
                {
                    string? vertex = null;
                    if (vertexPath is not null)
                        if (!FileSystem.TryReadText(vertexPath, out vertex))
                            return null;
                    string? fragment = null;
                    if (fragmentPath is null)
                        return new Shader(vertex, fragment);
                    return !FileSystem.TryReadText(fragmentPath, out fragment) ? null : new Shader(vertex, fragment);
                },
                cacheType,
                out shader
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
            return Shader.Resource(
                vertexResource,
                fragmentResource,
                @namespace,
                @namespace,
                assembly,
                assembly,
                cacheType
            );
        }

        public static bool Resource(
            string? vertexResource,
            string? fragmentResource,
            [MaybeNullWhen(false)] out Shader shader,
            string? @namespace = null,
            Assembly? assembly = null,
            CacheType? cacheType = null
        )
        {
            return Shader.Resource(
                vertexResource,
                fragmentResource,
                @namespace,
                @namespace,
                assembly,
                assembly,
                out shader,
                cacheType
            );
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
            return !Shader.Resource(
                vertexResource,
                fragmentResource,
                vertexNamespace,
                fragmentNamespace,
                vertexAssembly,
                fragmentAssembly,
                out var shader,
                cacheType
            )
                ? throw new AssetException(
                    $"Failed to load shader from resource{(vertexResource is not null && fragmentResource is not null ? "s" : "")}: {(vertexResource is null ? "" : Core.Resource.Format(vertexResource, vertexNamespace))}{(vertexResource is null ? "" : ", ")}{(fragmentResource is null ? "" : Core.Resource.Format(fragmentResource, fragmentNamespace))}"
                )
                : shader;
        }

        public static bool Resource(
            string? vertexResource,
            string? fragmentResource,
            string? vertexNamespace,
            string? fragmentNamespace,
            Assembly? vertexAssembly,
            Assembly? fragmentAssembly,
            [MaybeNullWhen(false)] out Shader shader,
            CacheType? cacheType = null
        )
        {
            var vertexPath = vertexResource is null ? null : Core.Resource.Format(vertexResource, vertexNamespace);
            var fragmentPath = fragmentResource is null
                ? null
                : Core.Resource.Format(fragmentResource, fragmentNamespace);
            return _container.Resource(
                () =>
                    (
                        vertexPath is null ? null : Core.Resource.Format(vertexPath, vertexAssembly?.FullName ?? ""),
                        fragmentPath is null
                            ? null
                            : Core.Resource.Format(fragmentPath, fragmentAssembly?.FullName ?? "")
                    ),
                () =>
                {
                    string? vertex = null;
                    if (vertexPath is not null)
                        if (!Core.Resource.TryReadText(vertexPath, out vertex, "", vertexAssembly))
                            return null;
                    string? fragment = null;
                    if (fragmentPath is null)
                        return new Shader(vertex, fragment);
                    return !Core.Resource.TryReadText(fragmentPath, out fragment, "", fragmentAssembly)
                        ? null
                        : new Shader(vertex, fragment);
                },
                cacheType,
                out shader
            );
        }

        public static Shader Raw(string? vertex, string? fragment, CacheType? cacheType = null)
        {
            _container.Raw((vertex, fragment), () => new Shader(vertex, fragment), cacheType, out var value);
            return value!;
        }

        public static void Invalidate(Shader shader)
        {
            _container.Invalidate(shader);
            OnInvalidate?.Invoke(shader);
        }
    }
}

public static class VertexShaderAssetManager
{
    extension(Shader.Vertex)
    {
        public static Shader File(string path, CacheType? cacheType = null)
        {
            return Shader.File(path, null, cacheType);
        }

        public static bool File(string path, [MaybeNullWhen(false)] out Shader shader, CacheType? cacheType = null)
        {
            return Shader.File(path, null, out shader, cacheType);
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

        public static bool Resource(
            string resource,
            [MaybeNullWhen(false)] out Shader shader,
            string? @namespace = null,
            Assembly? assembly = null,
            CacheType? cacheType = null
        )
        {
            return Shader.Resource(resource, null, out shader, @namespace, assembly, cacheType);
        }

        public static Shader Raw(string vertex, CacheType? cacheType = null)
        {
            return Shader.Raw(vertex, null, cacheType);
        }
    }
}

public static class FragmentShaderAssetManager
{
    extension(Shader.Fragment)
    {
        public static Shader File(string path, CacheType? cacheType = null)
        {
            return Shader.File(null, path, cacheType);
        }

        public static bool File(string path, [MaybeNullWhen(false)] out Shader shader, CacheType? cacheType = null)
        {
            return Shader.File(null, path, out shader, cacheType);
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

        public static bool Resource(
            string resource,
            [MaybeNullWhen(false)] out Shader shader,
            string? @namespace = null,
            Assembly? assembly = null,
            CacheType? cacheType = null
        )
        {
            return Shader.Resource(null, resource, out shader, @namespace, assembly, cacheType);
        }

        public static Shader Raw(string fragment, CacheType? cacheType = null)
        {
            return Shader.Raw(null, fragment, cacheType);
        }
    }
}
