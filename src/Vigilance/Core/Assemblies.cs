using System.Reflection;

namespace Vigilance.Core;

public static class Assemblies
{
    public static Assembly Game { get; } = Assembly.GetEntryAssembly()!;

    public static Assembly Engine { get; } = Assembly.GetExecutingAssembly();
}
