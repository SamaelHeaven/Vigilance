using System.Text;
using Microsoft.CodeAnalysis;

namespace Vigilance.Codegen.Generators;

[Generator]
public sealed class SafeInvokeGenerator : SourceGenerator
{
    protected override void Generate(StringBuilder sb)
    {
        sb.AppendLine(
            """
            using Vigilance.Logging;

            namespace Vigilance.Core;

            public static class DelegateExtensions
            {
            """
        );
        for (var i = 0; i <= 16; i++)
            Action(sb, i);
        for (var i = 0; i <= 16; i++)
            Func(sb, i);
        sb.AppendLine("}");
    }

    private static void Action(StringBuilder sb, int arity)
    {
        var inputs = Enumerable.Range(0, arity).Select(n => $"T{n}").ToArray();
        var generics = arity == 0 ? "" : $"<{string.Join(", ", inputs)}>";
        var delegateType = arity == 0 ? "Action" : $"Action{generics}";
        var parameters = string.Join(", ", Enumerable.Range(0, arity).Select(n => $"in T{n} arg{n}"));
        var args = string.Join(", ", Enumerable.Range(0, arity).Select(n => $"arg{n}"));
        var signatureTail = parameters == "" ? "" : $", {parameters}";
        sb.AppendLine();
        sb.AppendLine($"    public static void SafeInvoke{generics}(this {delegateType} action{signatureTail})");
        foreach (var t in inputs)
            sb.AppendLine($"        where {t} : allows ref struct");
        sb.AppendLine(
            $$"""
                {
                    if (action.HasSingleTarget)
                    {
                        try
                        {
                            action.Invoke({{args}});
                        }
                        catch (System.Exception e)
                        {
                            Log.Error(e);
                        }
                        return;
                    }
                    foreach (var handler in Delegate.EnumerateInvocationList(action))
                        try
                        {
                            handler.Invoke({{args}});
                        }
                        catch (System.Exception e)
                        {
                            Log.Error(e);
                        }
                }
            """
        );
    }

    private static void Func(StringBuilder sb, int arity)
    {
        var inputs = Enumerable.Range(0, arity).Select(n => $"T{n}").ToArray();
        var typeParams = inputs.Append("TResult").ToArray();
        var generics = $"<{string.Join(", ", typeParams)}>";
        var delegateType = $"Func{generics}";
        var parameters = string.Join(", ", Enumerable.Range(0, arity).Select(n => $"in T{n} arg{n}"));
        var args = string.Join(", ", Enumerable.Range(0, arity).Select(n => $"arg{n}"));
        var signatureTail = parameters == "" ? "" : $", {parameters}";
        sb.AppendLine();
        sb.AppendLine($"    public static TResult SafeInvoke{generics}(this {delegateType} func{signatureTail})");
        foreach (var t in typeParams)
            sb.AppendLine($"        where {t} : allows ref struct");
        sb.AppendLine(
            $$"""
                {
                    if (func.HasSingleTarget)
                    {
                        try
                        {
                            return func.Invoke({{args}});
                        }
                        catch (System.Exception e)
                        {
                            Log.Error(e);
                        }
                        return default!;
                    }
                    TResult result = default!;
                    foreach (var handler in Delegate.EnumerateInvocationList(func))
                        try
                        {
                            result = handler.Invoke({{args}});
                        }
                        catch (System.Exception e)
                        {
                            Log.Error(e);
                        }
                    return result;
                }
            """
        );
    }
}
