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
    }
}
