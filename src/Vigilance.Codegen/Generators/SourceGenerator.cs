using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Vigilance.Codegen.Generators;

public abstract class SourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var compilationProvider = context.CompilationProvider.Select(static (compilation, _) => IsEngine(compilation));
        context.RegisterSourceOutput(
            compilationProvider,
            (spc, isEngine) =>
            {
                if (!isEngine)
                    return;
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
                spc.AddSource(
                    $"{(name.EndsWith(suffix) ? name.Substring(0, name.Length - suffix.Length) : name)}.g.cs",
                    SourceText.From(sb.ToString(), Encoding.UTF8)
                );
            }
        );
    }

    protected abstract void Generate(StringBuilder sb);

    private static bool IsEngine(Compilation compilation)
    {
        return compilation.Assembly.GetTypeByMetadataName("Vigilance.Core.Game") is not null;
    }
}
