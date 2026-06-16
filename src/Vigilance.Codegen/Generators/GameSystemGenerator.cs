using System.Text;
using Microsoft.CodeAnalysis;
using Vigilance.Codegen.Helpers;

namespace Vigilance.Codegen.Generators;

[Generator]
public sealed class GameSystemGenerator : SourceGenerator
{
    protected override void Generate(StringBuilder sb)
    {
        sb.AppendLine(
            """
            #pragma warning disable CS9084

            namespace Vigilance.Core;

            public abstract partial class GameSystem
            {

            """
        );
        Entities(sb);
        Components(sb);
        Entries(sb);
        AssignableEntities(sb);
        AssignableComponents(sb);
        AssignableEntries(sb);
        TableEntities(sb);
        TableComponents(sb);
        TableEntries(sb);
        sb.AppendLine("}");
    }

    private static void Entities(StringBuilder sb)
    {
        sb.BeginRegion("Entities");
        sb.AppendLine(QueryIterator("Entity", "Entities"));
        for (var i = 0; i < 16; i++)
        {
            var typeParams = string.Join(", ", Enumerable.Range(0, i + 1).Select(n => $"T{n}"));
            sb.AppendLine(QueryIterator("Entity", "Entities", $"<{typeParams}>"));
        }

        sb.EndRegion();
    }

    private static void Components(StringBuilder sb)
    {
        sb.BeginRegion("Components");
        for (var i = 0; i < 16; i++)
        {
            var typeParams = string.Join(", ", Enumerable.Range(0, i + 1).Select(n => $"T{n}"));
            sb.AppendLine(QueryIterator("Component", "Components", $"<{typeParams}>"));
        }

        sb.EndRegion();
    }

    private static void Entries(StringBuilder sb)
    {
        sb.BeginRegion("Entries");
        for (var i = 0; i < 15; i++)
        {
            var typeParams = string.Join(", ", Enumerable.Range(0, i + 1).Select(n => $"T{n}"));
            sb.AppendLine(QueryIterator("Entry", "Entries", $"<{typeParams}>"));
        }

        sb.EndRegion();
    }

    private static void AssignableEntities(StringBuilder sb)
    {
        sb.BeginRegion("AssignableEntities");
        sb.AppendLine(QueryIterator("AssignableEntity", "AssignableEntities", "<T0>"));
        sb.EndRegion();
    }

    private static void AssignableComponents(StringBuilder sb)
    {
        sb.BeginRegion("AssignableComponents");
        sb.AppendLine(QueryIterator("AssignableComponent", "AssignableComponents", "<T0>"));
        sb.EndRegion();
    }

    private static void AssignableEntries(StringBuilder sb)
    {
        sb.BeginRegion("AssignableEntries");
        sb.AppendLine(QueryIterator("AssignableEntries", "AssignableEntries", "<T0>"));
        sb.EndRegion();
    }

    private static void TableEntities(StringBuilder sb)
    {
        sb.BeginRegion("TableEntities");
        for (var i = 0; i < 16; i++)
        {
            var methodParams = string.Join(", ", Enumerable.Range(0, i + 1).Select(n => $"Table table{n}"));
            var methodArgs = string.Join(", ", Enumerable.Range(0, i + 1).Select(n => $"table{n}"));
            sb.AppendLine(QueryIterator($"TableEntity{i + 1}", "Entities", "", methodParams, methodArgs));
        }

        sb.EndRegion();
    }

    private static void TableComponents(StringBuilder sb)
    {
        sb.BeginRegion("TableComponents");
        for (var i = 0; i < 16; i++)
        {
            var methodParams = string.Join(", ", Enumerable.Range(0, i + 1).Select(n => $"Table table{n}"));
            var methodArgs = string.Join(", ", Enumerable.Range(0, i + 1).Select(n => $"table{n}"));
            sb.AppendLine(QueryIterator($"TableComponent{i + 1}", "Components", "", methodParams, methodArgs));
        }

        sb.EndRegion();
    }

    private static void TableEntries(StringBuilder sb)
    {
        sb.BeginRegion("TableEntries");
        for (var i = 0; i < 15; i++)
        {
            var methodParams = string.Join(", ", Enumerable.Range(0, i + 1).Select(n => $"Table table{n}"));
            var methodArgs = string.Join(", ", Enumerable.Range(0, i + 1).Select(n => $"table{n}"));
            sb.AppendLine(QueryIterator($"TableEntry{i + 1}", "Entries", "", methodParams, methodArgs));
        }

        sb.EndRegion();
    }

    private static string QueryIterator(
        string name,
        string methodName,
        string typeParams = "",
        string methodParams = "",
        string methodArgs = ""
    )
    {
        return $$"""
                public Scene.{{name}}Enumerable{{typeParams}} {{methodName}}{{typeParams}}({{methodParams}}) {
                    return Scene.{{methodName}}{{typeParams}}({{methodArgs}}).WithDisabled(QueryWithDisabled).Deferred(QueryDeferred);
                }

            """;
    }
}
