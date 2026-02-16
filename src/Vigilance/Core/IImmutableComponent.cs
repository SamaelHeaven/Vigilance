namespace Vigilance.Core;

public interface ISetImmutableComponent;

public interface IWriteImmutableComponent;

public interface IRemoveImmutableComponent;

public interface IImmutableComponent : ISetImmutableComponent, IWriteImmutableComponent, IRemoveImmutableComponent;
