using Vigilance.Core;
using Vigilance.Math;

namespace Vigilance.Drawing;

public sealed class FontConfig
{
    public int DefaultQuality { get; set; } = 64;
    public int DefaultSize { get; set; } = 16;
    public TextHeightMode DefaultTextHeightMode { get; set; } = TextHeightMode.Character;
    public Vector2 DefaultTextSpacing { get; set; } = new(0, 4);

    public string DefaultCharset { get; set; } =
        "!\"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~";

    public Func<Font> Default { get; set; } = () => Font.Resource(("Font.default.ttf", Assemblies.Engine));
}

public static class FontConfigExtensions
{
    public static ConfigBuilder Font(this ConfigBuilder builder, Action<FontConfig> config)
    {
        return builder.Add(config);
    }
}
