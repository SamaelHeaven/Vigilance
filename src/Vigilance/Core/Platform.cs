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
        return platform == (OperatingSystem.IsBrowser() ? Platform.Web : Platform.Desktop);
    }
}
