using ZLinq;

namespace Vigilance.Core;

public enum Platform : sbyte
{
    Unknown,
    Desktop,
    Web,
}

public static class PlatformExtensions
{
    private static readonly Platform _current = Platform
        .Values()
        .AsValueEnumerable()
        .FirstOrDefault(platform => platform.IsCurrent);

    extension(Platform platform)
    {
        public static Platform Current => _current;

        public bool IsCurrent
        {
            get
            {
                if (OperatingSystem.IsBrowser())
                    return platform == Platform.Web;
                if (OperatingSystem.IsWindows() || OperatingSystem.IsMacOS() || OperatingSystem.IsLinux())
                    return platform == Platform.Desktop;
                return platform == Platform.Unknown;
            }
        }
    }
}
