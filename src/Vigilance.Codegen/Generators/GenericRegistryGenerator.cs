using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Vigilance.Codegen.Generators;

[Generator]
public sealed class GenericRegistryGenerator : IIncrementalGenerator
{
    private const string AttributeMetadataName = "Vigilance.Core.GenericRegistryAttribute";

    private const string ModuleInitializerMetadataName = "System.Runtime.CompilerServices.ModuleInitializerAttribute";

    private const string LogTypeName = "global::Vigilance.Logging.Log";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterSourceOutput(
            context.CompilationProvider,
            static (spc, compilation) => Execute(spc, compilation)
        );
    }

    private static void Execute(SourceProductionContext context, Compilation compilation)
    {
        if (compilation.GetTypeByMetadataName(AttributeMetadataName) is not { } attribute)
            return;
        var declaring = attribute.ContainingAssembly;
        var types = new List<INamedTypeSymbol>();
        Collect(compilation.Assembly.GlobalNamespace, types);
        CollectConstructed(compilation, types);
        var referencedTypes = new List<INamedTypeSymbol>();
        var registries = new List<IMethodSymbol>();
        foreach (var type in types)
        foreach (var method in Registered(type))
            if (Validate(context, compilation, method, true) is { } registry)
                registries.Add(registry);
        foreach (var reference in compilation.SourceModule.ReferencedAssemblySymbols)
        {
            if (!Sees(reference, declaring))
                continue;
            var referenced = new List<INamedTypeSymbol>();
            Collect(reference.GlobalNamespace, referenced);
            referencedTypes.AddRange(referenced);
            foreach (var type in referenced)
            foreach (var method in Registered(type))
            {
                if (!compilation.IsSymbolAccessibleWithin(method, compilation.Assembly))
                    continue;
                if (Validate(context, compilation, method, false) is { } registry)
                    registries.Add(registry);
            }
        }

        if (registries.Count == 0)
            return;
        registries.Sort(
            static (left, right) =>
                string.CompareOrdinal(
                    left.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                    right.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
                )
        );
        types.Sort(
            static (left, right) =>
                string.CompareOrdinal(
                    left.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                    right.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
                )
        );
        ReportInaccessible(context, compilation, registries, types);
        var declared = Combine(types, referencedTypes);
        var hints = new HashSet<string>();
        foreach (var method in registries)
        {
            var hint = Hint(method, hints);
            var candidates = SymbolEqualityComparer.Default.Equals(method.ContainingAssembly, compilation.Assembly)
                ? declared
                : types;
            context.AddSource(
                $"{hint}.GenericRegistry.g.cs",
                SourceText.From(Build(compilation, method, candidates, hint), Encoding.UTF8)
            );
        }
    }

    private static List<INamedTypeSymbol> Combine(List<INamedTypeSymbol> types, List<INamedTypeSymbol> referenced)
    {
        if (referenced.Count == 0)
            return types;
        var seen = new HashSet<INamedTypeSymbol>(types, SymbolEqualityComparer.Default);
        var combined = new List<INamedTypeSymbol>(types);
        combined.AddRange(referenced.Where(seen.Add));
        combined.Sort(
            static (left, right) =>
                string.CompareOrdinal(
                    left.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                    right.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
                )
        );
        return combined;
    }

    private static string Build(
        Compilation compilation,
        IMethodSymbol method,
        List<INamedTypeSymbol> types,
        string identifier
    )
    {
        var target = method.ContainingType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        var sb = new StringBuilder();
        sb.AppendLine("#nullable enable");
        sb.AppendLine();
        sb.AppendLine("namespace Vigilance.Generated");
        sb.AppendLine("{");
        sb.AppendLine($"    internal static class {identifier}");
        sb.AppendLine("    {");
        sb.AppendLine("#pragma warning disable CA2255");
        sb.AppendLine("        [global::System.Runtime.CompilerServices.ModuleInitializer]");
        sb.AppendLine("#pragma warning restore CA2255");
        sb.AppendLine("        internal static void Initialize()");
        sb.AppendLine("        {");
        foreach (var type in types)
        {
            if (!IsRegistrable(compilation, type, method.TypeParameters[0]))
                continue;
            var argument = type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            var call = $"{(method.ReturnsVoid ? "" : "_ = ")}{target}.{Escape(method.Name)}<{argument}>();";
            sb.AppendLine("            try");
            sb.AppendLine("            {");
            sb.AppendLine($"                {call}");
            sb.AppendLine("            }");
            sb.AppendLine("            catch (global::System.Exception exception)");
            sb.AppendLine("            {");
            sb.AppendLine($"                {LogTypeName}.Error(exception);");
            sb.AppendLine("            }");
        }

        sb.AppendLine("        }");
        sb.AppendLine("    }");
        sb.AppendLine("}");
        sb.AppendLine();
        sb.AppendLine("#nullable restore");
        return sb.ToString();
    }

    private static IMethodSymbol? Validate(
        SourceProductionContext context,
        Compilation compilation,
        IMethodSymbol method,
        bool report
    )
    {
        var attribute = Attribute(method);
        if (attribute is null)
            return null;
        var location = report ? GetLocation(method, attribute) : Location.None;
        var name = method.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat);
        var valid = true;
        if (!method.IsStatic)
        {
            Report(context, report, Diagnostics.GenericRegistryMethodMustBeStatic, location, name);
            valid = false;
        }

        if (method.TypeParameters.Length != 1)
        {
            Report(
                context,
                report,
                Diagnostics.GenericRegistryMethodMustHaveOneTypeParameter,
                location,
                name,
                method.TypeParameters.Length
            );
            valid = false;
        }

        if (method.Parameters.Any(static parameter => !parameter.IsOptional && !parameter.IsParams))
        {
            Report(context, report, Diagnostics.GenericRegistryMethodMustNotRequireParameters, location, name);
            valid = false;
        }

        if (IsGeneric(method.ContainingType))
        {
            Report(
                context,
                report,
                Diagnostics.GenericRegistryContainingTypeMustNotBeGeneric,
                location,
                name,
                method.ContainingType.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat)
            );
            valid = false;
        }

        if (!valid)
            return null;
        if (compilation.GetTypeByMetadataName(ModuleInitializerMetadataName) is null)
        {
            Report(context, report, Diagnostics.GenericRegistryModuleInitializerUnsupported, location, name);
            return null;
        }

        var parameter = method.TypeParameters[0];
        if (!IsConstrained(parameter))
            Report(
                context,
                report,
                Diagnostics.GenericRegistryTypeParameterShouldBeConstrained,
                location,
                name,
                parameter.Name
            );
        if (!IsVisible(method))
            Report(
                context,
                report,
                Diagnostics.GenericRegistryMethodShouldBeVisible,
                location,
                name,
                compilation.Assembly.Name
            );
        return method;
    }

    private static void Report(
        SourceProductionContext context,
        bool report,
        DiagnosticDescriptor descriptor,
        Location location,
        params object?[] arguments
    )
    {
        if (report)
            context.ReportDiagnostic(Diagnostic.Create(descriptor, location, arguments));
    }

    private static Location GetLocation(IMethodSymbol method, AttributeData attribute)
    {
        var reference = attribute.ApplicationSyntaxReference;
        if (reference is not null)
            return Location.Create(reference.SyntaxTree, reference.Span);
        return method.Locations.FirstOrDefault() ?? Location.None;
    }

    private static bool Sees(IAssemblySymbol assembly, IAssemblySymbol declaring)
    {
        return SymbolEqualityComparer.Default.Equals(assembly, declaring)
            || assembly
                .Modules.SelectMany(module => module.ReferencedAssemblies)
                .Any(identity => identity.Name == declaring.Identity.Name);
    }

    private static IEnumerable<IMethodSymbol> Registered(INamedTypeSymbol type)
    {
        foreach (var member in type.GetMembers())
            if (member is IMethodSymbol { MethodKind: MethodKind.Ordinary } method && Attribute(method) is not null)
                yield return method;
    }

    private static AttributeData? Attribute(IMethodSymbol method)
    {
        foreach (var attribute in method.GetAttributes())
            if (
                attribute.AttributeClass is { Name: "GenericRegistryAttribute" } attributeClass
                && attributeClass.ContainingNamespace?.ToDisplayString() == "Vigilance.Core"
            )
                return attribute;
        return null;
    }

    private static void Collect(INamespaceSymbol @namespace, List<INamedTypeSymbol> types)
    {
        foreach (var member in @namespace.GetMembers())
            switch (member)
            {
                case INamespaceSymbol nested:
                    Collect(nested, types);
                    break;
                case INamedTypeSymbol type:
                    Collect(type, types);
                    break;
            }
    }

    private static void Collect(INamedTypeSymbol type, List<INamedTypeSymbol> types)
    {
        types.Add(type);
        foreach (var nested in type.GetTypeMembers())
            Collect(nested, types);
    }

    private static void CollectConstructed(Compilation compilation, List<INamedTypeSymbol> types)
    {
        var seen = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);
        foreach (var tree in compilation.SyntaxTrees)
        {
            SemanticModel? model = null;
            foreach (var node in tree.GetRoot().DescendantNodes())
            {
                if (node is not GenericNameSyntax name)
                    continue;
                model ??= compilation.GetSemanticModel(tree);
                if (model.GetSymbolInfo(name).Symbol is not INamedTypeSymbol type)
                    continue;
                if (type.TypeKind == TypeKind.Error || !type.IsGenericType || IsOpen(type))
                    continue;
                if (seen.Add(type))
                    types.Add(type);
            }
        }
    }

    private static bool IsRegistrable(Compilation compilation, INamedTypeSymbol type, ITypeParameterSymbol parameter)
    {
        return IsCandidate(type, parameter)
            && IsAccessible(type)
            && compilation.IsSymbolAccessibleWithin(type, compilation.Assembly)
            && Satisfies(compilation, type, parameter);
    }

    private static bool IsCandidate(INamedTypeSymbol type, ITypeParameterSymbol parameter)
    {
        if (!type.CanBeReferencedByName || type.IsImplicitlyDeclared)
            return false;
        if (
            type.TypeKind
            is not (TypeKind.Class or TypeKind.Struct or TypeKind.Interface or TypeKind.Enum or TypeKind.Delegate)
        )
            return false;
        if (type.IsStatic || IsOpen(type))
            return false;
        return !type.IsRefLikeType || parameter.AllowsRefLikeType;
    }

    private static void ReportInaccessible(
        SourceProductionContext context,
        Compilation compilation,
        List<IMethodSymbol> registries,
        List<INamedTypeSymbol> types
    )
    {
        var reported = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);
        foreach (var type in types)
        {
            if (Inaccessible(type) is not { } inaccessible)
                continue;
            if (Source(inaccessible) is not { } location)
                continue;
            foreach (var method in registries)
            {
                var parameter = method.TypeParameters[0];
                if (!IsConstrained(parameter) || !IsCandidate(type, parameter))
                    continue;
                if (!Satisfies(compilation, type, parameter))
                    continue;
                if (!reported.Add(type))
                    break;
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        Diagnostics.GenericRegistryTypeMustBeAccessible,
                        location,
                        type.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat),
                        method.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat),
                        inaccessible.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat)
                    )
                );
                break;
            }
        }
    }

    private static Location? Source(ISymbol symbol)
    {
        return Enumerable.FirstOrDefault(symbol.Locations, location => location.IsInSource);
    }

    private static bool Satisfies(Compilation compilation, INamedTypeSymbol type, ITypeParameterSymbol parameter)
    {
        if (parameter.HasReferenceTypeConstraint && !type.IsReferenceType)
            return false;
        if (parameter.HasValueTypeConstraint && !type.IsValueType)
            return false;
        if (parameter.HasUnmanagedTypeConstraint && !type.IsUnmanagedType)
            return false;
        if (parameter.HasConstructorConstraint && !HasDefaultConstructor(type))
            return false;
        foreach (var constraint in parameter.ConstraintTypes)
        {
            var substituted = Substitute(compilation, constraint, parameter, type);
            if (substituted is not INamedTypeSymbol named || !IsAssignable(type, named))
                return false;
        }

        return true;
    }

    private static bool IsAssignable(INamedTypeSymbol type, INamedTypeSymbol constraint)
    {
        if (constraint.TypeKind == TypeKind.Interface)
            return type.AllInterfaces.Any(@interface => SymbolEqualityComparer.Default.Equals(@interface, constraint))
                || SymbolEqualityComparer.Default.Equals(type, constraint);

        for (var current = type; current is not null; current = current.BaseType)
            if (SymbolEqualityComparer.Default.Equals(current, constraint))
                return true;
        return false;
    }

    private static ITypeSymbol? Substitute(
        Compilation compilation,
        ITypeSymbol type,
        ITypeParameterSymbol parameter,
        ITypeSymbol argument
    )
    {
        if (!Contains(type, parameter))
            return type;
        switch (type)
        {
            case ITypeParameterSymbol other when SymbolEqualityComparer.Default.Equals(other, parameter):
                return argument;
            case IArrayTypeSymbol array:
                var element = Substitute(compilation, array.ElementType, parameter, argument);
                return element is null ? null : compilation.CreateArrayTypeSymbol(element, array.Rank);
            case INamedTypeSymbol { IsGenericType: true, ContainingType: null } named:
                var arguments = new ITypeSymbol[named.TypeArguments.Length];
                for (var i = 0; i < arguments.Length; i++)
                {
                    var substituted = Substitute(compilation, named.TypeArguments[i], parameter, argument);
                    if (substituted is null)
                        return null;
                    arguments[i] = substituted;
                }

                return named.ConstructedFrom.Construct(arguments);
            default:
                return null;
        }
    }

    private static bool Contains(ITypeSymbol type, ITypeParameterSymbol parameter)
    {
        return type switch
        {
            ITypeParameterSymbol other => SymbolEqualityComparer.Default.Equals(other, parameter),
            IArrayTypeSymbol array => Contains(array.ElementType, parameter),
            INamedTypeSymbol named => named.TypeArguments.Any(argument => Contains(argument, parameter)),
            _ => false,
        };
    }

    private static bool HasDefaultConstructor(INamedTypeSymbol type)
    {
        if (type.IsValueType)
            return true;
        if (type.IsAbstract || type.TypeKind == TypeKind.Interface)
            return false;
        return type.InstanceConstructors.Any(static constructor =>
            constructor is { Parameters.Length: 0, DeclaredAccessibility: Accessibility.Public }
        );
    }

    private static bool IsConstrained(ITypeParameterSymbol parameter)
    {
        return parameter.ConstraintTypes.Length > 0
            || parameter.HasReferenceTypeConstraint
            || parameter.HasValueTypeConstraint
            || parameter.HasUnmanagedTypeConstraint
            || parameter.HasNotNullConstraint;
    }

    private static bool IsGeneric(INamedTypeSymbol type)
    {
        for (var current = type; current is not null; current = current.ContainingType)
            if (current.TypeParameters.Length > 0)
                return true;
        return false;
    }

    private static bool IsOpen(INamedTypeSymbol type)
    {
        if (type.IsUnboundGenericType)
            return true;
        for (var current = type; current is not null; current = current.ContainingType)
            if (Enumerable.Any(current.TypeArguments, IsOpen))
                return true;
        return false;
    }

    private static bool IsOpen(ITypeSymbol type)
    {
        return type switch
        {
            ITypeParameterSymbol => true,
            IArrayTypeSymbol array => IsOpen(array.ElementType),
            IPointerTypeSymbol pointer => IsOpen(pointer.PointedAtType),
            INamedTypeSymbol named => IsOpen(named),
            _ => false,
        };
    }

    private static bool IsAccessible(INamedTypeSymbol type)
    {
        return Inaccessible(type) is null;
    }

    private static INamedTypeSymbol? Inaccessible(INamedTypeSymbol type)
    {
        for (var current = type; current is not null; current = current.ContainingType)
        {
            if (
                current.DeclaredAccessibility is not (Accessibility.Public or Accessibility.Internal)
                || current.IsFileLocal
            )
                return current;
            foreach (var argument in current.TypeArguments)
                if (argument is INamedTypeSymbol named && Inaccessible(named) is { } inaccessible)
                    return inaccessible;
        }

        return null;
    }

    private static bool IsVisible(IMethodSymbol method)
    {
        if (method.DeclaredAccessibility != Accessibility.Public)
            return false;
        for (var current = method.ContainingType; current is not null; current = current.ContainingType)
            if (current.DeclaredAccessibility != Accessibility.Public)
                return false;
        return true;
    }

    private static string Escape(string name)
    {
        return SyntaxFacts.GetKeywordKind(name) != SyntaxKind.None ? "@" + name : name;
    }

    private static string Hint(IMethodSymbol method, HashSet<string> hints)
    {
        var sb = new StringBuilder("GenericRegistry_");
        foreach (var ch in $"{method.ContainingType.ToDisplayString()}_{method.Name}")
            sb.Append(char.IsLetterOrDigit(ch) ? ch : '_');
        var hint = sb.ToString();
        if (hints.Add(hint))
            return hint;
        for (var index = 1; ; index++)
            if (hints.Add($"{hint}_{index}"))
                return $"{hint}_{index}";
    }
}
