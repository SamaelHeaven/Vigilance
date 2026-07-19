using System.Text;
using Microsoft.CodeAnalysis;

namespace Vigilance.Codegen.Generators;

[Generator]
public sealed class RefTupleGenerator : SourceGenerator
{
    protected override void Generate(StringBuilder sb)
    {
        sb.AppendLine(
            """
            namespace Vigilance.Core;

            """
        );

        for (var i = 0; i < 16; i++)
        {
            var typeParams = string.Join(", ", Enumerable.Range(0, i + 1).Select(n => $"T{n}"));
            var ctorParams = string.Join(", ", Enumerable.Range(0, i + 1).Select(n => $"T{n} item{n + 1}"));
            var wheres = string.Join(" ", Enumerable.Range(0, i + 1).Select(n => $"where T{n} : allows ref struct"));
            var properties = string.Join(
                "\n",
                Enumerable.Range(0, i + 1).Select(n => $"    public T{n} Item{n + 1} = item{n + 1};")
            );
            var outParams = string.Join(", ", Enumerable.Range(0, i + 1).Select(n => $"out T{n} item{n + 1}"));
            var assignments = string.Join(
                "\n",
                Enumerable.Range(0, i + 1).Select(n => $"        item{n + 1} = Item{n + 1};")
            );
            sb.AppendLine(
                $$"""
                public ref struct RefTuple<{{typeParams}}>({{ctorParams}}) {{wheres}}
                {
                {{properties}}

                    public void Deconstruct({{outParams}})
                    {
                {{assignments}}
                    }
                }

                """
            );
        }

        for (var componentCount = 2; componentCount <= 16; componentCount++)
            SpecializedTuple(sb, "ComponentTuple", false, componentCount);
        for (var componentCount = 1; componentCount <= 15; componentCount++)
            SpecializedTuple(sb, "EntryTuple", true, componentCount);
    }

    private static void SpecializedTuple(StringBuilder sb, string name, bool hasEntity, int componentCount)
    {
        var typeParams = string.Join(", ", Enumerable.Range(0, componentCount).Select(n => $"T{n}"));
        var memberNames = new List<string>();
        var memberTypes = new List<string>();
        var paramNames = new List<string>();
        if (hasEntity)
        {
            memberNames.Add("Entity");
            memberTypes.Add("Entity");
            paramNames.Add("entity");
        }

        for (var n = 0; n < componentCount; n++)
        {
            var suffix = componentCount == 1 ? "" : (n + 1).ToString();
            memberNames.Add($"Component{suffix}");
            memberTypes.Add($"ComponentRef<T{n}>");
            paramNames.Add($"component{suffix}");
        }

        var count = memberNames.Count;
        var ctorParams = string.Join(", ", Enumerable.Range(0, count).Select(k => $"{memberTypes[k]} {paramNames[k]}"));
        var properties = string.Join(
            "\n",
            Enumerable.Range(0, count).Select(k => $"    public {memberTypes[k]} {memberNames[k]} = {paramNames[k]};")
        );
        var outParams = string.Join(
            ", ",
            Enumerable.Range(0, count).Select(k => $"out {memberTypes[k]} {paramNames[k]}")
        );
        var assignments = string.Join(
            "\n",
            Enumerable.Range(0, count).Select(k => $"        {paramNames[k]} = {memberNames[k]};")
        );
        var refTupleArgs = string.Join(", ", memberTypes);
        var refTupleCtorArgs = string.Join(", ", memberNames.Select(m => $"tuple.{m}"));

        sb.AppendLine(
            $$"""
            public ref struct {{name}}<{{typeParams}}>({{ctorParams}})
            {
            {{properties}}

                public void Deconstruct({{outParams}})
                {
            {{assignments}}
                }

                public static implicit operator RefTuple<{{refTupleArgs}}>({{name}}<{{typeParams}}> tuple)
                {
                    return new RefTuple<{{refTupleArgs}}>({{refTupleCtorArgs}});
                }
            }

            """
        );
    }
}
