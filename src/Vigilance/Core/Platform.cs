namespace Vigilance.Core;

public enum Platform
{
    Desktop,
    Web,
}

public static class PlatformExtensions
{
    public static bool IsCurrent(this Platform platform)
    {
        if (OperatingSystem.IsBrowser())
            return platform == Platform.Web;
        return platform == Platform.Desktop;
    }
}
