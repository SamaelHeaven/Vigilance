using System.Diagnostics.CodeAnalysis;
using Vigilance.Audio;
using Vigilance.Drawing;

namespace Vigilance.Core;

public static class TextureAssetManager
{
    private static Asset.Container<string, Texture> _fileContainer = new();
    private static Asset.Container<Resource, Texture> _resourceContainer = new();

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
            return _fileContainer.File(
                ref path,
                () => path,
                bytes => new Texture(Path.GetExtension(path.AsSpan()), bytes),
                cacheType,
                out texture
            );
        }

        public static Texture Resource(in Resource resource, CacheType? cacheType = null)
        {
            return !Texture.Resource(resource, out var texture, cacheType)
                ? throw new AssetException($"Failed to load texture from resource: {resource}")
                : texture;
        }

        public static bool Resource(
            Resource resource,
            [MaybeNullWhen(false)] out Texture texture,
            CacheType? cacheType = null
        )
        {
            return _resourceContainer.Resource(
                resource,
                () => resource,
                bytes => new Texture(Path.GetExtension(resource.Name.AsSpan()), bytes),
                cacheType,
                out texture
            );
        }
    }
}

public static class ImageAssetManager
{
    private static Asset.Container<string, Image> _fileContainer = new();
    private static Asset.Container<Resource, Image> _resourceContainer = new();

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
            return _fileContainer.File(
                ref path,
                () => path,
                bytes => new Image(Path.GetExtension(path.AsSpan()), bytes),
                cacheType,
                out image
            );
        }

        public static Image Resource(in Resource resource, CacheType? cacheType = null)
        {
            return !Image.Resource(resource, out var image, cacheType)
                ? throw new AssetException($"Failed to load image from resource: {resource}")
                : image;
        }

        public static bool Resource(
            Resource resource,
            [MaybeNullWhen(false)] out Image image,
            CacheType? cacheType = null
        )
        {
            return _resourceContainer.Resource(
                resource,
                () => resource,
                bytes => new Image(Path.GetExtension(resource.Name.AsSpan()), bytes),
                cacheType,
                out image
            );
        }
    }
}

public static class FontAssetManager
{
    private static Asset.Container<(string Path, int Quality, string Charset), Font> _fileContainer = new();
    private static Asset.Container<(Resource Resource, int Quality, string Charset), Font> _resourceContainer = new();

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
            return _fileContainer.File(
                ref path,
                () => (path, quality ?? Font.DefaultQuality, charset ?? Font.DefaultCharset),
                bytes => new Font(bytes, quality ?? Font.DefaultQuality, charset ?? Font.DefaultCharset),
                cacheType,
                out font
            );
        }

        public static Font Resource(
            in Resource resource,
            int? quality = null,
            string? charset = null,
            CacheType? cacheType = null
        )
        {
            return !Font.Resource(resource, out var font, quality, charset, cacheType)
                ? throw new AssetException($"Failed to load font from resource: {resource}")
                : font;
        }

        public static bool Resource(
            Resource resource,
            [MaybeNullWhen(false)] out Font font,
            int? quality = null,
            string? charset = null,
            CacheType? cacheType = null
        )
        {
            return _resourceContainer.Resource(
                resource,
                () => (resource, quality ?? Font.DefaultQuality, charset ?? Font.DefaultCharset),
                bytes => new Font(bytes, quality ?? Font.DefaultQuality, charset ?? Font.DefaultCharset),
                cacheType,
                out font
            );
        }
    }
}

public static class MusicAssetManager
{
    private static Asset.Container<string, Music> _fileContainer = new();
    private static Asset.Container<Resource, Music> _resourceContainer = new();

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
            return _fileContainer.File(
                ref path,
                () => path,
                bytes => new Music(Path.GetExtension(path.AsSpan()), bytes),
                cacheType,
                out music
            );
        }

        public static Music Resource(in Resource resource, CacheType? cacheType = null)
        {
            return !Music.Resource(resource, out var music, cacheType)
                ? throw new AssetException($"Failed to load music from resource: {resource}")
                : music;
        }

        public static bool Resource(
            Resource resource,
            [MaybeNullWhen(false)] out Music music,
            CacheType? cacheType = null
        )
        {
            return _resourceContainer.Resource(
                resource,
                () => resource,
                bytes => new Music(Path.GetExtension(resource.Name.AsSpan()), bytes),
                cacheType,
                out music
            );
        }
    }
}

public static class SoundAssetManager
{
    private static Asset.Container<(string Path, int MaxAliases), Sound> _fileContainer = new();
    private static Asset.Container<(Resource Resource, int MaxAliases), Sound> _resourceContainer = new();

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
            return _fileContainer.File(
                ref path,
                () => (path, maxAliases ?? Audio.Audio.DefaultSoundMaxAliases),
                bytes => new Sound(
                    Path.GetExtension(path.AsSpan()),
                    bytes,
                    maxAliases ?? Audio.Audio.DefaultSoundMaxAliases
                ),
                cacheType,
                out sound
            );
        }

        public static Sound Resource(in Resource resource, int? maxAliases = null, CacheType? cacheType = null)
        {
            return !Sound.Resource(resource, out var sound, maxAliases, cacheType)
                ? throw new AssetException($"Failed to load sound from resource: {resource}")
                : sound;
        }

        public static bool Resource(
            Resource resource,
            [MaybeNullWhen(false)] out Sound sound,
            int? maxAliases = null,
            CacheType? cacheType = null
        )
        {
            return _resourceContainer.Resource(
                resource,
                () => (resource, maxAliases ?? Audio.Audio.DefaultSoundMaxAliases),
                bytes => new Sound(
                    Path.GetExtension(resource.Name.AsSpan()),
                    bytes,
                    maxAliases ?? Audio.Audio.DefaultSoundMaxAliases
                ),
                cacheType,
                out sound
            );
        }
    }
}

public static class ShaderAssetManager
{
    private static Asset.Container<(string? VertexPath, string? FragmentPath), Shader> _fileContainer = new();

    private static Asset.Container<(Resource? VertexResource, Resource? FragmentResource), Shader> _resourceContainer =
        new();

    private static Asset.Container<(string? Vertex, string? Fragment), Shader> _rawContainer = new();

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
            return _fileContainer.File(
                () =>
                    (
                        vertexPath is null ? null : FileSystem.NormalizePath(vertexPath),
                        fragmentPath is null ? null : FileSystem.NormalizePath(fragmentPath)
                    ),
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
            in Resource? vertexResource,
            in Resource? fragmentResource,
            CacheType? cacheType = null
        )
        {
            if (!Shader.Resource(vertexResource, fragmentResource, out var shader, cacheType))
                throw new AssetException(
                    $"Failed to load shader from resource{(vertexResource is not null && fragmentResource is not null ? "s" : "")}: {vertexResource}{(vertexResource is null ? "" : ", ")}{fragmentResource}"
                );
            return shader;
        }

        public static bool Resource(
            in Resource? vertexResource,
            in Resource? fragmentResource,
            [MaybeNullWhen(false)] out Shader shader,
            CacheType? cacheType = null
        )
        {
            return Shader.Resource(vertexResource, fragmentResource, out shader, cacheType);
        }

        public static bool Resource(
            Resource? vertexResource,
            Resource? fragmentResource,
            [MaybeNullWhen(false)] out Shader shader,
            CacheType? cacheType = null
        )
        {
            return _resourceContainer.Resource(
                () => (vertexResource, fragmentResource),
                () =>
                {
                    string? vertex = null;
                    if (vertexResource is not null)
                        if (!Core.Resource.TryReadText(vertexResource.Value, out vertex))
                            return null;
                    string? fragment = null;
                    if (fragmentResource is null)
                        return new Shader(vertex, fragment);
                    return !Core.Resource.TryReadText(fragmentResource.Value, out fragment)
                        ? null
                        : new Shader(vertex, fragment);
                },
                cacheType,
                out shader
            );
        }

        public static Shader Raw(string? vertex, string? fragment, CacheType? cacheType = null)
        {
            _rawContainer.Raw((vertex, fragment), () => new Shader(vertex, fragment), cacheType, out var value);
            return value!;
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

        public static Shader Resource(in Resource resource, CacheType? cacheType = null)
        {
            return Shader.Resource(resource, null, cacheType);
        }

        public static bool Resource(
            in Resource resource,
            [MaybeNullWhen(false)] out Shader shader,
            CacheType? cacheType = null
        )
        {
            return Shader.Resource(resource, null, out shader, cacheType);
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

        public static Shader Resource(in Resource resource, CacheType? cacheType = null)
        {
            return Shader.Resource(null, resource, cacheType);
        }

        public static bool Resource(
            in Resource resource,
            [MaybeNullWhen(false)] out Shader shader,
            CacheType? cacheType = null
        )
        {
            return Shader.Resource(null, resource, out shader, cacheType);
        }

        public static Shader Raw(string fragment, CacheType? cacheType = null)
        {
            return Shader.Raw(null, fragment, cacheType);
        }
    }
}
