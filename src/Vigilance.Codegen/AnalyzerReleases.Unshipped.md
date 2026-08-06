; Unshipped analyzer
release ; https://github.com/dotnet/roslyn-analyzers/blob/main/src/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md

### New Rules

| Rule ID | Category  | Severity | Notes                                                          |
|---------|-----------|----------|----------------------------------------------------------------|
| VIG0001 | Vigilance | Error    | GenericRegistry method must be static                          |
| VIG0002 | Vigilance | Error    | GenericRegistry method must have exactly one type parameter    |
| VIG0003 | Vigilance | Error    | GenericRegistry method must not require parameters             |
| VIG0004 | Vigilance | Error    | GenericRegistry method must not be declared in a generic type  |
| VIG0005 | Vigilance | Warning  | GenericRegistry type parameter should be constrained           |
| VIG0006 | Vigilance | Warning  | GenericRegistry method is not visible outside its assembly     |
| VIG0007 | Vigilance | Warning  | Module initializers are not supported by the target framework  |
| VIG0008 | Vigilance | Warning  | Type satisfying a GenericRegistry constraint is not accessible |
| VIG0010 | Vigilance | Error    | ValueWrapper type must be partial                              |
| VIG0011 | Vigilance | Error    | ValueWrapper containing type must be partial                   |
| VIG0012 | Vigilance | Error    | ValueWrapper type must not be static                           |
| VIG0013 | Vigilance | Error    | ValueWrapper value type is invalid                             |
| VIG0014 | Vigilance | Error    | ValueWrapper field name is invalid                             |
| VIG0015 | Vigilance | Error    | ValueWrapper wrapping is circular                              |
