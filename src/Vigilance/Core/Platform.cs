namespace Vigilance.Core;

public enum Platform
{
    Unknown,
    Desktop,
    Web,
}

public static class PlatformExtensions
{
    extension(Platform platform)
    {
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
