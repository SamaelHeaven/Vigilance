namespace Vigilance.Core;

internal record struct Parent(ulong FirstChildId, ulong LastChildId) : IHiddenComponent;
