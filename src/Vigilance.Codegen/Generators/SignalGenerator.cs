using System.Text;
using Microsoft.CodeAnalysis;

namespace Vigilance.Codegen.Generators;

[Generator]
public sealed class SignalGenerator : SourceGenerator
{
    protected override void Generate(StringBuilder sb)
    {
        sb.AppendLine(
            """
            namespace Vigilance.Core;

            """
        );

        Signal(sb, "", "", "");
        for (var i = 0; i < 15; i++)
        {
            var typeParams = string.Join(", ", Enumerable.Range(0, i + 1).Select(n => $"T{n}"));
            var invokeParams = string.Join(", ", Enumerable.Range(0, i + 1).Select(n => $"in T{n} t{n}"));
            var invokeArgs = string.Join(", ", Enumerable.Range(0, i + 1).Select(n => $"t{n}"));
            Signal(sb, typeParams, invokeParams, invokeArgs);
        }
    }

    private static void Signal(StringBuilder sb, string typeParams, string invokeParams, string invokeArgs)
    {
        var funcTypeParams = typeParams == "" ? "" : $"{typeParams}, ";
        sb.AppendLine(
            $$"""
            public readonly ref struct Signal{{(
                typeParams == "" ? "" : $"<{typeParams}>"
            )}}(ref Func<{{funcTypeParams}}bool>? handlers)
            {
                private readonly ref Func<{{funcTypeParams}}bool>? _handlers = ref handlers;
                
                public ref Func<{{funcTypeParams}}bool> Handlers => ref _handlers!;
                
                public Func<{{funcTypeParams}}bool> Subscribe(Func<{{funcTypeParams}}bool> handler)
                {
                    _handlers += handler;
                    return handler;
                }

                public Func<{{funcTypeParams}}bool> Subscribe(Action{{(
                    typeParams == "" ? "" : $"<{typeParams}>"
                )}} action)
                {
                    Func<{{funcTypeParams}}bool> handler = ({{invokeArgs}}) =>
                    {
                        action.Invoke({{invokeArgs}});
                        return false;
                    };
                    _handlers += handler;
                    return handler;
                }

                public void Unsubscribe(Func<{{funcTypeParams}}bool> handler)
                {
                    _handlers -= handler;
                }
                
                public bool Invoke({{invokeParams}})
                {
                    return Invoke(_handlers{{(invokeArgs == "" ? "" : $", {invokeArgs}")}});
                }

                public static bool Invoke(Func<{{funcTypeParams}}bool>? handlers{{(
                    invokeParams == "" ? "" : $", {invokeParams}"
                )}})
                {
                    foreach (var handler in Delegate.EnumerateInvocationList(handlers))
                        if (handler.Invoke({{invokeArgs}}))
                            return true;
                    return false;
                }
            }

            """
        );
    }
}
