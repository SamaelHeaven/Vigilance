namespace Vigilance.Core;

public enum Platform
{
    Desktop,
    Web,
    Unknown,
}

public static class PlatformExtensions
{
    public static bool IsCurrent(this Platform platform)
    {
        if (OperatingSystem.IsWindows() || OperatingSystem.IsMacOS() || OperatingSystem.IsLinux())
            return platform == Platform.Desktop;
        return platform == (OperatingSystem.IsBrowser() ? Platform.Web : Platform.Unknown);
    }
}
