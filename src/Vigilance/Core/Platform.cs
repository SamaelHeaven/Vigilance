namespace Vigilance.Core;

public enum Platform : sbyte
{
    Unknown,
    Desktop,
    Mobile,
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
                if (OperatingSystem.IsAndroid() || OperatingSystem.IsIOS())
                    return platform == Platform.Mobile;
                if (
                    OperatingSystem.IsWindows()
                    || OperatingSystem.IsMacOS()
                    || OperatingSystem.IsLinux()
                    || OperatingSystem.IsFreeBSD()
                )
                    return platform == Platform.Desktop;
                return platform == Platform.Unknown;
            }
        }

        public bool SupportsThreads =>
            platform switch
            {
                Platform.Desktop => true,
                _ => false,
            };
    }
}
