using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Vigilance.Codegen.Generators;

public abstract class SourceGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context) { }

    public void Execute(GeneratorExecutionContext context)
    {
        var sb = new StringBuilder();
        sb.Append(
            """
            #nullable enable


            """
        );
        Generate(sb);
        sb.Append(
            """

            #nullable restore
            """
        );
        var name = GetType().Name;
        const string suffix = "Generator";
        context.AddSource(
            $"{(name.EndsWith(suffix) ? name.Substring(0, name.Length - suffix.Length) : name)}.g.cs",
            SourceText.From(sb.ToString(), Encoding.UTF8)
        );
    }

    protected abstract void Generate(StringBuilder sb);
}
