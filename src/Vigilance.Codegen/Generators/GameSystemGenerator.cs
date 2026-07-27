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
            namespace Vigilance.Core;

            public abstract partial class GameSystem
            {

            """
        );
        Entities(sb);
        Components(sb);
        Entries(sb);
        RefComponents(sb);
        RefEntries(sb);
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
        sb.AppendLine(QueryIterator("Entity", "Entities", "Entity"));
        for (var i = 0; i < 16; i++)
        {
            var typeParams = string.Join(", ", Enumerable.Range(0, i + 1).Select(n => $"T{n}"));
            sb.AppendLine(QueryIterator("Entity", "Entities", "Entity", $"<{typeParams}>"));
        }

        sb.EndRegion();
    }

    private static void Components(StringBuilder sb)
    {
        sb.BeginRegion("Components");
        for (var i = 0; i < 16; i++)
        {
            var typeParams = string.Join(", ", Enumerable.Range(0, i + 1).Select(n => $"T{n}"));
            var type =
                i == 0 ? "T0" : QueryHelper.NamedTuple(false, Enumerable.Range(0, i + 1).Select(n => $"T{n}").ToList());
            sb.AppendLine(QueryIterator("Component", "Components", type, $"<{typeParams}>"));
        }

        sb.EndRegion();
    }

    private static void Entries(StringBuilder sb)
    {
        sb.BeginRegion("Entries");
        for (var i = 0; i < 15; i++)
        {
            var typeParams = string.Join(", ", Enumerable.Range(0, i + 1).Select(n => $"T{n}"));
            var type = QueryHelper.NamedTuple(true, Enumerable.Range(0, i + 1).Select(n => $"T{n}").ToList());
            sb.AppendLine(QueryIterator("Entry", "Entries", type, $"<{typeParams}>"));
        }

        sb.EndRegion();
    }

    private static void RefComponents(StringBuilder sb)
    {
        sb.BeginRegion("RefComponents");
        for (var i = 0; i < 16; i++)
        {
            var typeParams = string.Join(", ", Enumerable.Range(0, i + 1).Select(n => $"T{n}"));
            sb.AppendLine(RefQueryIterator("RefComponent", "RefComponents", $"<{typeParams}>"));
        }

        sb.EndRegion();
    }

    private static void RefEntries(StringBuilder sb)
    {
        sb.BeginRegion("RefEntries");
        for (var i = 0; i < 15; i++)
        {
            var typeParams = string.Join(", ", Enumerable.Range(0, i + 1).Select(n => $"T{n}"));
            sb.AppendLine(RefQueryIterator("RefEntry", "RefEntries", $"<{typeParams}>"));
        }

        sb.EndRegion();
    }

    private static void AssignableEntities(StringBuilder sb)
    {
        sb.BeginRegion("AssignableEntities");
        sb.AppendLine(QueryIterator("AssignableEntity", "AssignableEntities", "Entity", "<T0>", hasHidden: true));
        sb.EndRegion();
    }

    private static void AssignableComponents(StringBuilder sb)
    {
        sb.BeginRegion("AssignableComponents");
        sb.AppendLine(QueryIterator("AssignableComponent", "AssignableComponents", "T0", "<T0>", hasHidden: true));
        sb.EndRegion();
    }

    private static void AssignableEntries(StringBuilder sb)
    {
        sb.BeginRegion("AssignableEntries");
        sb.AppendLine(
            QueryIterator(
                "AssignableEntries",
                "AssignableEntries",
                "(Entity Entity, T0 Component)",
                "<T0>",
                hasHidden: true
            )
        );
        sb.EndRegion();
    }

    private static void TableEntities(StringBuilder sb)
    {
        sb.BeginRegion("TableEntities");
        for (var i = 0; i < 16; i++)
        {
            var methodParams = string.Join(", ", Enumerable.Range(0, i + 1).Select(n => $"Table table{n}"));
            var methodArgs = string.Join(", ", Enumerable.Range(0, i + 1).Select(n => $"table{n}"));
            sb.AppendLine(QueryIterator($"TableEntity{i + 1}", "Entities", "Entity", "", methodParams, methodArgs));
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
            var type =
                i == 0
                    ? "object"
                    : QueryHelper.NamedTuple(false, Enumerable.Range(0, i + 1).Select(_ => "object").ToList());
            sb.AppendLine(QueryIterator($"TableComponent{i + 1}", "Components", type, "", methodParams, methodArgs));
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
            var type = QueryHelper.NamedTuple(true, Enumerable.Range(0, i + 1).Select(_ => "object").ToList());
            sb.AppendLine(QueryIterator($"TableEntry{i + 1}", "Entries", type, "", methodParams, methodArgs));
        }

        sb.EndRegion();
    }

    private static string QueryIterator(
        string name,
        string methodName,
        string type,
        string typeParams = "",
        string methodParams = "",
        string methodArgs = "",
        bool hasHidden = false
    )
    {
        string allParams;
        {
            var paramParts = new List<string>();
            if (methodParams != "")
                paramParts.Add(methodParams);
            paramParts.Add("bool? withDisabled = null");
            if (hasHidden)
                paramParts.Add("bool withHidden = false");
            paramParts.Add("bool? deferred = null");
            allParams = string.Join(", ", paramParts);
        }

        string forwardArgs;
        {
            var argParts = new List<string>();
            if (methodArgs != "")
                argParts.Add(methodArgs);
            argParts.Add("withDisabled: withDisabled ?? QueryWithDisabled");
            if (hasHidden)
                argParts.Add("withHidden: withHidden");
            argParts.Add("deferred: deferred ?? QueryDeferred");
            forwardArgs = string.Join(", ", argParts);
        }

        return $$"""
                public ZLinq.ValueEnumerable<Scene.{{name}}Enumerator{{typeParams}}, {{type}}> {{methodName}}{{typeParams}}({{allParams}}) {
                    return Scene.{{methodName}}{{typeParams}}({{forwardArgs}});
                }

            """;
    }

    private static string RefQueryIterator(string name, string methodName, string typeParams)
    {
        return $$"""
                public Scene.{{name}}Enumerable{{typeParams}} {{methodName}}{{typeParams}}(bool? withDisabled = null, bool? deferred = null) {
                    return Scene.{{methodName}}{{typeParams}}().WithDisabled(withDisabled ?? QueryWithDisabled).Deferred(deferred ?? QueryDeferred);
                }

            """;
    }
}
