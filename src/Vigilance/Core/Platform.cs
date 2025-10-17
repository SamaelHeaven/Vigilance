namespace Vigilance.Core;

public enum Platform
{
    Unknown,
    Desktop,
    Web,
}

public static class PlatformExtensions
{
    private static readonly Platform _current = Enum.GetValues<Platform>()
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
