namespace Vigilance.Core;

public interface IAddImmutableComponent;

public interface ISetImmutableComponent;

public interface IWriteImmutableComponent;

public interface IRemoveImmutableComponent;

public interface IImmutableComponent
    : IAddImmutableComponent,
        ISetImmutableComponent,
        IWriteImmutableComponent,
        IRemoveImmutableComponent;
