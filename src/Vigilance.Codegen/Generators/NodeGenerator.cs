using System.Text;
using Microsoft.CodeAnalysis;
using Vigilance.Codegen.Helpers;

namespace Vigilance.Codegen.Generators;

[Generator]
public sealed class NodeGenerator : SourceGenerator
{
    protected override void Generate(StringBuilder sb)
    {
        sb.AppendLine(
            """
            namespace Vigilance.FlexLayout;

            public static class NodeExtensions
            {

            """
        );
        sb.TraverserExtensions(
            "Node<TStorage>",
            "Node<TStorage>.Traverser",
            typeParams: "<TStorage>",
            typeConstraints: "where TStorage : System.Collections.Generic.IList<Node<TStorage>>"
        );
        sb.AppendLine("}");
    }
}
