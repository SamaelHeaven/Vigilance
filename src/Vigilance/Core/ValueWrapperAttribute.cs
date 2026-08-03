namespace Vigilance.Core;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false)]
public sealed class ValueWrapperAttribute<TValue>(string fieldName = "Value") : Attribute
{
    public string FieldName { get; } = fieldName;
}
