using System.Text;
using Microsoft.CodeAnalysis;
using Vigilance.Codegen.Helpers;

namespace Vigilance.Codegen.Generators;

[Generator]
public sealed class UIElementGenerator : SourceGenerator
{
    protected override void Generate(StringBuilder sb)
    {
        sb.AppendLine(
            """
            namespace Vigilance.UI;

            public static partial class UIElementExtensions
            {

            """
        );
        sb.TraverserExtensions("UIElement", "UIElement.Traverser");
        sb.AppendLine("}");
    }
}
