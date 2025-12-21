using System.Text;
using Microsoft.CodeAnalysis;

namespace Vigilance.Codegen.Generators;

[Generator]
public sealed class RenderCommandsGenerator : SourceGenerator
{
    protected override void Generate(StringBuilder sb)
    {
        sb.AppendLine(
            """
            using Vigilance.Core;

            namespace Vigilance.Drawing;

            public readonly ref partial struct RenderCommands 
            {

            """
        );
        AddRangeEnumerable(sb, false);
        AddRangeEnumerable(sb, true);
        AddRangeGameSystem(sb, false);
        AddRangeGameSystem(sb, true);
        sb.AppendLine("}");
    }

    private static void AddRangeEnumerable(StringBuilder sb, bool context)
    {
        for (var i = 1; i < 15; i++)
        {
            var typeParams = string.Join(", ", Enumerable.Range(0, i + 1).Select(n => $"T{n}"));
            var items = string.Join(", ", Enumerable.Range(0, i + 1).Select(n => $"entry.Item{n + 2}"));
            sb.AppendLine(
                $$"""
                    public void AddRange<{{(context ? "TContext, " : "")}}{{typeParams}}>({{(
                          context ? "TContext context, " : ""
                      )}}{{$"Scene.EntryEnumerable<{typeParams}>"}} entries, Action<Entity, {{(
                      context ? "TContext, " : ""
                  )}}({{typeParams}})> action)
                    {
                        foreach (var entry in entries)
                            Add(entry.Item1, {{(context ? "context, " : "")}}({{items}}), action);
                    }
                    
                """
            );
        }
    }

    private static void AddRangeGameSystem(StringBuilder sb, bool context)
    {
        for (var i = 0; i < 15; i++)
        {
            var typeParams = string.Join(", ", Enumerable.Range(0, i + 1).Select(n => $"T{n}"));
            var type = i == 0 ? typeParams : $"({typeParams})";
            sb.AppendLine(
                $$"""
                public void AddRange<TSystem, {{typeParams}}>(TSystem system, Action<Entity, {{(
                      context ? "TSystem, " : ""
                  )}} {{type}}> action) where TSystem : GameSystem
                {
                    AddRange({{(context ? "system, " : "")}}system.Entries<{{typeParams}}>(), action);
                }

                """
            );
        }
    }
}
