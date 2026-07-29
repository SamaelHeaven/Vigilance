using System.Collections.Immutable;
using System.Globalization;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Vigilance.Codegen.Generators;

[Generator]
public sealed class ValueWrapperGenerator : IIncrementalGenerator
{
    private const string AttributeMetadataName = "Vigilance.Core.ValueWrapperAttribute`1";

    private const string AttributeSource = """
        #nullable enable

        namespace Vigilance.Core
        {
            [System.AttributeUsage(
                System.AttributeTargets.Class | System.AttributeTargets.Struct,
                AllowMultiple = false,
                Inherited = false
            )]
            internal sealed class ValueWrapperAttribute<TValue> : System.Attribute;
        }

        #nullable restore
        """;

    private static readonly SymbolDisplayFormat _typeFormat = new(
        SymbolDisplayGlobalNamespaceStyle.Included,
        SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
        SymbolDisplayGenericsOptions.IncludeTypeParameters,
        miscellaneousOptions: SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers
            | SymbolDisplayMiscellaneousOptions.UseSpecialTypes
            | SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier
    );

    private static readonly SymbolDisplayFormat _keyFormat = new(
        SymbolDisplayGlobalNamespaceStyle.Included,
        SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
        SymbolDisplayGenericsOptions.IncludeTypeParameters,
        miscellaneousOptions: SymbolDisplayMiscellaneousOptions.UseSpecialTypes
    );

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(ctx =>
            ctx.AddSource("ValueWrapperAttribute.g.cs", SourceText.From(AttributeSource, Encoding.UTF8))
        );
        var wrappers = context
            .SyntaxProvider.ForAttributeWithMetadataName(
                AttributeMetadataName,
                static (node, _) => node is TypeDeclarationSyntax,
                static (ctx, _) => Build(ctx)
            )
            .Where(static result => result is not null);
        context.RegisterSourceOutput(
            wrappers,
            static (spc, result) =>
                spc.AddSource(result!.Value.HintName, SourceText.From(result.Value.Source, Encoding.UTF8))
        );
    }

    private static (string HintName, string Source)? Build(GeneratorAttributeSyntaxContext context)
    {
        if (context.TargetSymbol is not INamedTypeSymbol wrapper)
            return null;
        if (
            context.TargetNode is not TypeDeclarationSyntax targetSyntax
            || !targetSyntax.Modifiers.Any(SyntaxKind.PartialKeyword)
        )
            return null;
        if (context.Attributes.Length == 0)
            return null;
        if (context.Attributes[0].AttributeClass is not { TypeArguments.Length: 1 } attributeClass)
            return null;
        if (attributeClass.TypeArguments[0] is not INamedTypeSymbol wrapped || wrapped.TypeKind == TypeKind.Error)
            return null;
        var sb = new StringBuilder();
        sb.AppendLine("#nullable enable");
        sb.AppendLine();
        var indent = "";
        var closers = new Stack<string>();
        if (wrapper.ContainingNamespace is { IsGlobalNamespace: false } ns)
        {
            sb.AppendLine($"namespace {ns.ToDisplayString()}");
            sb.AppendLine("{");
            closers.Push("}");
            indent = "    ";
        }

        var nesting = new Stack<INamedTypeSymbol>();
        for (var c = wrapper.ContainingType; c is not null; c = c.ContainingType)
            nesting.Push(c);
        foreach (var enclosing in nesting)
        {
            sb.AppendLine($"{indent}partial {TypeHeader(enclosing)}");
            sb.AppendLine($"{indent}{{");
            closers.Push($"{indent}}}");
            indent += "    ";
        }

        sb.AppendLine($"{indent}partial {TypeHeader(wrapper)}");
        sb.AppendLine($"{indent}{{");
        var body = indent + "    ";
        EmitBody(sb, body, wrapper, wrapped);
        sb.AppendLine($"{indent}}}");
        while (closers.Count > 0)
            sb.AppendLine(closers.Pop());
        sb.AppendLine();
        sb.AppendLine("#nullable restore");
        var hint = SanitizeHint(wrapper) + ".ValueWrapper.g.cs";
        return (hint, sb.ToString());
    }

    private static void EmitBody(StringBuilder sb, string indent, INamedTypeSymbol wrapper, INamedTypeSymbol wrapped)
    {
        var valueNames = new HashSet<string>();
        var methodNames = new HashSet<string>();
        var methodKeys = new HashSet<string>();
        var ctorKeys = new HashSet<string>();
        var indexerKeys = new HashSet<string>();
        var hasImplicitToWrapped = false;
        foreach (var member in wrapper.GetMembers())
            switch (member)
            {
                case IMethodSymbol { MethodKind: MethodKind.Constructor, IsImplicitlyDeclared: false } ctor:
                    ctorKeys.Add(ParamsKey(ctor.Parameters));
                    break;
                case IMethodSymbol { MethodKind: MethodKind.Conversion, Name: "op_Implicit" } conv
                    when SymbolEqualityComparer.Default.Equals(conv.ReturnType, wrapped):
                    hasImplicitToWrapped = true;
                    break;
                case IMethodSymbol { MethodKind: MethodKind.Ordinary } method:
                    methodNames.Add(method.Name);
                    methodKeys.Add(MethodKey(method));
                    break;
                case IPropertySymbol { IsIndexer: true } indexer:
                    indexerKeys.Add(ParamsKey(indexer.Parameters));
                    break;
                case IPropertySymbol property:
                    valueNames.Add(property.Name);
                    break;
                case IFieldSymbol field:
                    valueNames.Add(field.Name);
                    break;
            }

        var wrappedType = wrapped.ToDisplayString(_typeFormat);
        if (valueNames.Add("Value") && !methodNames.Contains("Value"))
            sb.AppendLine($"{indent}public {wrappedType} Value;");
        else
            valueNames.Add("Value");
        var ctorRegion = new StringBuilder();
        if (ctorKeys.Add(wrapped.ToDisplayString(_keyFormat)))
        {
            ctorRegion.AppendLine();
            ctorRegion.AppendLine($"{indent}public {wrapper.Name}({wrappedType} value)");
            ctorRegion.AppendLine($"{indent}{{");
            ctorRegion.AppendLine($"{indent}    Value = value;");
            ctorRegion.AppendLine($"{indent}}}");
        }

        foreach (var ctor in wrapped.InstanceConstructors)
        {
            if (ctor.DeclaredAccessibility != Accessibility.Public)
                continue;
            if (ContainsPointer(ctor.Parameters) || ContainsPointer(ctor.ReturnType))
                continue;
            var key = ParamsKey(ctor.Parameters);
            if (!ctorKeys.Add(key))
                continue;

            ctorRegion.AppendLine();
            ctorRegion.AppendLine($"{indent}public {wrapper.Name}({ParamsDecl(ctor.Parameters)})");
            ctorRegion.AppendLine($"{indent}{{");
            ctorRegion.AppendLine($"{indent}    Value = new {wrappedType}({ArgsCall(ctor.Parameters)});");
            ctorRegion.AppendLine($"{indent}}}");
        }

        if (ctorRegion.Length > 0)
            sb.Append(ctorRegion);
        var memberRegion = new StringBuilder();
        for (var t = wrapped; t is not null && !IsWalkBoundary(t); t = t.BaseType)
            foreach (var member in t.GetMembers())
            {
                if (member.IsStatic || member.DeclaredAccessibility != Accessibility.Public)
                    continue;
                switch (member)
                {
                    case IMethodSymbol { MethodKind: MethodKind.Ordinary } method:
                        if (!SyntaxFacts.IsValidIdentifier(method.Name))
                            continue;
                        if (OverridesObjectMethod(method))
                            continue;
                        if (ContainsPointer(method.Parameters) || ContainsPointer(method.ReturnType))
                            continue;
                        if (valueNames.Contains(method.Name))
                            continue;
                        if (!methodKeys.Add(MethodKey(method)))
                            continue;
                        methodNames.Add(method.Name);
                        EmitMethod(memberRegion, indent, method);
                        break;
                    case IPropertySymbol { IsIndexer: true } indexer:
                        if (ContainsPointer(indexer.Parameters) || ContainsPointer(indexer.Type))
                            continue;
                        if (!indexerKeys.Add(ParamsKey(indexer.Parameters)))
                            continue;
                        EmitIndexer(memberRegion, indent, indexer);
                        break;
                    case IPropertySymbol property:
                        if (ContainsPointer(property.Type))
                            continue;
                        if (!SyntaxFacts.IsValidIdentifier(property.Name))
                            continue;
                        if (methodNames.Contains(property.Name) || !valueNames.Add(property.Name))
                            continue;
                        EmitProperty(memberRegion, indent, property);
                        break;
                }
            }

        if (memberRegion.Length > 0)
            sb.Append(memberRegion);
        if (hasImplicitToWrapped)
            return;
        sb.AppendLine();
        sb.AppendLine(
            $"{indent}public static implicit operator {wrappedType}({wrapper.Name}{TypeParamList(wrapper.TypeParameters)} wrapper)"
        );
        sb.AppendLine($"{indent}{{");
        sb.AppendLine($"{indent}    return wrapper.Value;");
        sb.AppendLine($"{indent}}}");
    }

    private static void EmitMethod(StringBuilder sb, string indent, IMethodSymbol method)
    {
        var typeParams = TypeParamList(method.TypeParameters);
        var returnType = method.ReturnsVoid
            ? "void"
            : ReturnPrefix(method) + method.ReturnType.ToDisplayString(_typeFormat);
        var constraints = Constraints(method.TypeParameters);
        sb.AppendLine();
        sb.AppendLine(
            $"{indent}public {returnType} {method.Name}{typeParams}({ParamsDecl(method.Parameters)}){constraints}"
        );
        sb.AppendLine($"{indent}{{");
        var call = $"Value.{method.Name}{typeParams}({ArgsCall(method.Parameters)})";
        if (method.ReturnsVoid)
            sb.AppendLine($"{indent}    {call};");
        else if (method.ReturnsByRef || method.ReturnsByRefReadonly)
            sb.AppendLine($"{indent}    return ref {call};");
        else
            sb.AppendLine($"{indent}    return {call};");
        sb.AppendLine($"{indent}}}");
    }

    private static void EmitProperty(StringBuilder sb, string indent, IPropertySymbol property)
    {
        var type = ReturnPrefix(property) + property.Type.ToDisplayString(_typeFormat);
        var byRef = property.ReturnsByRef || property.ReturnsByRefReadonly;
        var hasGet = property.GetMethod is { DeclaredAccessibility: Accessibility.Public };
        var hasSet = !byRef && property.SetMethod is { DeclaredAccessibility: Accessibility.Public, IsInitOnly: false };
        sb.AppendLine();
        if (hasGet && !hasSet)
        {
            var expr = byRef ? $"ref Value.{property.Name}" : $"Value.{property.Name}";
            sb.AppendLine($"{indent}public {type} {property.Name} => {expr};");
            return;
        }

        sb.AppendLine($"{indent}public {type} {property.Name}");
        sb.AppendLine($"{indent}{{");
        if (hasGet)
            sb.AppendLine($"{indent}    get => Value.{property.Name};");
        if (hasSet)
        {
            var setter = property.SetMethod!.IsInitOnly ? "init" : "set";
            sb.AppendLine($"{indent}    {setter} => Value.{property.Name} = value;");
        }

        sb.AppendLine($"{indent}}}");
    }

    private static void EmitIndexer(StringBuilder sb, string indent, IPropertySymbol indexer)
    {
        var type = ReturnPrefix(indexer) + indexer.Type.ToDisplayString(_typeFormat);
        var byRef = indexer.ReturnsByRef || indexer.ReturnsByRefReadonly;
        var hasGet = indexer.GetMethod is { DeclaredAccessibility: Accessibility.Public };
        var hasSet = !byRef && indexer.SetMethod is { DeclaredAccessibility: Accessibility.Public };
        var args = ArgsCall(indexer.Parameters);
        sb.AppendLine();
        if (hasGet && !hasSet)
        {
            var expr = byRef ? $"ref Value[{args}]" : $"Value[{args}]";
            sb.AppendLine($"{indent}public {type} this[{ParamsDecl(indexer.Parameters)}] => {expr};");
            return;
        }

        sb.AppendLine($"{indent}public {type} this[{ParamsDecl(indexer.Parameters)}]");
        sb.AppendLine($"{indent}{{");
        if (hasGet)
            sb.AppendLine($"{indent}    get => Value[{args}];");
        if (hasSet)
        {
            var setter = indexer.SetMethod!.IsInitOnly ? "init" : "set";
            sb.AppendLine($"{indent}    {setter} => Value[{args}] = value;");
        }

        sb.AppendLine($"{indent}}}");
    }

    private static string TypeHeader(INamedTypeSymbol type)
    {
        var sb = new StringBuilder();
        if (type.IsReadOnly)
            sb.Append("readonly ");
        if (type.IsRefLikeType)
            sb.Append("ref ");
        if (type.IsRecord)
            sb.Append(type.IsValueType ? "record struct " : "record ");
        else
            sb.Append(type.IsValueType ? "struct " : "class ");
        sb.Append(type.Name);
        sb.Append(TypeParamList(type.TypeParameters));
        return sb.ToString();
    }

    private static string TypeParamList(ImmutableArray<ITypeParameterSymbol> typeParameters)
    {
        return typeParameters.Length == 0 ? "" : "<" + string.Join(", ", typeParameters.Select(tp => tp.Name)) + ">";
    }

    private static string Constraints(ImmutableArray<ITypeParameterSymbol> typeParameters)
    {
        var clauses = new List<string>();
        foreach (var tp in typeParameters)
        {
            var parts = new List<string>();
            if (tp.HasReferenceTypeConstraint)
                parts.Add(
                    tp.ReferenceTypeConstraintNullableAnnotation == NullableAnnotation.Annotated ? "class?" : "class"
                );
            else if (tp.HasUnmanagedTypeConstraint)
                parts.Add("unmanaged");
            else if (tp.HasValueTypeConstraint)
                parts.Add("struct");
            if (tp.HasNotNullConstraint)
                parts.Add("notnull");
            parts.AddRange(tp.ConstraintTypes.Select(constraint => constraint.ToDisplayString(_typeFormat)));
            if (tp.HasConstructorConstraint)
                parts.Add("new()");
            if (tp.AllowsRefLikeType)
                parts.Add("allows ref struct");
            if (parts.Count > 0)
                clauses.Add($"where {tp.Name} : {string.Join(", ", parts)}");
        }

        return clauses.Count == 0 ? "" : " " + string.Join(" ", clauses);
    }

    private static string ParamsDecl(ImmutableArray<IParameterSymbol> parameters)
    {
        return string.Join(", ", parameters.Select(ParamDecl));
    }

    private static string ParamDecl(IParameterSymbol p)
    {
        var sb = new StringBuilder();
        if (p.IsParams)
            sb.Append("params ");
        sb.Append(RefPrefix(p.RefKind));
        sb.Append(p.Type.ToDisplayString(_typeFormat));
        sb.Append(' ');
        sb.Append(Escape(p.Name));
        if (!p.HasExplicitDefaultValue)
            return sb.ToString();
        sb.Append(" = ");
        sb.Append(FormatDefault(p));
        return sb.ToString();
    }

    private static string ArgsCall(ImmutableArray<IParameterSymbol> parameters)
    {
        return string.Join(", ", parameters.Select(p => ArgRefPrefix(p.RefKind) + Escape(p.Name)));
    }

    private static string RefPrefix(RefKind kind)
    {
        return kind switch
        {
            RefKind.Ref => "ref ",
            RefKind.Out => "out ",
            RefKind.In => "in ",
            RefKind.RefReadOnlyParameter => "ref readonly ",
            _ => "",
        };
    }

    private static string ArgRefPrefix(RefKind kind)
    {
        return kind switch
        {
            RefKind.Ref => "ref ",
            RefKind.Out => "out ",
            RefKind.In => "in ",
            RefKind.RefReadOnlyParameter => "in ",
            _ => "",
        };
    }

    private static string ReturnPrefix(IMethodSymbol method)
    {
        if (method.ReturnsByRefReadonly)
            return "ref readonly ";
        return method.ReturnsByRef ? "ref " : "";
    }

    private static string ReturnPrefix(IPropertySymbol property)
    {
        if (property.ReturnsByRefReadonly)
            return "ref readonly ";
        return property.ReturnsByRef ? "ref " : "";
    }

    private static bool IsWalkBoundary(ITypeSymbol type)
    {
        return type.SpecialType is SpecialType.System_Object or SpecialType.System_ValueType or SpecialType.System_Enum;
    }

    private static bool OverridesObjectMethod(IMethodSymbol method)
    {
        var current = method;
        while (current.OverriddenMethod is { } overridden)
            current = overridden;
        return current.ContainingType?.SpecialType == SpecialType.System_Object;
    }

    private static bool ContainsPointer(ImmutableArray<IParameterSymbol> parameters)
    {
        return parameters.Any(p => ContainsPointer(p.Type));
    }

    private static bool ContainsPointer(ITypeSymbol type)
    {
        return type.TypeKind is TypeKind.Pointer or TypeKind.FunctionPointer;
    }

    private static string MethodKey(IMethodSymbol method)
    {
        return $"{method.Name}`{method.TypeParameters.Length}({ParamsKey(method.Parameters)})";
    }

    private static string ParamsKey(ImmutableArray<IParameterSymbol> parameters)
    {
        return string.Join(",", parameters.Select(p => RefPrefix(p.RefKind) + p.Type.ToDisplayString(_keyFormat)));
    }

    private static string FormatDefault(IParameterSymbol p)
    {
        var value = p.ExplicitDefaultValue;
        if (value is null)
            return "default";
        var type = p.Type;
        if (type is INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Nullable_T } named)
            type = named.TypeArguments[0];
        if (type.TypeKind == TypeKind.Enum)
            return $"({type.ToDisplayString(_typeFormat)}){Convert.ToString(value, CultureInfo.InvariantCulture)}";
        return value switch
        {
            bool b => b ? "true" : "false",
            string s => SymbolDisplay.FormatLiteral(s, true),
            char c => SymbolDisplay.FormatLiteral(c, true),
            float f => f.ToString("R", CultureInfo.InvariantCulture) + "F",
            double d => d.ToString("R", CultureInfo.InvariantCulture) + "D",
            decimal m => m.ToString(CultureInfo.InvariantCulture) + "M",
            _ => Convert.ToString(value, CultureInfo.InvariantCulture) ?? "default",
        };
    }

    private static string Escape(string name)
    {
        return
            SyntaxFacts.GetKeywordKind(name) != SyntaxKind.None
            || SyntaxFacts.GetContextualKeywordKind(name) != SyntaxKind.None
            ? "@" + name
            : name;
    }

    private static string SanitizeHint(INamedTypeSymbol wrapper)
    {
        var sb = new StringBuilder();
        foreach (var ch in wrapper.ToDisplayString())
            sb.Append(char.IsLetterOrDigit(ch) ? ch : '_');
        return sb.ToString();
    }
}
