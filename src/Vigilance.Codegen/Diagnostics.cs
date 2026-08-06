using Microsoft.CodeAnalysis;

namespace Vigilance.Codegen;

public static class Diagnostics
{
    private const string Category = "Vigilance";

    public static readonly DiagnosticDescriptor GenericRegistryMethodMustBeStatic = new(
        "VIG0001",
        "GenericRegistry method must be static",
        "Method '{0}' is marked with [GenericRegistry] but is not static",
        Category,
        DiagnosticSeverity.Error,
        true,
        "A [GenericRegistry] method is invoked from a module initializer and therefore cannot require an instance."
    );

    public static readonly DiagnosticDescriptor GenericRegistryMethodMustHaveOneTypeParameter = new(
        "VIG0002",
        "GenericRegistry method must have exactly one type parameter",
        "Method '{0}' is marked with [GenericRegistry] but has {1} type parameters instead of 1",
        Category,
        DiagnosticSeverity.Error,
        true,
        "A [GenericRegistry] method is invoked once per matching type and therefore must take exactly one type parameter."
    );

    public static readonly DiagnosticDescriptor GenericRegistryMethodMustNotRequireParameters = new(
        "VIG0003",
        "GenericRegistry method must not require parameters",
        "Method '{0}' is marked with [GenericRegistry] but requires parameters; only optional parameters are allowed",
        Category,
        DiagnosticSeverity.Error,
        true,
        "A [GenericRegistry] method is invoked without arguments from a module initializer."
    );

    public static readonly DiagnosticDescriptor GenericRegistryContainingTypeMustNotBeGeneric = new(
        "VIG0004",
        "GenericRegistry method must not be declared in a generic type",
        "Method '{0}' is marked with [GenericRegistry] but is declared in generic type '{1}'",
        Category,
        DiagnosticSeverity.Error,
        true,
        "A [GenericRegistry] method must be callable without supplying type arguments for its containing type."
    );

    public static readonly DiagnosticDescriptor GenericRegistryTypeParameterShouldBeConstrained = new(
        "VIG0005",
        "GenericRegistry type parameter should be constrained",
        "Type parameter '{1}' of method '{0}' has no constraint; every type of this assembly and of the assemblies it is compiled against will be registered",
        Category,
        DiagnosticSeverity.Warning,
        true,
        "A registry declared here registers the types of the referenced assemblies as well as its own, so leaving the type parameter unconstrained registers far more than the types of this assembly. Constrain it to a base class or an interface to select the types to register."
    );

    public static readonly DiagnosticDescriptor GenericRegistryMethodShouldBeVisible = new(
        "VIG0006",
        "GenericRegistry method is not visible outside its assembly",
        "Method '{0}' is marked with [GenericRegistry] but is not publicly accessible; assemblies referencing '{1}' will not register their own types with it",
        Category,
        DiagnosticSeverity.Warning,
        true,
        "A registry registers the types of its own assembly and of the assemblies it is compiled against, but each assembly compiled later registers its own types on its own and can only do so when it can call the method. Make the method and its containing type public to reach them."
    );

    public static readonly DiagnosticDescriptor GenericRegistryModuleInitializerUnsupported = new(
        "VIG0007",
        "Module initializers are not supported by the target framework",
        "Method '{0}' is marked with [GenericRegistry] but 'System.Runtime.CompilerServices.ModuleInitializerAttribute' is unavailable; no registration will be generated",
        Category,
        DiagnosticSeverity.Warning,
        true,
        "Module initializers require .NET 5.0 or later."
    );

    public static readonly DiagnosticDescriptor GenericRegistryTypeMustBeAccessible = new(
        "VIG0008",
        "Type satisfying a GenericRegistry constraint is not accessible",
        "Type '{0}' satisfies the constraint of [GenericRegistry] method '{1}' but cannot be registered because '{2}' is not accessible outside its declaration",
        Category,
        DiagnosticSeverity.Warning,
        true,
        "Registration is emitted in a module initializer that names the type, so a private, protected or file local type cannot be registered. Declare the type, every type enclosing it and every type argument it is constructed with, at least internal."
    );

    public static readonly DiagnosticDescriptor ValueWrapperTypeMustBePartial = new(
        "VIG0010",
        "ValueWrapper type must be partial",
        "Type '{0}' is marked with [ValueWrapper] but is not declared partial",
        Category,
        DiagnosticSeverity.Error,
        true,
        "The wrapper members are emitted in a separate partial declaration of the type."
    );

    public static readonly DiagnosticDescriptor ValueWrapperContainingTypeMustBePartial = new(
        "VIG0011",
        "ValueWrapper containing type must be partial",
        "Type '{0}' is marked with [ValueWrapper] but its containing type '{1}' is not declared partial",
        Category,
        DiagnosticSeverity.Error,
        true,
        "Every type enclosing a [ValueWrapper] type must be declared partial."
    );

    public static readonly DiagnosticDescriptor ValueWrapperTypeMustNotBeStatic = new(
        "VIG0012",
        "ValueWrapper type must not be static",
        "Type '{0}' is marked with [ValueWrapper] but is static",
        Category,
        DiagnosticSeverity.Error,
        true,
        "A static type cannot hold the wrapped value."
    );

    public static readonly DiagnosticDescriptor ValueWrapperValueTypeInvalid = new(
        "VIG0013",
        "ValueWrapper value type is invalid",
        "Type '{0}' is marked with [ValueWrapper] but the wrapped type could not be resolved",
        Category,
        DiagnosticSeverity.Error,
        true,
        "The type passed to [ValueWrapper] must be a resolvable class or struct."
    );

    public static readonly DiagnosticDescriptor ValueWrapperFieldNameInvalid = new(
        "VIG0014",
        "ValueWrapper field name is invalid",
        "Type '{0}' is marked with [ValueWrapper] but '{1}' is not a valid identifier",
        Category,
        DiagnosticSeverity.Error,
        true,
        "The field name given to [ValueWrapper] is used to declare the field holding the wrapped value."
    );

    public static readonly DiagnosticDescriptor ValueWrapperCircularWrapping = new(
        "VIG0015",
        "ValueWrapper wrapping is circular",
        "Type '{0}' is marked with [ValueWrapper] but wraps itself through '{1}'",
        Category,
        DiagnosticSeverity.Error,
        true,
        "A [ValueWrapper] type cannot wrap, directly or indirectly, a type that wraps it back."
    );
}
