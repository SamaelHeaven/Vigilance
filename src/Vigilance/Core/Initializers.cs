using System.Globalization;
using System.Runtime.CompilerServices;
using Vigilance.Drawing;
using Vigilance.Logging;

namespace Vigilance.Core;

public static class Initializers
{
    private static bool _initialized = false;

#pragma warning disable CA2255
    [ModuleInitializer]
#pragma warning restore CA2255
    public static void Run()
    {
        if (_initialized)
            return;
        _initialized = true;
        InitializeCultureInfo();
        Logger.LogLevel = LogLevel.None;
        Game.Defer(() =>
        {
            FileSystem.Initialize();
            Asset.Initialize();
            Font.Initialize();
            Audio.Audio.Initialize();
            Input.Input.Initialize();
            Drawing.Drawing.Initialize();
        });
    }

    private static void InitializeCultureInfo()
    {
        var cultureInfo = CultureInfo.InvariantCulture;
        CultureInfo.CurrentCulture = cultureInfo;
        CultureInfo.CurrentUICulture = cultureInfo;
        CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
        CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;
    }
}
