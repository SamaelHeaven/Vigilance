using System.Globalization;
using System.Runtime;
using System.Runtime.CompilerServices;
using Raylib_cs.BleedingEdge;
using Vigilance.Logging;
using Font = Vigilance.Drawing.Font;

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
        InitializeGC();
        InitializeCultureInfo();
        InitializeRaylib();
        Game.Defer(() =>
        {
            FileSystem.Initialize();
            Logger.Initialize();
            Display.Initialize();
            Asset.Initialize();
            Font.Initialize();
            Ecs.Initialize();
            Audio.Audio.Initialize();
            Input.Input.Initialize();
            Drawing.Drawing.Initialize();
        });
    }

    private static void InitializeGC()
    {
        GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;
    }

    private static void InitializeCultureInfo()
    {
        var cultureInfo = CultureInfo.InvariantCulture;
        CultureInfo.CurrentCulture = cultureInfo;
        CultureInfo.CurrentUICulture = cultureInfo;
        CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
        CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;
    }

    private static void InitializeRaylib()
    {
        Raylib.SetTraceLogLevel(TraceLogLevel.Error);
    }
}
