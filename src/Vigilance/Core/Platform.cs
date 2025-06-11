namespace Vigilance.Core;

public enum Platform
{
    Unknown,
    Desktop,
    Web,
}

public static class PlatformExtensions
{
    public static bool IsCurrent(this Platform platform)
    {
        if (OperatingSystem.IsBrowser())
            return platform == Platform.Web;
        if (OperatingSystem.IsWindows() || OperatingSystem.IsMacOS() || OperatingSystem.IsLinux())
            return platform == Platform.Desktop;
        return platform == Platform.Unknown;
    }
}
