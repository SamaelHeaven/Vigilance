using Raylib_cs;
using Vigilance.Collections;
using Vigilance.Core;

namespace Vigilance.Drawing;

internal static class RenderTexturePool
{
    private static ValueList<(RenderTexture2D Texture, TimeSpan Time)> _entries = [];

    public static bool TryRent(int width, int height, out RenderTexture2D renderTexture2D)
    {
        var bestIndex = -1;
        for (var i = 0; i < _entries.Count; i++)
        {
            var candidate = _entries[i];
            if (candidate.Texture.Texture.Width < width || candidate.Texture.Texture.Height < height)
                continue;
            if (
                bestIndex >= 0
                && (long)candidate.Texture.Texture.Width * candidate.Texture.Texture.Height
                    >= (long)_entries[bestIndex].Texture.Texture.Width * _entries[bestIndex].Texture.Texture.Height
            )
                continue;
            bestIndex = i;
        }

        if (bestIndex < 0)
        {
            renderTexture2D = default;
            return false;
        }

        var best = _entries[bestIndex];
        _entries[bestIndex] = _entries[^1];
        _entries.RemoveAt(_entries.Count - 1);
        renderTexture2D = best.Texture;
        return true;
    }

    public static void Return(RenderTexture2D renderTexture2D)
    {
        if (renderTexture2D.Texture.Id == 0)
            return;
        _entries.Add((renderTexture2D, Time.Elapsed));
    }

    internal static void Update()
    {
        var now = Time.Elapsed;
        _entries.RemoveAll(entry =>
        {
            if (now - entry.Time <= Drawing.RenderTexturePoolLifetime)
                return false;
            Raylib.UnloadRenderTexture(entry.Texture);
            return true;
        });
    }
}
