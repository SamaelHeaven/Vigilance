using System.Text;
using Microsoft.CodeAnalysis;
using Vigilance.Codegen.Helpers;

namespace Vigilance.Codegen.Generators;

[Generator]
public sealed class RenderCommandsGenerator : SourceGenerator
{
    protected override void Generate(StringBuilder sb)
    {
        sb.AppendLine(
            """
            using Vigilance.Core;
            using ZLinq;

            namespace Vigilance.Drawing;

            public readonly ref partial struct RenderCommands 
            {

            """
        );
        AddEntriesEnumerable(sb, false);
        AddEntriesEnumerable(sb, true);
        AddAssignableEntriesEnumerable(sb, false);
        AddAssignableEntriesEnumerable(sb, true);
        AddEntriesGameSystem(sb, false);
        AddEntriesGameSystem(sb, true);
        AddAssignableEntriesGameSystem(sb, false);
        AddAssignableEntriesGameSystem(sb, true);
        sb.AppendLine("}");
    }

    private static void AddEntriesEnumerable(StringBuilder sb, bool system)
    {
        for (var i = 1; i < 15; i++)
        {
            var typeParams = string.Join(", ", Enumerable.Range(0, i + 1).Select(n => $"T{n}"));
            var items = string.Join(", ", Enumerable.Range(0, i + 1).Select(n => $"entry.Item{n + 2}"));
            var entryType = QueryHelper.NamedTuple(true, Enumerable.Range(0, i + 1).Select(n => $"T{n}").ToList());
            sb.AppendLine(
                $$"""
                    public void AddEntries<{{(system ? "TSystem, " : "")}}{{typeParams}}>({{(
                        system ? "TSystem system, " : ""
                    )}}{{$"ZLinq.ValueEnumerable<Scene.EntryEnumerator<{typeParams}>, {entryType}>"}} entries, Action<{{(
                    system ? "TSystem, " : ""
                )}}Entity, ({{typeParams}})> action)
                    {
                        foreach (var entry in entries)
                            Add({{(system ? "system, " : "")}}entry.Item1, ({{items}}), action);
                    }
                    
                """
            );
        }
    }

    private static void AddEntriesGameSystem(StringBuilder sb, bool system)
    {
        for (var i = 0; i < 15; i++)
        {
            var typeParams = string.Join(", ", Enumerable.Range(0, i + 1).Select(n => $"T{n}"));
            var type = i == 0 ? typeParams : $"({typeParams})";
            sb.AppendLine(
                $$"""
                    public void AddEntries<TSystem, {{typeParams}}>(TSystem system, Action<{{(
                        system ? "TSystem, " : ""
                    )}}Entity, {{type}}> action) where TSystem : GameSystem
                    {
                        AddEntries({{(system ? "system, " : "")}}system.Entries<{{typeParams}}>(), action);
                    }

                """
            );
        }
    }

    private static void AddAssignableEntriesEnumerable(StringBuilder sb, bool system)
    {
        sb.AppendLine(
            $$"""
                public void AddAssignableEntries<{{(system ? "TSystem, " : "")}}T0>({{(
                    system ? "TSystem system, " : ""
                )}}ZLinq.ValueEnumerable<Scene.AssignableEntriesEnumerator<T0>, (Entity Entity, T0 Component)> entries, Action<{{(
                system ? "TSystem, " : ""
            )}}Entity, T0> action)
                {
                    foreach (var entry in entries)
                        Add({{(system ? "system, " : "")}}entry.Item1, (T0)entry.Item2, action);
                }
                
            """
        );
    }

    private static void AddAssignableEntriesGameSystem(StringBuilder sb, bool system)
    {
        sb.AppendLine(
            $$"""
                public void AddAssignableEntries<TSystem, T0>(TSystem system, Action<{{(
                    system ? "TSystem, " : ""
                )}}Entity, T0> action) where TSystem : GameSystem
                {
                    AddAssignableEntries({{(system ? "system, " : "")}}system.AssignableEntries<T0>(), action);
                }

            """
        );
    }
}
